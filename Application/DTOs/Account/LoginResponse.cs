namespace AuthGDPR.Application.DTOs.Account
{
    public class LoginResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
