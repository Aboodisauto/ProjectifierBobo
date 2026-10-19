using System;
using System.Collections.Generic;

namespace WezProjectifier.Models;

public partial class Project
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int CreatedBy { get; set; }

    public string? Image { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();

    public virtual ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
}
