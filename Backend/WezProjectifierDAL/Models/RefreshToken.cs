using System;
using System.Collections.Generic;

namespace WezProjectifier.Models;

public partial class RefreshToken
{
    public string RefreshToken1 { get; set; } = null!;

    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ExpirationDate { get; set; }

    public virtual User User { get; set; } = null!;
}
