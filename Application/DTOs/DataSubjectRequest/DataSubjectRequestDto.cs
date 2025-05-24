using AuthGDPR.Domain.Enums;

namespace AuthGDPR.Application.DTOs.DataSubjectRequest
{
    public class DataSubjectRequestDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public RequestType RequestType { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public ResponseIdentity ResponseIdentity { get; set; }
        public RequestStatus Status { get; set; }
        public string? Description { get; set; }
        public string TraceIdentifier { get; set; } = string.Empty;
        public string? IPAddress { get; set; }
    }

}
