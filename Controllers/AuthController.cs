using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UserManagement.Data;
using UserManagement.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using UserManagement.Helpers;

namespace UserManagement.Controllers
{
  [Authorize]   // Require authentication by default for all endpoints in this controller
  [ApiController]  // Enables automatic model validation and better routing
  [Route("[controller]")]  // Route pattern; this will be '/auth' because the controller is named AuthController
  public class AuthController : ControllerBase
  {
    private readonly DataContextDapper _dapper;  // Dapper database context
    private readonly AuthHelper _authHelper;

    public AuthController(IConfiguration config)   // Initialize Dapper with config
    {
      _dapper = new DataContextDapper(config);
      _authHelper = new AuthHelper(config);
    }



    [AllowAnonymous]  // Allow unauthenticated access for registration
    [HttpPost("Register")]  // POST /auth/register
    public IActionResult Register(UserForRegistrationDto userForRegistration)
    {
      // Check if password and password confirmation match
      if (userForRegistration.Password != userForRegistration.PasswordConfirm)
      {
        throw new Exception("Password do not match!");   // If passwords don't match, throw an error
      }
      
      // Check if email already exists
      string sqlCheckUserExists = "SELECT Email FROM TutorialAppSchema.Auth WHERE Email = @Email";
      var emailParam = new { Email = userForRegistration.Email };
      IEnumerable<string> existingUsers = _dapper.LoadDataWithParameters<string>(sqlCheckUserExists, emailParam);
      if (existingUsers.Any())
      {
          return Conflict("A user with this email already exists.");
      }

      // Generate salt
      byte[] passwordSalt = new byte[128 / 8];
      using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
      {
        rng.GetNonZeroBytes(passwordSalt);  // Fill the salt with non-zero random bytes
      }

      // Call the GetPasswordHash() to hash the password
      byte[] passwordHash = _authHelper.GetPasswordHash(userForRegistration.Password, passwordSalt);

      // Insert into Auth table
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

      var userParametersForAuth = new List<SqlParameter>
        {
          new SqlParameter("@Email", SqlDbType.NVarChar) { Value = userForRegistration.Email },
          new SqlParameter("@PasswordHash", SqlDbType.VarBinary) { Value = passwordHash },
          new SqlParameter("@PasswordSalt", SqlDbType.VarBinary) { Value = passwordSalt }
        };

      // Execute the insert command with parameters
      if (!_dapper.ExecuteSqlWithParameters(sqlAddAuth, userParametersForAuth))
      {
        throw new Exception("Failed to register user.");   // If insert failed, throw an error
      }
      
      string sqlAddUser = @"INSERT INTO TutorialAppSchema.Users (
              [FirstName],
              [LastName],
              [Email],
              [Gender],
              [Active] 
            ) VALUES (@FirstName, @LastName, @Email, @Gender, @Active)";

      var userParameters = new List<SqlParameter>
      {
        new SqlParameter("@FirstName", SqlDbType.NVarChar) { Value = userForRegistration.FirstName },
        new SqlParameter("@LastName", SqlDbType.NVarChar) { Value = userForRegistration.LastName },
        new SqlParameter("@Email", SqlDbType.NVarChar) { Value = userForRegistration.Email },
        new SqlParameter("@Gender", SqlDbType.NVarChar) { Value = userForRegistration.Gender },
        new SqlParameter("@Active", SqlDbType.Bit) { Value = 1 }
      };
      
      if (_dapper.ExecuteSqlWithParameters(sqlAddUser, userParameters))
      {
        return Ok("User registered successfully.");  // Return 200 OK if registration succeeded
      }
      throw new Exception("Failed to add user.");
    }


      
    [AllowAnonymous]
    [HttpPost("Login")]  // POST /auth/login
    public IActionResult Login(UserForLoginDto userForLogin)
    {
      //1. Query for password hash and salt for this email
      string sqlForHashAndSalt = @"SELECT 
            [PasswordHash],
            [PasswordSalt] FROM TutorialAppSchema.Auth WHERE Email = @Email";
      var parameter = new { Email = userForLogin.Email };
      UserForLoginConfirmationDto userForLoginConfirmation = _dapper.LoadDataSingleWithParameters<UserForLoginConfirmationDto>(sqlForHashAndSalt, parameter);
      
      if (userForLoginConfirmation == null)
      {
        return StatusCode(401, "Invalid Email");  // Email not found
      }

      //2. Recalculate hash with the provided password and stored salt
      byte[] passwordHash = _authHelper.GetPasswordHash(userForLogin.Password, userForLoginConfirmation.PasswordSalt);

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
      int userId = _dapper.LoadDataSingleWithParameters<int>(userIdSql, parameter);

      //5. Return JWT token on successful login
      return Ok(new Dictionary<string, string>
      {
        {"token", _authHelper.CreateToken(userId)}
      });
    }



    [HttpGet("RefreshToken")]  // GET /auth/refreshToken
    public IActionResult RefreshToken()
    {
      // 1. Extract user ID from JWT claims
      //string userId = User.FindFirst("userId")?.Value + "";
      var userIdClaim = User.FindFirst("userId")?.Value;
      if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
      {
          return Unauthorized("Invalid or missing user ID claim.");
      }

      // 2. Confirm user exists
      string userIdSql = "SELECT userId FROM TutorialAppSchema.Users WHERE UserId = @UserId";
      var parameter = new { UserId = userId };
      int? userIdFromDb = _dapper.LoadDataSingleWithParameters<int?>(userIdSql, parameter);
      if (!userIdFromDb.HasValue)
      {
        return Unauthorized("User not found.");
      }

      // 3. Return new JWT token
      return Ok(new Dictionary<string, string>
      {
        {"token", _authHelper.CreateToken(userIdFromDb.Value)}
      });
    }
  }
}
