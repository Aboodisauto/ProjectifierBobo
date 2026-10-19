using System;
using System.Collections.Generic;
using System.Text;
using WezProjectifierBLL.DTOs.User;

namespace WezProjectifierBLL.DTOs.Projects
{
    public class MemberDTO
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int UserId { get; set; }
        public string Role { get; set; } = string.Empty;
        public UserSummaryDTO? User { get; set; }
    }
}
