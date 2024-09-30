using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JWT.Auth.Models.DTOs
{
    public class RenewTokenRequestDTO
    {
        public int UserId { get; set; }
        public string RefreshToken { get; set; }
    }
}
