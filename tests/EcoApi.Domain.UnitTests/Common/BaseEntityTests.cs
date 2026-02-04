using EcoApi.Domain.Common;
using Xunit;

namespace EcoApi.Domain.UnitTests.Common;

public class BaseEntityTests
{
    private class TestEntity : BaseEntity { }

    [Fact]
    public void ShouldSetProperties()
    {
        var entity = new TestEntity
        {
            Id = 1,
            Created = DateTime.Now,
            CreatedBy = "Admin"
        };

        Assert.Equal(1, entity.Id);
        Assert.Equal("Admin", entity.CreatedBy);
    }
}
