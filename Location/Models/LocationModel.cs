﻿using System.ComponentModel.DataAnnotations;

namespace Location.Models
{
    public class LocationModel
    {
        [Key]
        public int Id { get; set; }
        public string Location { get; set; }
        public TimeSpan Hour { get; set; }
    }
}
