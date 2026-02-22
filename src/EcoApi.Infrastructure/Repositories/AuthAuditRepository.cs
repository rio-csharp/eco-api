using EcoApi.Application.Common.Interfaces;
using EcoApi.Domain.Entities;
using EcoApi.Infrastructure.Persistence;

namespace EcoApi.Infrastructure.Repositories;

public class AuthAuditRepository : IAuthAuditRepository
{
    private readonly ApplicationDbContext _context;

    public AuthAuditRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuthAuditLog auditLog)
    {
        await _context.AuthAuditLogs.AddAsync(auditLog);
        await _context.SaveChangesAsync();
    }
}
