using JWT.Auth.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JWT.Auth.Services.Interfaces
{
    public interface IUserServices
    {
        Task<(bool IsUserRegistered, string message)> RegisterNewUserAsync(UserRegisterDTO userRegisterDTO);
        bool CheckUserUniqueEmail(string email);
        Task<(bool IsLoginSuccess, JwtTokenResponseDTO TokenResponse)> LoginAsync(LoginDTO dto);
        Task<(string ErrorMessage, JwtTokenResponseDTO jwtTokenResponse)> RenewTokenAsync(RenewTokenRequestDTO renewTokenRequestDTO);
    }
}
