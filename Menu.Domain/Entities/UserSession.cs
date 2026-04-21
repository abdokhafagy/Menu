namespace Menu.Domain.Entities;

public class UserSession : BaseEntity
{
    public Guid UserId { get; set; }
    public string Jti { get; set; } = string.Empty;
    public string RefreshTokenHash { get; set; } = string.Empty;
    public string? Device { get; set; }
    public string? IpAddress { get; set; }
    public DateTime AccessTokenExpiresAt { get; set; }
    public DateTime RefreshTokenExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedAt { get; set; }

    public User User { get; set; } = null!;
}