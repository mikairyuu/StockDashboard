using System.Security.Cryptography;
using Dapper;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using WebVezdekod.DTO;
using WebVezdekod.Models;

namespace WebVezdekod.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly string _connectionString;
    private readonly ILogger<UserController> _logger;

    public UserController(ILogger<UserController> logger)
    {
        _connectionString = Environment.GetEnvironmentVariable("MainDB");
        if (_connectionString == null) throw new Exception("Connection string not specified");
        _logger = logger;
    }

    [HttpPost("/signup")]
    public async Task<ActionResult<string>> SignUp([Bind("User")] SignUpRequest signUpRequest)
    {
        NpgsqlConnection connection = null;
        try
        {
            await using (connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var hashAndSalt = GenerateHash(signUpRequest.password);
                var token = Guid.NewGuid();
                var ans = await connection.ExecuteAsync(
                    @"insert into users (name,username,last_name,hash,salt,token) " +
                    "values (@name,@last_name, @username,@hash,@salt,@token)", new
                    {
                        @name = signUpRequest.name, @last_name = signUpRequest.last_name,
                        @username = signUpRequest.login,
                        @hash = hashAndSalt.Key, @salt = hashAndSalt.Value, @token = token.ToString()
                    });
                await connection.CloseAsync();
                return ans > 0 ? token.ToString() : "";
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return "";
        }
        finally
        {
            connection?.CloseAsync();
        }
    }

    [HttpPost("/login")]
    public async Task<ResponseModel<UserSummary>> Login([Bind("User")] LoginRequest loginRequest)
    {
        NpgsqlConnection connection = null;
        try
        {
            await using (connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var ans = await connection.QueryAsync<UserDTO>(
                    @"select * from users where username = @username",
                    new {@username = loginRequest.login});
                var user = ans.FirstOrDefault();
                if (GenerateHashFromSalt(loginRequest.password, user.salt) != user.hash)
                    user = null;
                return new ResponseModel<UserSummary>
                    {success = true, data = new UserSummary {last_name = user.last_name, login = user.username, name = user.name}};
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return new ResponseModel<UserSummary>{success = false, data = null};
        }
        finally
        {
            await connection?.CloseAsync();
        }
    }
    
    [HttpPost("/getuser")]
    public async Task<UserSummary> GetUser([Bind("token")] TokenModel token)
    {
        NpgsqlConnection connection = null;
        try
        {
            await using (connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var ans = await connection.QueryAsync<UserSummary>(@"select * from users where token = @token",
                    new {@token = token.token});
                return ans.FirstOrDefault();
            }
        }
        catch (Exception e)
        {
            return null;
        }
        finally
        {
            await connection?.CloseAsync();
        }
    }


    private KeyValuePair<string, string> GenerateHash(string s)
    {
        var salt = new byte[128 / 8];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        var stringSalt = Convert.ToBase64String(salt);
        var hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(s, salt, KeyDerivationPrf.HMACSHA1, 1000, 256 / 8));
        return new KeyValuePair<string, string>(hash, stringSalt);
    }

    private static string GenerateHashFromSalt(string s, string strSalt) => Convert.ToBase64String(
        KeyDerivation.Pbkdf2(s, Convert.FromBase64String(strSalt), KeyDerivationPrf.HMACSHA1, 1000, 256 / 8));
}