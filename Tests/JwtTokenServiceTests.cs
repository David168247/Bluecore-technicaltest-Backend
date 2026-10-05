using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using BluecoreApi.Configuration;
using BluecoreApi.Models;
using BluecoreApi.Services.Implementations;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit;
namespace BluecoreApi.Tests;

public sealed class JwtTokenServiceTests
{
    private readonly JwtOptions _options = new()
    {
        Key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)),
        Issuer = "test-issuer", Audience = "test-audience", ExpirationMinutes = 5
    };

    [Fact]
    public void Token_HasValidSignatureSubjectAndExpiration()
    {
        var account = new UserAccount { Username = "tester" };
        var response = new JwtTokenService(Options.Create(_options), TimeProvider.System).CreateToken(account);
        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
        var principal = handler.ValidateToken(response.AccessToken, Parameters(), out var token);
        Assert.Equal(account.Id.ToString(), principal.FindFirst("sub")!.Value);
        Assert.Equal("tester", principal.FindFirst("unique_name")!.Value);
        Assert.NotNull(principal.FindFirst("jti"));
        Assert.InRange(token.ValidTo, DateTime.UtcNow.AddMinutes(4), DateTime.UtcNow.AddMinutes(6));
        Assert.DoesNotContain("password", response.AccessToken, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("issuer")]
    [InlineData("audience")]
    [InlineData("signature")]
    [InlineData("expired")]
    public void Validation_RejectsInvalidToken(string failure)
    {
        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            failure == "issuer" ? "wrong" : _options.Issuer,
            failure == "audience" ? "wrong" : _options.Audience,
            notBefore: now.AddMinutes(-10),
            expires: failure == "expired" ? now.AddMinutes(-1) : now.AddMinutes(5),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(
                failure == "signature" ? RandomNumberGenerator.GetBytes(32) : Encoding.UTF8.GetBytes(_options.Key)), SecurityAlgorithms.HmacSha256));
        Assert.ThrowsAny<SecurityTokenException>(() =>
            new JwtSecurityTokenHandler().ValidateToken(new JwtSecurityTokenHandler().WriteToken(token), Parameters(), out _));
    }

    private TokenValidationParameters Parameters() => new()
    {
        ValidateIssuer = true, ValidIssuer = _options.Issuer,
        ValidateAudience = true, ValidAudience = _options.Audience,
        ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
        ValidateLifetime = true, RequireExpirationTime = true,
        ValidAlgorithms = [SecurityAlgorithms.HmacSha256], ClockSkew = TimeSpan.Zero
    };
}
