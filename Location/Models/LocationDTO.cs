using Location.Domain.Entities;

namespace Location.Models
{
    /// <summary>
    /// The Location DTO.
    /// </summary>
    public class LocationDTO
    {
        public string? Location { get; set; }
        public TimeSpan? Hour { get; set; }

        public Dictionary<string, string> serialize()
        {
            return new Dictionary<string, string> {
                { "Location",Location },
                { "Hour",Hour.ToString() }
            };
        }

        public static Dictionary<string, string> serializeLocObj(LocationEntity model)
        {
            return new LocationDTO() { Location = model.Location, Hour = model.Hour }.serialize();
        }

        public static List<Dictionary<string, string>> serializeLocList(List<LocationEntity> modelList)
        {
            var serializedList = new List<Dictionary<string, string>>();
            foreach (var model in modelList)
            {
                serializedList.Add(serializeLocObj(model));
            }
            return serializedList;
        }
    }
}
