using EcoApi.Application.Interfaces;

namespace EcoApi.Infrastructure.Services;

public class DateTimeService : IDateTime
{
    public DateTime Now => DateTime.Now;
}
