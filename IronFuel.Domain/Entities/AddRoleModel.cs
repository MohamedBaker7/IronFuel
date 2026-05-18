using System;
using System.Collections.Generic;
using System.Text;

namespace IronFuel.Domain.Entities
{
    public class AddRoleModel
    {
        [Required]
        public string UserId { get; set; } = null!;
        [Required]
        public string RoleName { get; set; } = null!;
    }
}
