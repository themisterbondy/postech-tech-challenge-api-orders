using Microsoft.EntityFrameworkCore;
using Postech.Fiap.Orders.WebApi.Persistence;

namespace Postech.Fiap.Orders.WepApi.UnitTests.Mocks;

public static class ApplicationDbContextMock
{
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);

        return context;
    }
}