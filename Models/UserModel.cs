namespace WebVezdekod.Models;

public class SignUpRequest
{
    public string name { get; set; }
    public string login { get; set; }
    public string password { get; set; }
    public string last_name { get; set; }
}

public class LoginRequest
{
    public string login { get; set; }
    public string password { get; set; }
}

public class UserSummary
{
    public string name { get; set; }
    public string login { get; set; }
    public string last_name { get; set; }
}