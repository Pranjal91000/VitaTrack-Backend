using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using VitaTrack.Core.Auth;
using VitaTrack.Core.Entities;
using VitaTrack.Core.Interfaces;

namespace VitaTrack.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public IdentityService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
            throw new Exception("Email already exists.");

        var user = new User
        {
            Email = request.Email,
            Name = request.Name,
            PasswordHash = HashPassword(request.Password),
            Role = "User"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return await GenerateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user == null || user.PasswordHash != HashPassword(request.Password))
            throw new Exception("Invalid email or password");

        return await GenerateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshRequest request, CancellationToken cancellationToken)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == request.RefreshToken, cancellationToken);

        if (refreshToken == null || !refreshToken.IsActive)
            throw new Exception("Invalid refresh token");

        // Revoke used token
        refreshToken.Revoked = DateTime.UtcNow;
        refreshToken.RevokedByIp = "Unknown"; // Can inject IHttpContextAccessor if needed

        // Generate new. This will save changes including revocation.
        var newTokens = await GenerateAuthResponseAsync(refreshToken.User, cancellationToken);

        // Mark replacement on old token (requires update unless tracked entity handles it, but just setting property is enough for tracking)
        refreshToken.ReplacedByToken = newTokens.RefreshToken;

        // Since we called SaveChanges in GenerateAuthResponseAsync, the revocation might have been saved already?
        // Wait, GenerateAuthResponseAsync calls Add and Save.
        // It saves all tracked changes. So yes, revocation is saved.
        // However, we set ReplacedByToken AFTER generating new token.
        // So we need another Save?
        // Yes.

        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return newTokens;
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(User user, CancellationToken cancellationToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "supersecretkey1234567890abcdef");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("name", user.Name)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpireMinutes"] ?? "60")),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["Jwt:Issuer"]
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = tokenHandler.WriteToken(token);

        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            Expires = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow,
            UserId = user.Id
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResponse(jwtToken, refreshToken.Token, user.Id, user.Email, user.Name);
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

}
