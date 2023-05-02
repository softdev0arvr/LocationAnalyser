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

        [HttpPost]
        [Route("/addLocation")]
        public async Task<ActionResult<List<LocationEntity>>> AddLocation([FromBody] LocationDTO dto)
        {
            _logger.LogInfo("ActionMethod Called: Dbaddlocation");
            try
            {
                // Checking if the passed DTO is valid
                if (!ModelState.IsValid || dto == null)
                    return BadRequest(new ApiResponseModel(ApiStatus.Error, "Location was empty"));

                dto.Hour = dto.Hour == null ? DateTime.Now.TimeOfDay : dto.Hour;

                var locationEntity = _mapper.Map<LocationEntity>(dto);

                // Begin the Transaction
                _unitOfWork.CreateTransaction();
                await _locationRepository.InsertAsync(locationEntity);
                _unitOfWork.Save();
                _unitOfWork.Commit();

                // await _locationRepository.InsertAsync(locationHourRange);
                _logger.LogInfo("New location successfully added");

                return Ok(new ApiResponseModel(dto, "Success"));
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                _logger.LogError("Error creating location");
                return BadRequest(new ApiResponseModel(ApiStatus.Error, ApiErrors.DefaultError.GetDescription(), ex));
            }

        }

        [HttpPost("/getLocationsFromCsv")]
        public ActionResult<List<string[]>> GetLocationsFromCsv(IFormFile file)
        {

            _logger.LogInfo("ActionMethod Called: GetLocationsFromCsv");
            try
            {
                using var reader = new StreamReader(file.OpenReadStream());
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ",",
                    HasHeaderRecord = true,
                    IgnoreBlankLines = true,
                });
                List<LocationDTO> locations = new List<LocationDTO>();
                TimeSpan startTime = TimeSpan.Parse(LocationConstant.StartingTime);
                TimeSpan endTime = TimeSpan.Parse(LocationConstant.EndingTime);

                while (csv.Read())
                {
                    string location = csv.GetField<string>(0);
                    dynamic hours = IsValidTimeFormat(csv.GetField<string>(1));

                    if (hours == null)
                        continue;

                    LocationDTO loc = new LocationDTO
                    {
                        Location = location,
                        Hour = hours
                    };

                    if (hours <= endTime && hours >= startTime)
                    {
                        locations.Add(loc);
                    }
                }
                _logger.LogInfo("Locations from the csv was passed successfully");

                return Ok(new ApiResponseModel(locations, "CSV file parsed successfully"));

            }
            catch (Exception ex)
            {
                _logger.LogError("Error was occured while parsing CSV");
                return BadRequest(new ApiResponseModel(ApiStatus.Error, ApiErrors.DefaultError.GetDescription(), ex));
            }
        }

        [HttpPost("/getLocationsFromDB")]
        public ActionResult<List<string[]>> GetLocationsFromDB()
        {
            _logger.LogInfo("ActionMethod Called: GetLocationsFromDB");
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
                    TimeSpan currentTime = TimeSpan.Parse(location.Hour.ToString("HH:mm"));

                    if (currentTime <= endTime && currentTime >= startTime)
                        filteredLocations.Add(_mapper.Map<LocationDTO>(location));
                }

                _logger.LogInfo("Getting all locations from db");

                return Ok(new ApiResponseModel(filteredLocations, "Locations received successfully"));
            }
            catch (Exception ex)
            {

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

