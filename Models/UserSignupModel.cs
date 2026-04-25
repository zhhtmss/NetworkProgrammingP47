using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkProgrammingP47.Models
{
    internal class UserSignupModel
    {
        public String Email { get; set; } = null!;
        public String Name { get; set; } = null!;
        public String Password { get; set; } = null!;
        public String ConfirmCode { get; set; } = null!;
    }
}
