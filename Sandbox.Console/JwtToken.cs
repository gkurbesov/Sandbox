using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Sandbox.Console;

public static class JwtToken
{
    public static void Execute()
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("testvtr5b5g4f3cfvb6unyb5gtvrcfeKey"));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var Sectoken = new JwtSecurityToken("testIssuer",
            "testAudience",
            null,
            expires: DateTime.Now.AddMinutes(120),
            signingCredentials: credentials);

        var token = new JwtSecurityTokenHandler().WriteToken(Sectoken);

        System.Console.WriteLine(token);
    }
}