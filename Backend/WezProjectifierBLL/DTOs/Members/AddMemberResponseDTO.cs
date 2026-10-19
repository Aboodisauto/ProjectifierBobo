using System;
using System.Collections.Generic;
using System.Text;
using WezProjectifierBLL.DTOs.Projects;
using WezProjectifierBLL.DTOs.User;

namespace WezProjectifierBLL.DTOs.Members
{
    public class AddMemberResponseDTO
    {
        public int MemberId { get; set; }
        public UserSummaryDTO User { get; set; } = null!;
        public ProjectSummaryDTO Project { get; set; } = null!;
    }
}
