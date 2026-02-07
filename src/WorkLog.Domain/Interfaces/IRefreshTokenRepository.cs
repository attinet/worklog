using WorkLog.Domain.Entities;

namespace WorkLog.Domain.Interfaces;

/// <summary>
/// Refresh Token Repository 介面
/// </summary>
public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task CreateAsync(RefreshToken refreshToken);
    Task RevokeAsync(string token);
    Task RevokeAllByUserIdAsync(int userId);
}
