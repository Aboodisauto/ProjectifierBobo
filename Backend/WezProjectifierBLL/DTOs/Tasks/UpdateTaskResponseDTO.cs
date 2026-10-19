using System;
using System.Collections.Generic;
using System.Text;

namespace WezProjectifierBLL.DTOs.Tasks
{
    public class UpdateTaskResponseDTO
    {
        public int ID { get; set; }
        public string TaskName { get; set; }
        public string? Description { get; set; }
        public string Priority { get; set; }
        public string? Status { get; set; }
        public int ProjectID { get; set; }
    }
}
