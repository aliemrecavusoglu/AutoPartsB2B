using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPartsB2B.Core.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }

        // Sisteme giriş ve kayıt işlemleri için gereken doğrulama alanları
        public string Username { get; set; }
        public string Password { get; set; }
    }
}