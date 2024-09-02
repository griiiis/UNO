using System.ComponentModel.DataAnnotations;

namespace Domain.Database;

public class Player : BaseEntity
{
    [MaxLength(64)] 
    public string NickName { get; set; } = default!;

    public PlayerType PlayerType { get; set; }
    
    public Guid GameId { get; set; }
    public Game? Game { get; set; }
}