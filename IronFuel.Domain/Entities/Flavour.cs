namespace IronFuel.Domain.Entities
{
    public class Flavour : BaseEntity
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = null!;
    }
}
