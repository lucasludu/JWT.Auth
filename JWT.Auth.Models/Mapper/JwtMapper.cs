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

        public static CarDTO FromCarToCarDto(Car car)
        {
            return new CarDTO()
            {
                Marca = car.Marca,
                Modelo = car.Modelo,
                FechaModelo = car.FechaModelo
            };
        }

        public static List<CarDTO> FromListCarToListCarDto(List<Car> cars)
        {
            var listaCarsDTO = new List<CarDTO>();
            foreach (var car in cars)
            {
                listaCarsDTO.Add(new CarDTO(car.Marca, car.Modelo, car.FechaModelo));
            }
            return listaCarsDTO;
        }
    }
}
