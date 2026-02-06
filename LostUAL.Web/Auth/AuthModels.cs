namespace LostUAL.Web.Auth;

public sealed class LoginRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public sealed class LoginResponse
{
    public string Token { get; set; } = "";
}
