using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FinancialPlanner.Application.DTOs;
using FinancialPlanner.Application.Interfaces;
using FinancialPlanner.Application.Requests;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;

namespace FinancialPlanner.Infrastructure.Services;

/// <summary>
/// <para>Provides authentication and authorization services.</para>
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly FinancialPlannerDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly AuditActorContext _auditActorContext;

    /// <summary>
    /// <para>Initializes a new instance of the auth service.</para>
    /// </summary>
    public AuthService(
        FinancialPlannerDbContext context,
        IConfiguration configuration,
        AuditActorContext auditActorContext)
    {
        _context = context;
        _configuration = configuration;
        _auditActorContext = auditActorContext;
    }

    /// <inheritdoc/>
    public async Task<LoginResponseDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var username = request.Username.Trim();
        var email = request.Email.Trim();
        if (username.Length == 0 || email.Length == 0)
            throw new InvalidOperationException("Username and email are required.");
        if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
            throw new InvalidOperationException("Invalid email address.");

        if (await _context.Users.AnyAsync(
                u => u.Username == username || u.Email.ToLower() == email.ToLower(),
                cancellationToken))
        {
            throw new InvalidOperationException("Username or email already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            FullName = string.IsNullOrWhiteSpace(request.FullName) ? null : request.FullName.Trim(),
            PasswordHash = HashPassword(request.Password),
            IsActive = true,
            IsAdministrator = false,
            LastLoginAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        _auditActorContext.ActingUserId = user.Id;
        try
        {
            await SaveChangesWithAuditFallbackAsync(cancellationToken);
        }
        finally
        {
            _auditActorContext.ActingUserId = null;
        }

        var token = GenerateJwtToken(user);
        var expiresAt = DateTime.UtcNow.AddDays(7);

        return new LoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = MapUserDto(user)
        };
    }

    /// <inheritdoc/>
    public async Task<LoginResponseDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => 
                (u.Username == request.UsernameOrEmail || u.Email == request.UsernameOrEmail) && 
                u.IsActive, 
                cancellationToken);

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        _auditActorContext.ActingUserId = user.Id;
        try
        {
            await SaveChangesWithAuditFallbackAsync(cancellationToken);
        }
        finally
        {
            _auditActorContext.ActingUserId = null;
        }

        var token = GenerateJwtToken(user);
        var expiresAt = DateTime.UtcNow.AddDays(7);

        return new LoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = MapUserDto(user)
        };
    }

    /// <inheritdoc/>
    public Guid? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is not configured."));
            
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : null;
        }
        catch
        {
            return null;
        }
    }

    private static UserDto MapUserDto(User user) =>
        new()
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            IsAdministrator = user.IsAdministrator
        };

    private string GenerateJwtToken(User user)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is not configured."));
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email)
        };
        if (user.IsAdministrator)
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private static bool VerifyPassword(string password, string passwordHash)
    {
        var hashOfInput = HashPassword(password);
        return hashOfInput == passwordHash;
    }

    private async Task SaveChangesWithAuditFallbackAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsAuditLogTableMissing(ex))
        {
            foreach (var entry in _context.ChangeTracker.Entries<AuditLog>().Where(e => e.State == EntityState.Added).ToList())
                entry.State = EntityState.Detached;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private static bool IsAuditLogTableMissing(DbUpdateException exception)
    {
        return exception.InnerException is MySqlException mySqlException &&
               mySqlException.Number == 1146 &&
               mySqlException.Message.Contains("audit_logs", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public async Task UpdatePasswordAsync(Guid userId, UpdatePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException("User was not found.");
        }

        if (!VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Current password is incorrect.");
        }

        user.PasswordHash = HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}

