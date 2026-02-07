using WorkLog.Domain.Entities;

namespace WorkLog.Domain.Interfaces;

/// <summary>
/// JWT Token 服務介面
/// </summary>
public interface IJwtService
{
    /// <summary>產生 Access Token</summary>
    string GenerateAccessToken(User user);

    /// <summary>產生 Refresh Token</summary>
    string GenerateRefreshToken();

    /// <summary>從 Token 取得使用者 ID（不驗證過期）</summary>
    int? GetUserIdFromToken(string token);
}
