using System;
using System.Collections.Generic;
using System.Text;

namespace WezProjectifierBLL.DTOs.Projects
{
    public class ProjectTaskDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string? Status { get; set; }
        public int ProjectId { get; set; }
        public IList<int> AssignedMemberIds { get; set; } = new List<int>();
    }
}
