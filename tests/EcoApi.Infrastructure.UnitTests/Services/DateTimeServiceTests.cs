using EcoApi.Infrastructure.Services;
using Xunit;

namespace EcoApi.Infrastructure.UnitTests.Services;

public class DateTimeServiceTests
{
    [Fact]
    public void ShouldReturnCurrentTime()
    {
        var service = new DateTimeService();
        var now = service.Now;
        
        Assert.True(now <= DateTime.Now);
    }
}
