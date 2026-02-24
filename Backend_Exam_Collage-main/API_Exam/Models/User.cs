using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace API_Exam_23010101161.Models;

public partial class User
{
    [JsonIgnore]
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int RoleId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Role Role { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Ticket> TicketAssignedToNavigations { get; set; } = new List<Ticket>();
    [JsonIgnore]

    public virtual ICollection<TicketComment> TicketComments { get; set; } = new List<TicketComment>();
    [JsonIgnore]

    public virtual ICollection<Ticket> TicketCreatedByNavigations { get; set; } = new List<Ticket>();
    [JsonIgnore]

    public virtual ICollection<TicketStatusLog> TicketStatusLogs { get; set; } = new List<TicketStatusLog>();
}
