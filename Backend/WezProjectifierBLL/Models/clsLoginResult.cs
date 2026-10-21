using System;
using System.Collections.Generic;
using System.Text;
using WezProjectifier.Models;

namespace WezProjectifierBLL.Models
{
    public class clsLoginResult
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public enOperationResult Result { get; set; }
    }
}
