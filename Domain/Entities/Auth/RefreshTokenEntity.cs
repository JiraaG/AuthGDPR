namespace AuthGDPR.Domain.Entities.Auth
{
    public class RefreshTokenEntity
    {
        public int Id { get; set; }
        // Identificatore univoco del token (claim "tid")
        public string TokenId { get; set; }
        // Id reale dell’utente (ciò che verrà pseudonimizzato nei token)
        public Guid UserId { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }
    }
}
