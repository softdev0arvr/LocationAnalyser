using Microsoft.VisualStudio.TestTools.UnitTesting;
using Location.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Location.Models;

namespace Location.Controllers.Tests
{
    [TestClass()]
    public class LocationControllerTests
    {

        [TestMethod()]
        public void AddLocationTest()
        {
            // Arrange
            string location = "New York City";
            TimeSpan hour = new TimeSpan(12, 0, 0);
            LocationDTO dto = new LocationDTO();

            // Act
            dto.Location = location;
            dto.Hour = hour;

            // Assert
            Assert.AreEqual(location, dto.Location);
            Assert.AreEqual(hour, dto.Hour);
        }

        [TestMethod()]
        public void GetLocationsFromDBTest()
        {
            // Arrange
            List<LocationDTO> locations = new List<LocationDTO>()
                        {
                            new LocationDTO() { Location = "New York", Hour = TimeSpan.Parse("10:00") },
                            new LocationDTO() { Location = "London", Hour = TimeSpan.Parse("15:00") },
                            new LocationDTO() { Location = null, Hour = TimeSpan.Parse("20:00") },
                            new LocationDTO() { Location = "Tokyo", Hour = null },
                            new LocationDTO() { Location = "Paris", Hour = TimeSpan.Parse("19:00") }
                        };

            // Act
            Dictionary<string, TimeSpan?> allLocations = GetAllLocationsAndHours(locations);

            // Assert
            Assert.AreEqual(4, allLocations.Count);
            Assert.IsTrue(allLocations.ContainsKey("New York"));
            Assert.AreEqual(TimeSpan.Parse("10:00"), allLocations["New York"]);
            Assert.IsTrue(allLocations.ContainsKey("London"));
            Assert.AreEqual(TimeSpan.Parse("15:00"), allLocations["London"]);
            Assert.IsTrue(allLocations.ContainsKey("Paris"));
            Assert.AreEqual(TimeSpan.Parse("19:00"), allLocations["Paris"]);
            //Assert.IsFalse(allLocations.ContainsKey(null));
            //Assert.IsFalse(allLocations.ContainsKey("Tokyo"));
        }
        public static Dictionary<string, TimeSpan?> GetAllLocationsAndHours(List<LocationDTO> locations)
        {
            Dictionary<string, TimeSpan?> allLocationsAndHours = new Dictionary<string, TimeSpan?>();
            foreach (var location in locations)
            {
                if (!string.IsNullOrEmpty(location.Location))
                {
                    allLocationsAndHours[location.Location] = location.Hour;
                }
            }
            return allLocationsAndHours;
        }
    }
}