using System.Security.Cryptography;
using BluecoreApi.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
namespace BluecoreApi.Tests;

public sealed class AuthenticationConfigurationTests
{
    [Theory]
    [InlineData("key")]
    [InlineData("issuer")]
    [InlineData("audience")]
    [InlineData("expiration")]
    public void Configuration_RejectsUnsafeOrIncompleteSettings(string failure)
    {
        using var services = new ServiceCollection().AddLogging().AddApiAuthentication(
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = failure == "key" ? "short" : Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)),
                ["Jwt:Issuer"] = failure == "issuer" ? "" : "tests",
                ["Jwt:Audience"] = failure == "audience" ? "" : "tests",
                ["Jwt:ExpirationMinutes"] = failure == "expiration" ? "0" : "60"
            }).Build()).BuildServiceProvider();
        Assert.Throws<OptionsValidationException>(() => services.GetRequiredService<IOptions<JwtOptions>>().Value);
    }
}
