using Location.Models;
using Location.Repository.Generic;
using Microsoft.AspNetCore.Mvc;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using Location.UnitOfWork;
using LoggerService;
using Location.Domain.Entities;
using Location.Domain;
using Location.Models.Constant;
using AutoMapper;
using MIS.Domain.Enums;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Location.Controllers
{
    /// <summary>
    /// The Location Controller.
    /// </summary>
    [Route("api/v1")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILoggerManager _logger;
        private readonly IGenericRepository<LocationEntity> _locationRepository;
        private readonly IUnitOfWork<LocationDbContext> _unitOfWork;

        public LocationController(LocationDbContext dbContext, ILoggerManager logger, IMapper mapper)
        {
            _mapper = mapper;
            _logger = logger;
            _unitOfWork = new UnitOfWork<LocationDbContext>(dbContext);
            _locationRepository = new GenericRepository<LocationEntity>(dbContext);
        }

        /// <summary>
        /// Add location in db
        /// </summary>
        /// <param name="dto">The Location dto.</param>
        /// <returns>An ActionResult.</returns>
        [HttpPost]
        [Route("/addLocation")]
        public async Task<ActionResult<List<LocationEntity>>> AddLocation([FromBody] LocationDTO dto)
        {
            try
            {
                // Checking if the passed DTO is valid
                if (!ModelState.IsValid || dto == null)
                    return BadRequest(new ApiResponseModel(ApiStatus.Error, "Location was empty"));

                dto.Hour = dto.Hour == null ? DateTime.Now.TimeOfDay : dto.Hour;

                // Begin the Transaction
                _unitOfWork.CreateTransaction();

                var locationEntity = _mapper.Map<LocationEntity>(dto);
                await _locationRepository.InsertAsync(locationEntity);

                //save the transaction
                _unitOfWork.Save();

                //commit the transaction
                _unitOfWork.Commit();

                // Log info message
                _logger.LogInfo("New location successfully added");

                return Ok(new ApiResponseModel(dto, "Location added successfully"));
            }
            catch (Exception ex)
            {
                //rollback the transaction
                _unitOfWork.Rollback();

                // Log error message
                _logger.LogError("Error adding location");

                return BadRequest(new ApiResponseModel(ApiStatus.Error, ApiErrors.DefaultError.GetDescription(), ex));
            }
        }

        /// <summary>
        /// Gets all locations from csv file.
        /// </summary>
        /// <param name="file">The IFormFile.</param>
        /// <returns>The list of LocationDTO.</returns>
        [HttpPost("/getLocationsFromCsv")]
        public ActionResult<List<LocationDTO[]>> GetLocationsFromCsv(IFormFile file)
        {
            try
            {
                // Create a new StreamReader to read the contents of the CSV file
                using var reader = new StreamReader(file.OpenReadStream());

                // Create a new CsvReader to parse the contents of the CSV file
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ",",
                    HasHeaderRecord = true,
                    IgnoreBlankLines = true,
                });

                List<LocationDTO> locations = new List<LocationDTO>();
                TimeSpan startTime = TimeSpan.Parse(LocationConstant.StartingTime);
                TimeSpan endTime = TimeSpan.Parse(LocationConstant.EndingTime);

                //reading lines from csv file
                while (csv.Read())
                {
                    string? location = csv.GetField<string>(0);
                    dynamic hours = IsValidTimeFormat(csv.GetField<string>(1));

                    if (hours == null)
                        continue;

                    LocationDTO loc = new LocationDTO
                    {
                        Location = location,
                        Hour = hours
                    };

                    // If the hours value is between the start and end times, add the
                    // LocationDTO object to the locations list
                    if (hours <= endTime && hours >= startTime)
                    {
                        locations.Add(loc);
                    }
                }

                // Log info message
                _logger.LogInfo("Getting all locations from csv based on condition");

                return Ok(new ApiResponseModel(locations, "CSV file parsed successfully"));

            }
            catch (Exception ex)
            {
                // Log error message
                _logger.LogError("Error occured while parsing CSV");
                return BadRequest(new ApiResponseModel(ApiStatus.Error, ApiErrors.DefaultError.GetDescription(), ex));
            }
        }

        /// <summary>
        /// Gets all locations from db.
        /// </summary>
        /// <returns>The list of LocationDTO.</returns>
        [HttpGet("/getLocationsFromDB")]
        public ActionResult<List<string[]>> GetLocationsFromDB()
        {
            try
            {
                //get all locations from db
                var allLocations = _locationRepository.GetAll().ToList();

                TimeSpan startTime = TimeSpan.Parse(LocationConstant.StartingTime);
                TimeSpan endTime = TimeSpan.Parse(LocationConstant.EndingTime);

                List<LocationDTO> filteredLocations = new List<LocationDTO>();

                //fill out the location dto by filtering start and end time
                foreach (var location in allLocations)
                {
                    TimeSpan currentTime = TimeSpan.Parse(location.Hour.ToString());

                    // If the hours value is between the start and end times, add the
                    // LocationDTO object to the locations list
                    if (currentTime <= endTime && currentTime >= startTime)
                        filteredLocations.Add(_mapper.Map<LocationDTO>(location));
                }

                // Log info message
                _logger.LogInfo("Getting all locations from db based on condition");

                return Ok(new ApiResponseModel(filteredLocations, "Locations received successfully"));
            }
            catch (Exception ex)
            {
                // Log error message
                _logger.LogError("Internal error was occured while fetching locations on a condition from Database");
                return BadRequest(new ApiResponseModel(ApiStatus.Error, ApiErrors.DefaultError.GetDescription(), ex));
            }
        }

        public static dynamic IsValidTimeFormat(string input)
        {
            TimeSpan dummyOutput;

            if (TimeSpan.TryParse(input, out dummyOutput))
                return TimeSpan.Parse(input);

            return null;
        }
    }
}

