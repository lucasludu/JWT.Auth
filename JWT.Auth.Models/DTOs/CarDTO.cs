using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JWT.Auth.Models.DTOs
{
    public class CarDTO
    {
        public string? Marca { get; set; }
        public string? Modelo { get; set; }
        public int FechaModelo { get; set; }

        public CarDTO()
        {
            
        }

        public CarDTO(string marca, string modelo, int fechaModelo)
        {
            this.Marca = marca;
            this.Modelo = modelo;
            this.FechaModelo = fechaModelo;
        }
    }
}
