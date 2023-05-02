using System.ComponentModel.DataAnnotations;

namespace Location.Domain.Entities
{
    /// <summary>
    /// The Location Entity.
    /// </summary>
    public class LocationEntity
    {
        [Key]
        public int Id { get; set; }
        public string Location { get; set; }
        public TimeSpan Hour { get; set; }
    }
}
