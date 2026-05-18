namespace IronFuel.Domain.Entities
{  
    public class ApplicationUser : IdentityUser
    {
        public bool IsDeleted { get; set; }
        public string? CreatedById { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public string? LastUpdatedById { get; set; }
        public DateTime? LastUpdatedOn { get; set; } = null;
        public List<RefreshToken> RefreshTokens { get; set; } = new();
    }
}
