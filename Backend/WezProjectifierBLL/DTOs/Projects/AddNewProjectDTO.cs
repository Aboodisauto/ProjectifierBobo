using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using WezProjectifier.Models;
using WezProjectifierBLL.DTOs.User;

namespace WezProjectifierBLL.DTOs.Projects
{
    public class AddNewProjectRequestDTO
    {
        [Required]
        public string ProjectName { get; set; }
        public string? Description { get; set; }
        public int CreatedBy { get; set; }
        public string? Image { get; set; }
    }
    public class AddNewProjectResponseDTO
    {
        [Required]
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public string? Description { get; set; }
        public int CreatedBy { get; set; }
        public UserSummaryDTO? User { get; set; }
        public string? Image { get; set; }
    }
}
