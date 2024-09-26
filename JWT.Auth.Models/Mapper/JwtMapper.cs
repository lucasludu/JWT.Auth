using JWT.Auth.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JWT.Auth.Models.Mapper
{
    public static class JwtMapper
    {
        public static User FromUserRegisterDtoToUser(UserRegisterDTO dto)
        {
            return new User()
            {
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Password = dto.Password
            };
        }
    }
}
