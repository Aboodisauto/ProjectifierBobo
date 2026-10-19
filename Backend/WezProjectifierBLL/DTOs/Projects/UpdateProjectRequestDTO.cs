using System;
using System.Collections.Generic;
using System.Text;

namespace WezProjectifierBLL.DTOs.Projects
{
    public class UpdateProjectRequestDTO
    {
        public int ID { get; set; }
        public string ProjectName { get; set; } = null!;
        public string? Description { get; set; }
        public string? Image { get; set; }
    }
}
