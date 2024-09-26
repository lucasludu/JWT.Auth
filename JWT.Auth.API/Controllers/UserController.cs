using JWT.Auth.Models.DTOs;
using JWT.Auth.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JWT.Auth.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserServices _userServices;

        public UserController(IUserServices userServices)
        {
            this._userServices = userServices;
        }


        [HttpPost("register")]
        public async Task<IActionResult> UserRegistration(UserRegisterDTO dto)
        {
            try
            {
                var result = await _userServices.RegisterNewUserAsync(dto);

                if (result.IsUserRegistered)
                {
                    return Ok(result.message);
                }
                ModelState.AddModelError("Email", result.message);
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("unique-user-email")]
        public IActionResult CheckUniqueUserEmail(string email)
        {
            try
            {
                var result = _userServices.CheckUserUniqueEmail(email);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(LoginDTO dto)
        {
            try
            {
                var result = await _userServices.LoginAsync(dto);

                if(result.IsLoginSuccess)
                {
                    return Ok(result.TokenResponse);
                }
                ModelState.AddModelError("Login Error", "Invalid Credentials");
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
