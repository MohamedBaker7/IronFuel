using System;
using System.Collections.Generic;
using System.Text;

namespace IronFuel.Domain.Entities
{
    public class RegisterModel
    {
        [MaxLength(200)]
        public string Email { get; set; } = null!;

        [MaxLength(100)]
        public string Password { get; set; } = null!;
    }
}
