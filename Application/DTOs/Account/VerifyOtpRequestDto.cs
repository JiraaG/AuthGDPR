namespace AuthGDPR.Application.DTOs.Account
{
    public class VerifyOtpRequestDto
    {
        public string ChallengeId { get; set; }
        public string Otp { get; set; }
    }

}
