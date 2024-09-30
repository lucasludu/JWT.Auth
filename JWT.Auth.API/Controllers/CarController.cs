using JWT.Auth.Data;
using JWT.Auth.Models.Mapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JWT.Auth.API.Controllers
{
    [Route("api/car")]
    [ApiController]
    public class CarController : ControllerBase
    {
        private readonly Context _context;

        public CarController(Context context)
        {
            this._context = context;
        }

        [HttpGet("cars")]
        [Authorize]
        public IActionResult GetAll()
        {
            try
            {
                var result = _context.Cars.ToList();
                return Ok(JwtMapper.FromListCarToListCarDto(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
