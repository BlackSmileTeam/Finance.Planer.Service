using FinancialPlanner.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace FinancialPlanner.Infrastructure.Persistence;

/// <summary>
/// <para>
/// Entity Framework Core database context that encapsulates all persistence
/// details for the financial planner backend.
/// </para>
/// </summary>
public sealed partial class FinancialPlannerDbContext : DbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;
    private readonly AuditActorContext? _auditActorContext;

    /// <summary>
    /// <para>Initializes a new instance of the context.</para>
    /// </summary>
    public FinancialPlannerDbContext(
        DbContextOptions<FinancialPlannerDbContext> options,
        IHttpContextAccessor? httpContextAccessor = null,
        AuditActorContext? auditActorContext = null)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
        _auditActorContext = auditActorContext;
    }

    /// <summary>
    /// <para>Gets the set of users.</para>
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// <para>Gets the set of categories.</para>
    /// </summary>
    public DbSet<Category> Categories => Set<Category>();

    /// <summary>
    /// <para>Gets the set of expenses.</para>
    /// </summary>
    public DbSet<Expense> Expenses => Set<Expense>();

    /// <summary>
    /// <para>Gets the set of incomes.</para>
    /// </summary>
    public DbSet<IncomeCycle> IncomeCycles => Set<IncomeCycle>();

    /// <summary>
    /// <para>Gets the set of monthly plans.</para>
    /// </summary>
    public DbSet<MonthlyPlan> MonthlyPlans => Set<MonthlyPlan>();

    /// <summary>
    /// <para>Gets the set of planned budgets.</para>
    /// </summary>
    public DbSet<PlannedBudget> PlannedBudgets => Set<PlannedBudget>();

    /// <summary>
    /// <para>Gets the set of monthly snapshots.</para>
    /// </summary>
    public DbSet<MonthlySnapshot> MonthlySnapshots => Set<MonthlySnapshot>();

    /// <summary>
    /// <para>Gets the set of recurring expenses.</para>
    /// </summary>
    public DbSet<RecurringExpense> RecurringExpenses => Set<RecurringExpense>();

    /// <summary>
    /// <para>Gets the set of credit accounts.</para>
    /// </summary>
    public DbSet<CreditAccount> CreditAccounts => Set<CreditAccount>();

    /// <summary>
    /// <para>Gets the set of credit transactions.</para>
    /// </summary>
    public DbSet<CreditTransaction> CreditTransactions => Set<CreditTransaction>();

    /// <summary>
    /// <para>Gets the set of credit payment schedules.</para>
    /// </summary>
    public DbSet<CreditPaymentSchedule> CreditPaymentSchedules => Set<CreditPaymentSchedule>();

    /// <summary>
    /// <para>Gets the set of investments.</para>
    /// </summary>
    public DbSet<Investment> Investments => Set<Investment>();

    /// <summary>
    /// <para>Gets the set of income records.</para>
    /// </summary>
    public DbSet<IncomeRecord> IncomeRecords => Set<IncomeRecord>();

    /// <summary>
    /// <para>Gets the set of accounts.</para>
    /// </summary>
    public DbSet<Account> Accounts => Set<Account>();

    /// <summary>
    /// <para>Gets the set of audit log entries.</para>
    /// </summary>
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // �������������� �������������� ���� ���� �������� � snake_case
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // �������������� ���� ������
            entity.SetTableName(ToSnakeCase(entity.GetTableName()!));

            // �������������� ���� ��������
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));
            }

            // �������������� ���� ������
            foreach (var key in entity.GetKeys())
            {
                key.SetName(ToSnakeCase(key.GetName()!));
            }

            // �������������� ���� ��������
            foreach (var index in entity.GetIndexes())
            {
                index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName()!));
            }
        }

        base.OnModelCreating(modelBuilder);
        ConfigureUsers(modelBuilder);
        ConfigureCategories(modelBuilder);
        ConfigureExpenses(modelBuilder);
        ConfigureIncome(modelBuilder);
        ConfigureMonthlyPlans(modelBuilder);
        ConfigurePlannedBudgets(modelBuilder);
        ConfigureSnapshots(modelBuilder);
        ConfigureRecurringExpenses(modelBuilder);
        ConfigureCreditAccounts(modelBuilder);
        ConfigureCreditTransactions(modelBuilder);
        ConfigureCreditPaymentSchedules(modelBuilder);
        ConfigureInvestments(modelBuilder);
        ConfigureIncomeRecords(modelBuilder);
        ConfigureAccounts(modelBuilder);
        ConfigureAuditLogs(modelBuilder);
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username)
                  .HasMaxLength(50)
                  .IsRequired();
            entity.Property(e => e.Email)
                  .HasMaxLength(100)
                  .IsRequired();
            entity.Property(e => e.PasswordHash)
                  .HasMaxLength(255)
                  .IsRequired()
                  .HasColumnName("password_hash");
            entity.Property(e => e.FullName)
                  .HasMaxLength(150)
                  .HasColumnName("full_name");
            entity.Property(e => e.IsActive)
                  .HasDefaultValue(true)
                  .HasColumnName("is_active");
            entity.Property(e => e.IsAdministrator)
                  .HasDefaultValue(false)
                  .HasColumnName("is_administrator");
            entity.Property(e => e.LastLoginAt)
                  .HasColumnName("last_login_at");
            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                  .HasColumnName("updated_at");
            entity.HasIndex(e => e.Username)
                  .IsUnique();
            entity.HasIndex(e => e.Email)
                  .IsUnique();
        });
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var result = new StringBuilder();
        result.Append(char.ToLower(input[0]));

        for (int i = 1; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]))
            {
                result.Append('_');
                result.Append(char.ToLower(input[i]));
            }
            else
            {
                result.Append(input[i]);
            }
        }

        return result.ToString();
    }


    private static void ConfigureCategories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                  .HasMaxLength(120)
                  .IsRequired();
            entity.Property(e => e.HexColor)
                  .HasMaxLength(7)
                  .IsRequired()
                  .HasColumnName("hex_color"); 
            entity.Property(e => e.IsActive)
                  .HasDefaultValue(true)
                  .ValueGeneratedNever()
                  .HasColumnName("is_active"); 
            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                  .HasColumnName("updated_at");
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Categories)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Parent)
                  .WithMany(c => c.Subcategories)
                  .HasForeignKey(e => e.ParentId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserId, e.Name })
                  .IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ParentId);
        });
    }

    private static void ConfigureExpenses(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Expense>(entity =>
        {
            entity.ToTable("expenses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).HasMaxLength(400);
            entity.Property(e => e.Amount).HasColumnType("decimal(12,2)");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.ExpenseDate)
                  .HasColumnName("expense_date")
                  .HasConversion(
                      date => date.ToDateTime(TimeOnly.MinValue),
                      value => DateOnly.FromDateTime(value));
            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Expenses)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Subcategory)
                  .WithMany()
                  .HasForeignKey(e => e.SubcategoryId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.PlannedBudget)
                  .WithMany(p => p.Expenses)
                  .HasForeignKey(e => e.PlannedBudgetId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Expenses)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Account)
                  .WithMany()
                  .HasForeignKey(e => e.AccountId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.CreditPaymentSchedule)
                  .WithMany()
                  .HasForeignKey(e => e.CreditPaymentScheduleId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.Property(e => e.CreditPaymentScheduleId)
                  .HasColumnName("credit_payment_schedule_id");
            entity.HasOne(e => e.CreditAccount)
                  .WithMany()
                  .HasForeignKey(e => e.CreditAccountId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.Property(e => e.CreditAccountId)
                  .HasColumnName("credit_account_id");
            entity.Property(e => e.IsPlanned)
                  .HasDefaultValue(false)
                  .HasColumnName("is_planned");
            entity.HasIndex(e => e.ExpenseDate);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.ExpenseDate });
            entity.HasIndex(e => e.SubcategoryId);
            entity.HasIndex(e => e.AccountId);
            entity.HasIndex(e => new { e.IsPlanned, e.ExpenseDate });
            entity.HasIndex(e => e.CreditAccountId);
        });
    }

    private static void ConfigureIncome(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IncomeCycle>(entity =>
        {
            entity.ToTable("income_cycles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Amount).HasColumnType("decimal(12,2)");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.ReceivedDate)
                  .HasColumnName("received_date")
                  .HasConversion(
                      date => date.ToDateTime(TimeOnly.MinValue),
                      value => DateOnly.FromDateTime(value));
            entity.Property(e => e.StartDate)
                  .HasColumnName("start_date")
                  .HasConversion(
                      date => date.ToDateTime(TimeOnly.MinValue),
                      value => DateOnly.FromDateTime(value));
            entity.Property(e => e.EndDate)
                  .HasColumnName("end_date")
                  .HasConversion(
                      date => date.HasValue ? date.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                      value => value.HasValue ? DateOnly.FromDateTime(value.Value) : (DateOnly?)null);
            entity.Property(e => e.FirstConfirmedDate)
                  .HasColumnName("first_confirmed_date")
                  .HasConversion(
                      date => date.HasValue ? date.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                      value => value.HasValue ? DateOnly.FromDateTime(value.Value) : (DateOnly?)null);
            entity.Property(e => e.Frequency)
                  .HasConversion<string>()
                  .HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(e => e.User)
                  .WithMany(u => u.IncomeCycles)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Account)
                  .WithMany()
                  .HasForeignKey(e => e.AccountId)
                  .OnDelete(DeleteBehavior.SetNull)
                  .IsRequired(false);
            entity.Property(e => e.IsPlanned)
                  .HasDefaultValue(false)
                  .HasColumnName("is_planned");
            entity.HasIndex(e => e.ReceivedDate);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.AccountId);
            entity.HasIndex(e => new { e.IsPlanned, e.StartDate });
        });
    }

    private static void ConfigureMonthlyPlans(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MonthlyPlan>(entity =>
        {
            entity.ToTable("monthly_plans");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PlanYear).HasColumnName("plan_year");
            entity.Property(e => e.PlanMonth).HasColumnName("plan_month");
            entity.Property(e => e.PlannedIncome).HasColumnType("decimal(12,2)");
            entity.Property(e => e.PlannedExpense).HasColumnType("decimal(12,2)");
            entity.Property(e => e.CarryOver).HasColumnType("decimal(12,2)");
            entity.Property(e => e.ExpectedPayCycles).HasColumnName("expected_pay_cycles");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(e => e.User)
                  .WithMany(u => u.MonthlyPlans)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Snapshot)
                  .WithOne(s => s.Plan!)
                  .HasForeignKey<MonthlySnapshot>(s => s.PlanId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserId, e.PlanYear, e.PlanMonth }).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.PlanYear, e.PlanMonth });
        });
    }

    private static void ConfigurePlannedBudgets(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlannedBudget>(entity =>
        {
            entity.ToTable("planned_budgets");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PlannedAmount).HasColumnType("decimal(12,2)");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(e => e.Plan)
                  .WithMany(p => p.PlannedBudgets)
                  .HasForeignKey(e => e.PlanId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Category)
                  .WithMany(c => c.PlannedBudgets)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Subcategory)
                  .WithMany()
                  .HasForeignKey(e => e.SubcategoryId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => new { e.PlanId, e.CategoryId }).IsUnique();
            entity.HasIndex(e => e.SubcategoryId);
        });
    }

    private static void ConfigureSnapshots(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MonthlySnapshot>(entity =>
        {
            entity.ToTable("monthly_snapshots");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ActualIncome).HasColumnType("decimal(12,2)");
            entity.Property(e => e.ActualExpense).HasColumnType("decimal(12,2)");
            entity.Property(e => e.ClosingBalance).HasColumnType("decimal(12,2)");
            entity.Property(e => e.GeneratedAt).HasColumnName("generated_at");
        });
    }

    private static void ConfigureRecurringExpenses(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RecurringExpense>(entity =>
        {
            entity.ToTable("recurring_expenses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Amount).HasColumnType("decimal(12,2)");
            entity.Property(e => e.StartDate)
                  .HasColumnName("start_date")
                  .HasConversion(
                      date => date.ToDateTime(TimeOnly.MinValue),
                      value => DateOnly.FromDateTime(value));
            entity.Property(e => e.EndDate)
                  .HasColumnName("end_date")
                  .HasConversion(
                      date => date.HasValue ? date.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                      value => value.HasValue ? DateOnly.FromDateTime(value.Value) : (DateOnly?)null);
            entity.Property(e => e.Frequency)
                  .HasConversion<string>()
                  .HasMaxLength(20);
            entity.Property(e => e.IsActive)
                  .HasDefaultValue(true)
                  .HasColumnName("is_active");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(e => e.User)
                  .WithMany(u => u.RecurringExpenses)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Category)
                  .WithMany()
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Subcategory)
                  .WithMany()
                  .HasForeignKey(e => e.SubcategoryId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.StartDate);
        });
    }

    private static void ConfigureCreditAccounts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CreditAccount>(entity =>
        {
            entity.ToTable("credit_accounts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(150).IsRequired();
            entity.Property(e => e.AccountType)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .HasColumnName("account_type");
            entity.Property(e => e.CreditLimit)
                  .HasColumnType("decimal(12,2)")
                  .HasColumnName("credit_limit");
            entity.Property(e => e.MonthlyPayment)
                  .HasColumnType("decimal(12,2)")
                  .HasColumnName("monthly_payment");
            entity.Property(e => e.TotalAmount)
                  .HasColumnType("decimal(12,2)")
                  .HasColumnName("total_amount");
            entity.Property(e => e.TermMonths)
                  .HasColumnName("term_months");
            entity.Property(e => e.PaymentStartDate)
                  .HasColumnName("payment_start_date")
                  .HasConversion(
                      date => date.HasValue ? date.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                      value => value.HasValue ? DateOnly.FromDateTime(value.Value) : (DateOnly?)null);
            entity.Property(e => e.CurrentBalance)
                  .HasColumnType("decimal(12,2)")
                  .HasDefaultValue(0)
                  .HasColumnName("current_balance");
            entity.Property(e => e.InterestRate)
                  .HasColumnType("decimal(5,2)")
                  .HasColumnName("interest_rate");
            entity.Property(e => e.IsActive)
                  .HasDefaultValue(true)
                  .HasColumnName("is_active");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(e => e.User)
                  .WithMany(u => u.CreditAccounts)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.UserId);
        });
    }

    private static void ConfigureCreditTransactions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CreditTransaction>(entity =>
        {
            entity.ToTable("credit_transactions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnType("decimal(12,2)");
            entity.Property(e => e.Description).HasMaxLength(400);
            entity.Property(e => e.TransactionDate)
                  .HasColumnName("transaction_date")
                  .HasConversion(
                      date => date.ToDateTime(TimeOnly.MinValue),
                      value => DateOnly.FromDateTime(value));
            entity.Property(e => e.IsIncomeRecorded)
                  .HasDefaultValue(false)
                  .HasColumnName("is_income_recorded");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(e => e.User)
                  .WithMany(u => u.CreditTransactions)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.CreditAccount)
                  .WithMany(a => a.Transactions)
                  .HasForeignKey(e => e.CreditAccountId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Category)
                  .WithMany()
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Subcategory)
                  .WithMany()
                  .HasForeignKey(e => e.SubcategoryId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.IncomeRecord)
                  .WithOne(i => i.CreditTransaction!)
                  .HasForeignKey<IncomeRecord>(i => i.CreditTransactionId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreditAccountId);
            entity.HasIndex(e => e.TransactionDate);
        });
    }

    private static void ConfigureCreditPaymentSchedules(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CreditPaymentSchedule>(entity =>
        {
            entity.ToTable("credit_payment_schedule");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ScheduledYear).HasColumnName("scheduled_year");
            entity.Property(e => e.ScheduledMonth).HasColumnName("scheduled_month");
            entity.Property(e => e.PaymentAmount).HasColumnType("decimal(12,2)");
            entity.Property(e => e.IsPaid)
                  .HasDefaultValue(false)
                  .HasColumnName("is_paid");
            entity.Property(e => e.PaidDate)
                  .HasColumnName("paid_date")
                  .HasConversion(
                      date => date.HasValue ? date.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                      value => value.HasValue ? DateOnly.FromDateTime(value.Value) : (DateOnly?)null);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(e => e.CreditTransaction)
                  .WithMany(t => t.PaymentSchedule)
                  .HasForeignKey(e => e.CreditTransactionId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User)
                  .WithMany(u => u.CreditPaymentSchedules)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.CreditTransactionId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.ScheduledYear, e.ScheduledMonth });
        });
    }

    private static void ConfigureInvestments(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Investment>(entity =>
        {
            entity.ToTable("investments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(150).IsRequired();
            entity.Property(e => e.InvestmentType)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .HasColumnName("investment_type");
            entity.Property(e => e.Amount).HasColumnType("decimal(12,2)");
            entity.Property(e => e.CurrentValue)
                  .HasColumnType("decimal(12,2)")
                  .HasColumnName("current_value");
            entity.Property(e => e.PurchaseDate)
                  .HasColumnName("purchase_date")
                  .HasConversion(
                      date => date.ToDateTime(TimeOnly.MinValue),
                      value => DateOnly.FromDateTime(value));
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Investments)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.PurchaseDate);
        });
    }

    private static void ConfigureIncomeRecords(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IncomeRecord>(entity =>
        {
            entity.ToTable("income_records");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Amount).HasColumnType("decimal(12,2)");
            entity.Property(e => e.ReceivedDate)
                  .HasColumnName("received_date")
                  .HasConversion(
                      date => date.ToDateTime(TimeOnly.MinValue),
                      value => DateOnly.FromDateTime(value));
            entity.Property(e => e.IsFromCredit)
                  .HasDefaultValue(false)
                  .HasColumnName("is_from_credit");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(e => e.User)
                  .WithMany(u => u.IncomeRecords)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.IncomeCycle)
                  .WithMany()
                  .HasForeignKey(e => e.IncomeCycleId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.CreditTransaction)
                  .WithOne(t => t.IncomeRecord!)
                  .HasForeignKey<IncomeRecord>(e => e.CreditTransactionId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.Property(e => e.IsPlanned)
                  .HasDefaultValue(false)
                  .HasColumnName("is_planned");
            entity.Property(e => e.FirstConfirmedDate)
                  .HasColumnName("first_confirmed_date");
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ReceivedDate);
            entity.HasIndex(e => e.IncomeCycleId);
            entity.HasIndex(e => new { e.IsPlanned, e.ReceivedDate });
        });
    }

    private static void ConfigureAccounts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("accounts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(150).IsRequired();
            entity.Property(e => e.AccountNumber).HasMaxLength(50).HasColumnName("account_number");
            entity.Property(e => e.AccountType).HasMaxLength(20).IsRequired().HasColumnName("account_type");
            entity.Property(e => e.Balance).HasColumnType("decimal(12,2)");
            entity.Property(e => e.ExpiryDate).HasMaxLength(10).HasColumnName("expiry_date");
            entity.Property(e => e.Color).HasMaxLength(7).HasColumnName("color");
            entity.Property(e => e.Currency).HasMaxLength(10).HasColumnName("currency");
            entity.Property(e => e.IsActive)
                  .HasDefaultValue(true)
                  .ValueGeneratedNever()
                  .HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Accounts)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.UserId).HasDatabaseName("ix_accounts_user");
            entity.HasIndex(e => new { e.UserId, e.IsActive }).HasDatabaseName("ix_accounts_user_active");
        });
    }

    private static void ConfigureAuditLogs(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.Property(e => e.Action).HasMaxLength(20).IsRequired();
            entity.Property(e => e.EntityType).HasMaxLength(120).IsRequired();
            entity.Property(e => e.EntityIdJson).HasColumnType("longtext");
            entity.Property(e => e.StateBeforeJson).HasColumnType("longtext");
            entity.Property(e => e.StateAfterJson).HasColumnType("longtext");
            entity.Property(e => e.CreatedAtUtc).HasColumnName("created_at_utc");
            entity.HasIndex(e => e.UserId).HasDatabaseName("ix_audit_log_user");
            entity.HasIndex(e => e.CreatedAtUtc).HasDatabaseName("ix_audit_log_created");
            entity.HasIndex(e => new { e.EntityType, e.CreatedAtUtc }).HasDatabaseName("ix_audit_log_entity_created");
            entity.HasOne<User>().WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.SetNull);
        });
    }
}

