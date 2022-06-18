namespace WebVezdekod.Models;

public class ResponseModel<T>
{
    public bool success { get; set; }
    public T? data { get; set; }
}

public class TokenModel
{
    public string token { get; set; }
}