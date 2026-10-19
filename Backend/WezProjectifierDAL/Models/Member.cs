using System;
using System.Collections.Generic;

namespace WezProjectifier.Models;

public partial class Member
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public int UserId { get; set; }

    public string Role { get; set; } = null!;

    public virtual ICollection<AssignedTask> AssignedTasks { get; set; } = new List<AssignedTask>();

    public virtual Project Project { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
