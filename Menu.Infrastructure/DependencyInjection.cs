using Menu.Application.Interfaces;
using Menu.Domain.Interfaces;
using Menu.Infrastructure.Data;
using Menu.Infrastructure.Repositories;
using Menu.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Menu.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPublicMenuService, PublicMenuService>();

        return services;
    }
}
