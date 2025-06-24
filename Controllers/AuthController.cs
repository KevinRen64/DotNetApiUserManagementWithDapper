using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DotNetApi.Data;
using DotNetApi.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace DotNetApi.Controllers
{
  [Authorize]   // Require authentication by default for all endpoints in this controller
  [ApiController]  // Enables automatic model validation and better routing
  [Route("[controller]")]  // Route pattern; this will be '/auth' because the controller is named AuthController
  public class AuthController : ControllerBase
  {
    private readonly DataContextDapper _dapper;  // Dapper database context
    private readonly IConfiguration _config;  // Access to appsettings.json
    public AuthController(IConfiguration config)   // Initialize Dapper with config
    {
      _dapper = new DataContextDapper(config);
      _config = config;
    }

    [AllowAnonymous]  // Allow unauthenticated access for registration
    [HttpPost("Register")]  // POST /auth/register
    public IActionResult Register(UserForRegistrationDto userForRegistration)
    {
      // Check if password and password confirmation match
      if (userForRegistration.Password == userForRegistration.PasswordConfirm)
      {
        // SQL query to check if the email already exists in the database
        //string sqlCheckUserExists = "SELECT Email FROM TutorialAppSchema.Auth WHERE Email = '" + userForRegistration.Email + "'";
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        string sqlCheckUserExists = "SELECT Email FROM TutorialAppSchema.Auth WHERE Email = @Email";
        var emailParam = new { Email = userForRegistration.Email };
        // Execute the SQL query using Dapper and get list of existing emails
        IEnumerable<string> existingUsers = _dapper.LoadDataWithParameters<string>(sqlCheckUserExists, emailParam);
        // If no matching email found, proceed with registration
        if (existingUsers.Count() == 0)
        {
          // Create a random salt using a secure random number generator
          byte[] passwordSalt = new byte[128 / 8];
          using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
          {
            rng.GetNonZeroBytes(passwordSalt);  // Fill the salt with non-zero random bytes
          }

          // Call the GetPasswordHash() to hash the password by combining it with the salt and app secret key
          byte[] passwordHash = GetPasswordHash(userForRegistration.Password, passwordSalt);

          // SQL command to insert the new user into the Auth table
          string sqlAddAuth = @"
          INSERT INTO TutorialAppSchema.Auth (
              [Email],
              [PasswordHash],
              [PasswordSalt]
              ) VALUES (
              @Email,
              @PasswordHash,
              @PasswordSalt
              )";

          // Prepare the SQL parameters to prevent SQL injection for the hash and salt
          List<SqlParameter> sqlParameters = new List<SqlParameter>();

          // Create SqlParameter for password salt (binary data)
          SqlParameter passwordSaltParameter = new SqlParameter("@PasswordSalt", SqlDbType.VarBinary);
          passwordSaltParameter.Value = passwordSalt;

          // Create SqlParameter for password hash (binary data)
          SqlParameter passwordHashParameter = new SqlParameter("@PasswordHash", SqlDbType.VarBinary);
          passwordHashParameter.Value = passwordHash;

          // Create SqlParameter for email (NVarChar)
          SqlParameter emailParameter = new SqlParameter("@Email", SqlDbType.NVarChar);
          emailParameter.Value = userForRegistration.Email;

          // Add parameters to the list
          sqlParameters.Add(passwordSaltParameter);
          sqlParameters.Add(passwordHashParameter);
          sqlParameters.Add(emailParameter);



          // Execute the insert command with parameters
          if (_dapper.ExecuteSqlWithParameters(sqlAddAuth, sqlParameters))
          {
            string sqlAddUser = @"INSERT INTO TutorialAppSchema.Users (
                    [FirstName],
                    [LastName],
                    [Email],
                    [Gender],
                    [Active] 
                  ) VALUES ( " +
                  "'" + userForRegistration.FirstName +
                  "', '" + userForRegistration.LastName +
                  "', '" + userForRegistration.Email +
                  "', '" + userForRegistration.Gender +
              "', 1)";
            if (_dapper.ExecuteSql(sqlAddUser))
            {
              return Ok();  // Return 200 OK if registration succeeded
            }
            throw new Exception("Failed to add user.");
          }
          throw new Exception("Failed to register user.");   // If insert failed, throw an error
        }
        throw new Exception("User with this email already exist!");  // If the user already exists, throw an error
      }
      throw new Exception("Password do not match!");   // If passwords don't match, throw an error

    }


    [AllowAnonymous]
    [HttpPost("Login")]  // POST /auth/login
    public IActionResult Login(UserForLoginDto userForLogin)
    {
      var parameters = new { Email = userForLogin.Email };

      //1. Query for password hash and salt for this email
      string sqlForHashAndSalt = @"SELECT 
            [PasswordHash],
            [PasswordSalt] FROM TutorialAppSchema.Auth WHERE Email = @Email";

      UserForLoginConfirmationDto userForLoginConfirmation = _dapper.LoadDataSingleWithParameters<UserForLoginConfirmationDto>(sqlForHashAndSalt, parameters);
      
      if (userForLoginConfirmation == null)
      {
        return StatusCode(401, "Invalid Email");  // Email not found
      }

      //2. Recalculate hash with the provided password and stored salt
      byte[] passwordHash = GetPasswordHash(userForLogin.Password, userForLoginConfirmation.PasswordSalt);

      //3. Compare hashes byte by byte
      for (int index = 0; index < passwordHash.Length; index++)
      {
        // If any byte does not match, return HTTP 401 Unauthorized with "Incorrect Password"
        if (passwordHash[index] != userForLoginConfirmation.PasswordHash[index])
        {
          return StatusCode(401, "Incorrect Password");
        }
      }

      //4. Get the user's ID
      string userIdSql = "SELECT UserId FROM TutorialAppSchema.Users WHERE Email = @Email";
      int userId = _dapper.LoadDataSingleWithParameters<int>(userIdSql, parameters);

      //5. Return JWT token on successful login
      return Ok(new Dictionary<string, string>
      {
        {"token", CreateToken(userId)}
      });
    }



    [HttpGet("RefreshToken")]  // GET /auth/refreshToken
    public IActionResult RefreshToken()
    {
      // 1. Extract user ID from JWT claims
      string userId = User.FindFirst("userId")?.Value + "";

      // 2. Confirm user exists
      string userIdSql = "SELECT userId FROM TutorialAppSchema.Users WHERE UserId = " + userId;
      int userIdFromDb = _dapper.LoadDataSingle<int>(userIdSql);

      // 3. Return new JWT token
      return Ok(new Dictionary<string, string>
      {
        {"token", CreateToken(userIdFromDb)}
      });
    }




    // Helper method to hash a password with salt and app secret key using PBKDF2
    private byte[] GetPasswordHash(string password, byte[] passwordSalt)
    {
      // Combine salt and a secret password key from appsettings.json
      string passwordSaltPlusString = _config.GetSection("AppSettings:PasswordKey").Value + Convert.ToBase64String(passwordSalt);

      // PBKDF2 hashing with HMACSHA256
      return KeyDerivation.Pbkdf2(
        password: password,
        salt: Encoding.ASCII.GetBytes(passwordSaltPlusString),  
        prf: KeyDerivationPrf.HMACSHA256,  
        iterationCount: 1000000,  // Secure number of iterations
        numBytesRequested: 256 / 8  // 32-byte output
      );
    }


    // Method to create a JWT token for a given userId
    private string CreateToken(int userId)
    {
      // 1. Define claims for the token
      Claim[] claims = new Claim[] {
        new Claim("userId", userId.ToString())  // Custom userId claim
      };

      // 2. Create security key using secret from config
      string? tokenKeyString = _config.GetSection("Appsettings:TokenKey").Value;
      SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(
          Encoding.UTF8.GetBytes(
            tokenKeyString != null ? tokenKeyString : ""
          )
        );

      // 3. Create signing credentials
      SigningCredentials credentials = new SigningCredentials(
          tokenKey,
          SecurityAlgorithms.HmacSha512Signature
        );

      // 4. Define token details
      SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor()
      {
        Subject = new ClaimsIdentity(claims),
        SigningCredentials = credentials,
        Expires = DateTime.Now.AddDays(1)   // Token expires in 1 day
      };
      
      // 5. Generate token
      JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
      SecurityToken token = tokenHandler.CreateToken(descriptor);

      return tokenHandler.WriteToken(token);  // Return token as string
    }
  }
}
