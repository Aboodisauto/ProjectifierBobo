using System;
using System.Collections.Generic;

namespace WezProjectifier.Models;

public partial class User
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Firstname { get; set; } = null!;

    public string Lastname { get; set; } = null!;

    public string Phonenumber { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string? Image { get; set; }

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
