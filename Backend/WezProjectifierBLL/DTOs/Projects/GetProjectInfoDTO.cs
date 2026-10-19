using System;
using System.Collections.Generic;
using System.Text;
using WezProjectifierBLL.DTOs.User;
using System.Linq;

namespace WezProjectifierBLL.DTOs.Projects
{
    public class GetProjectInfoDTO
    {
        public int ID { get; set; }
        public string ProjectName { get; set; }
        public string? Description { get; set; }
        public int? CreatedBy { get; set; }
        public UserSummaryDTO? OwnerUser { get; set; }
        public ICollection<MemberDTO> Member { get; set; } = new List<MemberDTO>();
        public ICollection<ProjectTaskDTO> Tasks { get; set; } = new List<ProjectTaskDTO>(); 
        public string? Image { get; set; }
    }
}
