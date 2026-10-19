using System;
using System.Collections.Generic;

namespace WezProjectifier.Models;

public partial class AssignedTask
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public int MemberId { get; set; }

    public virtual Member Member { get; set; } = null!;

    public virtual ProjectTask Task { get; set; } = null!;
}
