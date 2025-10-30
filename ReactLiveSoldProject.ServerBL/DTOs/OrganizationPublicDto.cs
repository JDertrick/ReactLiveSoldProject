namespace ReactLiveSoldProject.ServerBL.DTOs
{
    /// <summary>
    /// DTO público para Organization - Solo contiene información segura para mostrar en el portal
    /// NUNCA incluir: PrimaryContactEmail, PlanType, IsActive, etc.
    /// </summary>
    public class OrganizationPublicDto
    {
        public string Name { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
    }
}
