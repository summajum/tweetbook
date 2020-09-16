using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TweetBook.Data;
using TweetBook.Domain;
using TweetBook.Options;

namespace TweetBook.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtSettings _jwtSettings;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly DataContext _dataContext;
        public IdentityService(UserManager<IdentityUser> userManager, JwtSettings jwtSettings, TokenValidationParameters tokenValidationParameters, DataContext dataContext)
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings;
            _tokenValidationParameters = tokenValidationParameters;
            _dataContext = dataContext;
        }
        public async Task<AuthenticationResult> RegisterUserAsync(string email, string password)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);

            if(existingUser != null)
            {
                return new AuthenticationResult
                {
                    ErrorMessage = new [] {"User with Email Id already exists."}
                };
            }

            var newUser = new IdentityUser
            {
                Email = email,
                UserName = email
            };

            var createdUser = await _userManager.CreateAsync(newUser, password);

            if (!createdUser.Succeeded)
            {
                return new AuthenticationResult
                {
                    ErrorMessage = createdUser.Errors.Select(x => x.Description)
                };
            }

            return await GenerateAuthenticationResultAsync(newUser);
        }


        public async Task<AuthenticationResult> LoginUserAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return new AuthenticationResult
                {
                    ErrorMessage = new[] { "UserId doesn't exist." }
                };
            }

            var userHasValidPassword = await _userManager.CheckPasswordAsync(user, password);
            if (!userHasValidPassword)
            {
                return new AuthenticationResult
                {
                    ErrorMessage = new[] { "UserId or Password is incorrect." }
                };
            }

            return await GenerateAuthenticationResultAsync(user);
        }


        public async Task<AuthenticationResult> RefreshTokenAsync(string token, string refreshToken)
        {
            var validatedToken = GetClaimsPrincipalFromToken(token);
            if (validatedToken == null)
            {
                return new AuthenticationResult
                {
                    ErrorMessage = new[] { "Invalid Token" }
                };                   
            }

            var expiryDateinUnix = long.Parse(validatedToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
            var expiryDateUTC = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(expiryDateinUnix);

            if(expiryDateUTC > DateTime.UtcNow)
            {
                return new AuthenticationResult
                {
                    ErrorMessage = new[] { "This token hasn't expired yet." }
                };
            }

            var jti = validatedToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

            var storedRefreshToken = await _dataContext.RefreshTokens.SingleOrDefaultAsync(x=>x.Token == refreshToken);

            if (storedRefreshToken == null)
            {
                return new AuthenticationResult
                {
                    ErrorMessage = new[] { "This refresh token doesn't exist." }
                };
            }

            if(DateTime.UtcNow > storedRefreshToken.ExpiryDate)
            {
                return new AuthenticationResult
                {
                    ErrorMessage = new[] { "This refresh token has expired." }
                };
            }

            if (storedRefreshToken.Invalidated)
            {
                return new AuthenticationResult
                {
                    ErrorMessage = new[] { "This refresh token has been invalidated." }
                };
            }
            if (storedRefreshToken.Used)
            {
                return new AuthenticationResult
                {
                    ErrorMessage = new[] { "This refresh token has been used." }
                };
            }
            if (storedRefreshToken.JwtId != jti)
            {
                return new AuthenticationResult
                {
                    ErrorMessage = new[] { "This refresh token doesn't match this JWT ." }
                };
            }

            storedRefreshToken.Used = true;
            _dataContext.RefreshTokens.Update(storedRefreshToken);
            await _dataContext.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(validatedToken.Claims.Single(x => x.Type == "userid").Value);
            return await GenerateAuthenticationResultAsync(user);
        }

        private async Task<AuthenticationResult> GenerateAuthenticationResultAsync(IdentityUser user)
        {
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                 {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("userid", user.Id)
                 }),
                Expires = DateTime.UtcNow.Add(_jwtSettings.TokenLifeTime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var refreshToken = new RefreshToken
            {
                JwtId = token.Id,
                UserId = user.Id,
                CreationDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(6)
            };

            await _dataContext.RefreshTokens.AddAsync(refreshToken);
            await _dataContext.SaveChangesAsync();


            return new AuthenticationResult
            {
                IsSuccess = true,
                Token = tokenHandler.WriteToken(token),
                RefreshToken = refreshToken.Token
            };
        }

        private ClaimsPrincipal GetClaimsPrincipalFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var tokenValidationParameters = _tokenValidationParameters.Clone();
                tokenValidationParameters.ValidateLifetime = false;
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
                if (!IsJwtWithValidSecurityAlgorithm(validatedToken))
                {
                    return null;
                }
                return principal;
            }
            catch
            {
                return null;
            }
        }

        private bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
        {
            return (validatedToken is JwtSecurityToken jwtSecurityToken) &&
                jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
