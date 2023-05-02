using AutoMapper;
using Location.Domain.Entities;

namespace Location.Models.Mapping
{
    /// <summary>
    /// The mapping profile.
    /// </summary>
    public class MappingProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MappingProfile"/> class.
        /// </summary>
        public MappingProfile()
        {
            // LocationDTO
            CreateMap<LocationDTO, LocationEntity>();
            CreateMap<LocationEntity, LocationDTO>();
        }
    }
}

