namespace AuthGDPR.Application.DTOs.Account
{
    public class TwoFactorChallenge
    {
        public string ChallengeId { get; set; }
        public Guid UserId { get; set; }
        public string Otp { get; set; }
        public DateTime CreatedAt { get; set; }
        public int AttemptCount { get; set; }
    }
}
