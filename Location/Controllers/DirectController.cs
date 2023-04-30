using Location.Constant;
using Location.Models;
using Location.Repository.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Nodes;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Configuration;
using System.Text.Encodings.Web;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using Microsoft.EntityFrameworkCore;
using Location.UnitOfWork;
using Location.IRepository;
using NuGet.Protocol.Core.Types;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using LoggerService;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Location.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class DirectController : ControllerBase
    {
        private readonly ILoggerManager _logger;
        private readonly IGenericRepository<LocationModel> _locationRepository;
        private readonly IUnitOfWork<DatabaseContextCla> _unitOfWork;
        public DirectController(DatabaseContextCla dbContext, ILoggerManager logger)
        {
            _logger = logger;
            _unitOfWork = new UnitOfWork<DatabaseContextCla>(dbContext);
            _locationRepository = new GenericRepository<LocationModel>(dbContext);
        }

        //UoW-below
        [HttpPost]
        [Route("/uow-get")]
        public ActionResult<List<string[]>> UowGet()
        {
            _logger.LogInfo("ActionMethod Called: UowGet");
            try
            {
                var locations = _locationRepository.GetAll().ToList();
                var uowLocations = _locationRepository.GetAll().ToList();

                TimeSpan startTime = TimeSpan.Parse(LocationConstant.StartingTime);
                TimeSpan endTime = TimeSpan.Parse(LocationConstant.EndingTime);
                List<LocationModel> filteredLocations = new List<LocationModel>();

                foreach (var location in uowLocations)
                {
                    TimeSpan currentTime = TimeSpan.Parse(location.Hour.ToString("H:mm"));

                    if (currentTime <= endTime && currentTime >= startTime)
                    {
                        filteredLocations.Add(location);
                    }
                }

                _logger.LogInfo("Locations were received.");
                return Ok(new
                {
                    StatusCode = 200,
                    Message = "Locations received successfully",
                    Data = filteredLocations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting locations");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    StatusCode = 500,
                    Message = ex.Message
                });
            }
        }

        [HttpPost]
        [Route("/dbaddlocation")]
        public async Task<ActionResult<List<Models.LocationModel>>> Dbaddlocation([FromBody] JsonObject data)
        {
            _logger.LogInfo("ActionMethod Called: Dbaddlocation");
            try
            {
                dynamic location = data["location"];
                dynamic hour = IsValidTimeFormat((string)data["hour"]);

                if (string.IsNullOrEmpty((string)location))
                    return BadRequest(
                        new
                        {
                            StatusCode = 400,
                            Message = "Location was empty"
                        });

                if(hour == null)
                    hour = DateTime.Now.TimeOfDay;

                var locationHourRange = new Models.LocationModel
                {
                    Location = (string)location,
                    Hour = hour
                };
                // Begin the Transaction
                _unitOfWork.CreateTransaction();
                await _locationRepository.InsertAsync(locationHourRange);
                _unitOfWork.Save();
                _unitOfWork.Commit();

                // await _locationRepository.InsertAsync(locationHourRange);
                _logger.LogInfo("New location successfully added");
                return Ok(new
                {
                    StatusCode = 200,
                    Message = "Success",
                    Data = locationHourRange
                });
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                _logger.LogError("Error creating location");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    StatusCode = 500,
                    Message = ex.Message
                });

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
                return Ok(new
                {
                    StatusCode = 200,
                    Message = "CSV file parsed successfully",
                    Data = locations
            });
            }
            catch (Exception ex)
            { 
                _logger.LogError("Error was occured while parsing CSV");

                return BadRequest(new
                {
                    StatusCode = 500,
                    Message = ex.Message,
                });
            }
        }


        [HttpPost("/getLocationsFromDB")]
        public ActionResult<List<string[]>> GetLocationsFromDB()
        {
            _logger.LogInfo("ActionMethod Called: GetLocationsFromDB");
            try
            {
                var locations = _locationRepository.GetAll().ToList();

                TimeSpan startTime = TimeSpan.Parse(LocationConstant.StartingTime);
                TimeSpan endTime = TimeSpan.Parse(LocationConstant.EndingTime);
                List<LocationModel> filteredLocations = new List<LocationModel>();

                foreach (var location in locations)
                {
                    TimeSpan currentTime = TimeSpan.Parse(location.Hour.ToString("H:mm"));

                    if (currentTime <= endTime && currentTime >= startTime)
                    {
                        filteredLocations.Add(location);
                    }
                }

                _logger.LogInfo("Locations on a condition were fetched from database");
                return Ok(new
                {
                    StatusCode = 200,
                    Message = "Locations received successfully",
                    Data = filteredLocations
                });
            }
            catch (Exception ex)
            {

                _logger.LogError("Internal error was occured while fetching locations on a condition from Database");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    StatusCode = 500,
                    Message = ex.Message
                });
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

