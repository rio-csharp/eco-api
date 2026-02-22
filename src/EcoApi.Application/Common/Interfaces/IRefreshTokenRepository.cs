using EcoApi.Domain.Entities;

namespace EcoApi.Application.Common.Interfaces;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken refreshToken);
    Task<RefreshToken?> GetActiveByTokenHashAsync(string tokenHash);
    Task UpdateAsync(RefreshToken refreshToken);
}
