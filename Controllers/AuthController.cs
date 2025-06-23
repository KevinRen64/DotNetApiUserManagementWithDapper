using System.Data;
using System.Security.Cryptography;
using System.Text;
using DotNetApi.Data;
using DotNetApi.Dtos;
using DotNetApi.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace DotNetApi.Controllers
{
  public class AuthController : ControllerBase
  {
    private readonly DataContextDapper _dapper;
    private readonly IConfiguration _config;
    public AuthController(IConfiguration config)
    {
      _dapper = new DataContextDapper(config);
      _config = config;
    }


    [HttpPost("Register")]
    public IActionResult Register(UserForRegistrationDto userForRegistration)
    {
      // Check if password and password confirmation match
      if (userForRegistration.Password == userForRegistration.PasswordConfirm)
      {
        // SQL query to check if the email already exists in the database
        string sqlCheckUserExists = "SELECT Email FROM TutorialAppSchema.Auth WHERE Email = '" + userForRegistration.Email + "'";

        // Execute the SQL query using Dapper and get list of existing emails
        IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists);
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
          string sqlAddAuth = @"INSERT INTO TutorialAppSchema.Auth([Email],
              [PasswordHash],
              [PasswordSalt]) VALUES ('" + userForRegistration.Email +
              "', @PasswordHash, @PasswordSalt)";

          /*
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
          */

          // Prepare the SQL parameters to prevent SQL injection for the hash and salt
          List<SqlParameter> sqlParameters = new List<SqlParameter>();

          // Create SqlParameter for password salt (binary data)
          SqlParameter passwordSaltParameter = new SqlParameter("@PasswordSalt", SqlDbType.VarBinary);
          passwordSaltParameter.Value = passwordSalt;

          // Create SqlParameter for password hash (binary data)
          SqlParameter passwordHashParameter = new SqlParameter("@PasswordHash", SqlDbType.VarBinary);
          passwordHashParameter.Value = passwordHash;

          // Create SqlParameter for email (NVarChar)
          // SqlParameter emailParameter = new SqlParameter("@Email", SqlDbType.NVarChar);
          // emailParameter.Value = userForRegistration.email;

          // Add parameters to the list
          sqlParameters.Add(passwordSaltParameter);
          sqlParameters.Add(passwordHashParameter);
          //sqlParameters.Add(emailParameter);

          

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



    [HttpPost("Login")]
    public IActionResult Login(UserForLoginDto userForLogin)
    {
      // SQL query to retrieve the password hash and salt for the given email
      string sqlForHashAndSalt = @"SELECT 
            [PasswordHash],
            [PasswordSalt] FROM TutorialAppSchema.Auth WHERE Email = '" +
            userForLogin.Email + "'";
      
      // Execute the query and get a single UserForLoginConfirmationDto containing hash and salt
      UserForLoginConfirmationDto userForLoginConfirmation = _dapper.LoadDataSingle<UserForLoginConfirmationDto>(sqlForHashAndSalt);

      // Recompute the password hash with the provided password and stored salt
      byte[] passwordHash = GetPasswordHash(userForLogin.Password, userForLoginConfirmation.PasswordSalt);

      // Compare the computed hash byte-by-byte with the stored hash
      for (int index = 0; index < passwordHash.Length; index++)
      {
        // If any byte does not match, return HTTP 401 Unauthorized with "Incorrect Password"
        if (passwordHash[index] != userForLoginConfirmation.PasswordHash[index])
        {
          return StatusCode(401, "Incorrect Password");
        }
      }
      // If all bytes match, return HTTP 200 OK indicating successful login
      return Ok();
    }
    

    // Helper method to hash a password with salt and app secret key using PBKDF2
    private byte[] GetPasswordHash(string password, byte[] passwordSalt)
    {
      // Get the app secret key from configuration and append the salt as a base64 string
      string passwordSaltPlusString = _config.GetSection("AppSettings:PasswordKey").Value + Convert.ToBase64String(passwordSalt);
      
      // Use PBKDF2 to hash the password with the combined salt and secret key
      return KeyDerivation.Pbkdf2(
        password: password,
        salt: Encoding.ASCII.GetBytes(passwordSaltPlusString),  // Salt bytes
        prf: KeyDerivationPrf.HMACSHA256,  // Pseudorandom function used (HMACSHA256)
        iterationCount: 1000000,  // Number of hashing iterations (very secure)
        numBytesRequested: 256 / 8  // Length of resulting hash (32 bytes)
      );
    }
  }
}
