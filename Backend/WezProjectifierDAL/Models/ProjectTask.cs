using System;
using System.Collections.Generic;

namespace WezProjectifier.Models;

public partial class ProjectTask
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string Priority { get; set; } = null!;

    public string? Status { get; set; }

    public int ProjectId { get; set; }

    public virtual ICollection<AssignedTask> AssignedTasks { get; set; } = new List<AssignedTask>();

    public virtual Project Project { get; set; } = null!;
}
