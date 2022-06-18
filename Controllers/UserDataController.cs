using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using WebVezdekod.DTO;
using WebVezdekod.Models;

namespace WebVezdekod.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserDataController : ControllerBase
{
    private readonly string _connectionString;
    private readonly ILogger<UserDataController> _logger;

    public UserDataController(ILogger<UserController> logger)
    {
        _connectionString = Environment.GetEnvironmentVariable("MainDB");
        if (_connectionString == null) throw new Exception("Connection string not specified");
    }

    [HttpPost("/editprofile")]
    public async Task<ResponseModel<int>> EditProfile([Bind("profile")] UserProfileRequestModel profile)
    {
        NpgsqlConnection connection = null;
        try
        {
            await using (connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var user_id = (await connection.QueryAsync<int>("select id from users", new {@token = profile.token})).FirstOrDefault();
                foreach (var t in profile.user_know)
                {
                    await connection.ExecuteAsync("insert into user_know(user_id, technology) values (@user_id,@tech)",
                        new {@user_id = user_id, @tech = t});
                }
                
                foreach (var t in profile.user_want_know)
                {
                    await connection.ExecuteAsync("insert into user_want_know(user_id, technology) values (@user_id,@tech)",
                        new {@user_id = user_id, @tech = t});
                }

                await connection.ExecuteAsync("insert into user_data(want_mentor, search_mentor, about, time_start, time_finish, user_id) VALUES " +
                                              "(@want_mentor, @search_mentor, @about, @time_start, @time_finish, @user_id)", new
                {
                    @want_mentor=profile.want_mentor, @search_mentor=profile.search_mentor,
                    @about=profile.about, @time_start=profile.time_start,
                    @time_finish=profile.time_finish, @user_id = user_id
                });
                
                await connection.CloseAsync();
                return new ResponseModel<int> {success = true};
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return new ResponseModel<int> {success = false};;
        }
        finally
        {
            connection?.CloseAsync();
        }
    }

    [HttpPost("/getprofile")]
    public async Task<ProfileDTO> GetProfile([Bind("token")] TokenModel token)
    {
        NpgsqlConnection connection = null;
        try
        {
            await using (connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var user_id = (await connection.QueryAsync<int>("select id from users", new {@token = token.token})).FirstOrDefault();
                var data = (await connection.QueryAsync<ProfileDTO>(
                    @"select want_mentor, search_mentor, about, time_start, time_finish, user_id from user_data where user_id = @user_id",
                    new {@user_id = user_id})).FirstOrDefault();
                var user_know = (await connection.QueryAsync<string>(
                    @"select technology from user_know where user_know.user_id = @user_id",
                    new {@user_id = user_id})).AsList();
                var user_want_know = (await connection.QueryAsync<string>(
                    @"select technology from user_want_know where user_want_know.user_id = @user_id",
                    new {@user_id = user_id})).AsList();
                return new ProfileDTO()
                {
                    know_list = user_know, about = data.about, search_mentor = data.search_mentor,
                    time_finish = data.time_finish, time_start = data.time_start, want_know_list = user_want_know
                };
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
        finally
        {
            await connection?.CloseAsync();
        }
    }

    [HttpGet("/gettech")]
    public async Task<GetTechnologyResponse> GetTechnologies()
    {
        NpgsqlConnection connection = null;
        try
        {
            await using (connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var res = (await connection.QueryAsync<string>("select name from technology")).AsList();
                return new GetTechnologyResponse()
                {
                    technologies = res
                };
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
        finally
        {
            await connection?.CloseAsync();
        } 
    }
}