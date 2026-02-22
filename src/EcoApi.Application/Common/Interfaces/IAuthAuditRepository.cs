using EcoApi.Domain.Entities;

namespace EcoApi.Application.Common.Interfaces;

public interface IAuthAuditRepository
{
    Task AddAsync(AuthAuditLog auditLog);
}
