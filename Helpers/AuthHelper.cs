using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;

namespace UserManagement.Helpers
{
  public class AuthHelper
  {
    private readonly IConfiguration _config;
    public AuthHelper(IConfiguration config)
    {
      _config = config;
    }



    // Method to create a JWT token for a given userId
    // Helper method to hash a password with salt and app secret key using PBKDF2
    public byte[] GetPasswordHash(string password, byte[] passwordSalt)
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
    public string CreateToken(int userId)
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