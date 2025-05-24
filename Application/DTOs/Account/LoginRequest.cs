namespace AuthGDPR.Application.DTOs.Account
{
    public class LoginRequest
    {
        public string UsernameOrEmail { get; set; }
        public string Password { get; set; }
    }
}
