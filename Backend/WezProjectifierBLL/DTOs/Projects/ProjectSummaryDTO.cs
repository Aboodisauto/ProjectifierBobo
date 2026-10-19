using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace WezProjectifierBLL.DTOs.Projects
{
    public class ProjectSummaryDTO
    {
        [Required]
        public int ProjectID { get; set; }
        [Required]
        [MaxLength(50)]
        public string ProjectName { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
    }
}
