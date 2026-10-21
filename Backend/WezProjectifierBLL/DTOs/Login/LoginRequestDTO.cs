using System;
using System.Collections.Generic;
using System.Text;

namespace WezProjectifierBLL.DTOs.Login
{
    public class LoginRequestDTO
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
