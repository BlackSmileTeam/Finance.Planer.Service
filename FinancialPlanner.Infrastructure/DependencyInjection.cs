using FinancialPlanner.Application.Interfaces;
using FinancialPlanner.Infrastructure.Persistence;
using FinancialPlanner.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialPlanner.Infrastructure;

/// <summary>
/// <para>Provides helpers to wire up infrastructure services.</para>
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// <para>Registers the infrastructure services in the DI container.</para>
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Database connection string is not configured.");

        services.AddDbContext<FinancialPlannerDbContext>(options =>
        {
            var serverVersion = ServerVersion.AutoDetect(connectionString);
            options.UseMySql(connectionString, serverVersion);
        });

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<IIncomeService, IncomeService>();
        services.AddScoped<IMonthlyPlanService, MonthlyPlanService>();
        services.AddScoped<IMonthlySummaryService, MonthlySummaryService>();
        services.AddScoped<IRecurringExpenseService, RecurringExpenseService>();
        services.AddScoped<ICreditAccountService, CreditAccountService>();
        services.AddScoped<ICreditTransactionService, CreditTransactionService>();
        services.AddScoped<IInvestmentService, InvestmentService>();
        services.AddScoped<IIncomeRecordService, IncomeRecordService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IDataResetService, DataResetService>();

        return services;
    }
}

