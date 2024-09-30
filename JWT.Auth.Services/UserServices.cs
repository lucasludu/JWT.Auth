using JWT.Auth.Data;
using JWT.Auth.Models;
using JWT.Auth.Models.DTOs;
using JWT.Auth.Models.Mapper;
using JWT.Auth.Models.Shared.Settings;
using JWT.Auth.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace JWT.Auth.Services
{
    public class UserServices : IUserServices
    {
        private readonly Context _context;
        private readonly TokenSettings _tokenSettings;

        public UserServices(Context context, IOptions<TokenSettings> tokenSettings)
        {
            this._context = context;
            this._tokenSettings = tokenSettings.Value;
        }

        private string HashPassowrd(string plainPassword)
        {
            byte[] salt = new byte[16];
            
            using(var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var rfcPassword = new Rfc2898DeriveBytes(plainPassword, salt, 1000, HashAlgorithmName.SHA1);
            byte[] rfcPasswordHash = rfcPassword.GetBytes(20);

            byte[] passwordHash = new byte[36];
            Array.Copy(salt, 0, passwordHash, 0, 16);
            Array.Copy(rfcPasswordHash, 0, passwordHash, 16, 20);

            return Convert.ToBase64String(passwordHash);
        }

        public async Task<(bool IsUserRegistered, string message)> RegisterNewUserAsync(UserRegisterDTO userRegisterDTO)
        {
            var isUserExist = this._context.Users.Any(_ => _.Email.ToLower() == userRegisterDTO.Email.ToLower());

            if(isUserExist)
            {
                return (false, "Email Address Already Register.");
            }

            var newUser = JwtMapper.FromUserRegisterDtoToUser(userRegisterDTO);
            newUser.Password = HashPassowrd(newUser.Password);

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return (true, "success");
        }

        public bool CheckUserUniqueEmail(string email)
        {
            var userExist = _context.Users.Any(_ => _.Email.ToLower() == email.ToLower());
            return !userExist;
        }

        private bool PasswordVerification(string password, string dbPassword)
        {
            var dbPasswordHash = Convert.FromBase64String(dbPassword);
            var salt = new byte[16];
            Array.Copy(dbPasswordHash, 0, salt, 0, 16);

            var rfcPassword = new Rfc2898DeriveBytes(password, salt, 1000, HashAlgorithmName.SHA1);
            var rfcPasswordHash = rfcPassword.GetBytes(20);

            for(int i = 0; i < rfcPasswordHash.Length; i++)
            {
                if (dbPasswordHash[i+16] != rfcPasswordHash[i])
                {
                    return false;
                }
            }
            return true;
        }

        private string GenerateJwtToken(User user)
        {
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.SecretKey));
            var credentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var _claims = new List<Claim>();
            _claims.Add(new Claim("Sub", user.Id.ToString()));
            _claims.Add(new Claim("FirstName", user.FirstName));
            _claims.Add(new Claim("LastName", user.LastName));
            _claims.Add(new Claim("Email", user.Email));

            var securityToken = new JwtSecurityToken(
                    issuer: _tokenSettings.Issuer,
                    audience: _tokenSettings.Audience,
                    expires: DateTime.Now.AddMinutes(1),
                    signingCredentials: credentials,
                    claims: _claims
                );

            return new JwtSecurityTokenHandler().WriteToken(securityToken);
        }

        public async Task<(bool IsLoginSuccess, JwtTokenResponseDTO TokenResponse)> LoginAsync(LoginDTO dto)
        {
            if(string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password)) 
            {
                return (false, null);
            }

            var user = await this._context.Users
                .Where(_ => _.Email.ToLower() == dto.Email.ToLower())
                .FirstOrDefaultAsync();

            if(user == null)
            {
                return (false, null);
            }

            bool validPassword = PasswordVerification(dto.Password, user.Password);

            if (!validPassword)
            {
                return (false, null);
            }

            var jwtAccessToken = this.GenerateJwtToken(user);
            var refreshToken = await this.GenerateRefreshToken(user.Id);

            var result = new JwtTokenResponseDTO
            {
                AccessToken = jwtAccessToken,
                RefreshToken = refreshToken
            };
            return (true, result); 
        }

        private async Task<string> GenerateRefreshToken(int userId)
        {
            byte[] bytesOfToken = new byte[32];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytesOfToken);
            }

            var token = Convert.ToBase64String(bytesOfToken);

            var newRefreshToken = new UserRefreshToken
            {
                ExpirationDate = DateTime.Now.AddDays(3),
                Token = token,
                UserId = userId,
            };

            _context.UserRefreshToken.Add(newRefreshToken);
            await this._context.SaveChangesAsync();

            return token;
        }

        public async Task<(string ErrorMessage, JwtTokenResponseDTO jwtTokenResponse)> RenewTokenAsync(RenewTokenRequestDTO renewTokenRequestDTO)
        {
            var existingRefreshToken = await this._context.UserRefreshToken
                .Where(_ => _.UserId == renewTokenRequestDTO.UserId &&
                            _.Token == renewTokenRequestDTO.RefreshToken &&
                            _.ExpirationDate > DateTime.Now)
                .FirstOrDefaultAsync();

            if(existingRefreshToken == null)
            {
                return ("Invalid Refresh Token", null);
            }

            this._context.UserRefreshToken.Remove(existingRefreshToken);
            await this._context.SaveChangesAsync();

            var user = await this._context.Users.Where(_ => _.Id == renewTokenRequestDTO.UserId).FirstOrDefaultAsync();
            
            var jwtAccessToken = this.GenerateJwtToken(user);
            var refreshToken = await this.GenerateRefreshToken(user.Id);

            var result = new JwtTokenResponseDTO
            {
                AccessToken = jwtAccessToken,
                RefreshToken = refreshToken
            };
            return ("", result);
        }
    }
}
