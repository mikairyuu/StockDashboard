using WebVezdekod.DTO;

namespace WebVezdekod.Models;

public class UserProfileRequestModel : ProfileDTO
{
    public string token { get; set; }
    public List<string> user_know { get; set; }
    public List<string> user_want_know { get; set; }
}

public class GetTechnologyResponse
{
    public List<string> technologies { get; set; }
}