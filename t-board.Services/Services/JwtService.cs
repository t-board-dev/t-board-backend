using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using t_board.Services.Contracts;

namespace t_board.Services.Services
{
    public class JwtService : IJwtService
    {
        private readonly string _secretKey;
        private readonly string _expireMinute;

        private readonly IConfiguration _configuration;

        public JwtService(
            IConfiguration configuration)
        {
            _configuration = configuration;

            _secretKey = _configuration["Jwt:Key"];
            _expireMinute = _configuration["Jwt:ExpireMinute"];
        }

        /// <summary>
        /// Validates whether a given token is valid or not. 
        /// Returns true in case the token is valid,
        /// Otherwise it will throw exception and then return false;
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Boolean, whether token is valid or not</returns>
        public bool IsTokenValid(string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Given token is null or empty.");

            try
            {
                var validToken = ValidateToken(token);

                return true;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                return false;
            }
        }

        /// <summary>
        /// Generates token by given model.
        /// Validates whether the given model is valid, then gets the symmetric key.
        /// Encrypt the token and returns it.
        /// </summary>
        /// <param name="claims"></param>
        /// <returns>Generated token as a string.</returns>
        public string GenerateToken(IEnumerable<Claim> claims)
        {
            if (claims == null || claims.Any() is false)
                throw new ArgumentException("Arguments to create token are not valid.");

            var key = GetSymmetricSecurityKey();
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(_expireMinute)),
                claims: claims,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Receives the claims of token by given token as string.
        /// </summary>
        /// <remarks>
        /// Pay attention, when the token is INVALID the method will throw an exception.
        /// </remarks>
        /// <param name="token"></param>
        /// <returns>List of claims for the given token.</returns>
        public List<Claim> GetTokenClaims(string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Given token is null or empty.");

            var validToken = ValidateToken(token);

            return validToken.Claims.ToList();
        }

        private ClaimsPrincipal ValidateToken(string token)
        {
            var tokenValidationParameters = GetTokenValidationParameters();

            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

            return jwtSecurityTokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
        }

        private SecurityKey GetSymmetricSecurityKey()
        {
            byte[] symmetricKey = Encoding.UTF8.GetBytes(_secretKey);
            return new SymmetricSecurityKey(symmetricKey);
        }

        private TokenValidationParameters GetTokenValidationParameters()
        {
            return new TokenValidationParameters()
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = GetSymmetricSecurityKey()
            };
        }
    }
}
