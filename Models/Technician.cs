public enum TechnicianStatus { PendingVerification, Active, Inactive }

public class Technician
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public int ProfessionId { get; set; }
    public TechnicianStatus Status { get; set; }
    public bool CertificatesUploaded { get; set; }
}
