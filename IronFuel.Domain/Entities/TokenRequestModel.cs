using System;
using System.Collections.Generic;
using System.Text;

namespace IronFuel.Domain.Entities
{
    public class TokenRequestModel
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
