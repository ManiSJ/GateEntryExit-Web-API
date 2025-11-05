using GateEntryExit.Domain;
using GateEntryExit.Domain.Manager;
using GateEntryExit.Dtos.Gate;
using GateEntryExit.Dtos.GateExit;
using GateEntryExit.Dtos.Sensor;
using GateEntryExit.Dtos.Shared;
using GateEntryExit.Helper;
using GateEntryExit.Repositories;
using GateEntryExit.Repositories.Interfaces;
using GateEntryExit.Service.Cache;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OfficeOpenXml;
using Scryber.Components;
using System.Linq.Expressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GateEntryExit.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/sensor")]
    public class SensorController : ControllerBase
    {
        private readonly ISensorRepository _sensorRepository;

        private readonly ISensorManager _sensorManager;

        private readonly IGuidGenerator _guidGenerator;

        private readonly ICacheService _cacheService;

        public SensorController(ISensorRepository sensorRepository,
            ISensorManager sensorManager,
            IGuidGenerator guidGenerator,
            ICacheService cacheService)
        {
            _sensorRepository = sensorRepository;
            _sensorManager = sensorManager;
            _guidGenerator = guidGenerator;
            _cacheService = cacheService;
        }

        [Route("create")]
        [HttpPost]
        public async Task<SensorDetailsDto> CreateAsync(CreateSensorDto input)
        {
            var isGateAlreadyHasSensor = await _sensorRepository.IsGateAlreadyHasSensorAsync(input.GateId);

            if (isGateAlreadyHasSensor)
                throw new Exception("Selected gate has a sensor");

            var sensor = _sensorManager.Create(_guidGenerator.Create(), input.GateId, input.Name);
            _cacheService.RemoveDatas("getAllSensors-*");
            await _sensorRepository.InsertAsync(sensor);

            return new SensorDetailsDto()
            {
                Id = sensor.Id,
                Name = sensor.Name,
                GateDetails = new GateDetailsDto()
                {
                    Id = sensor.GateId
                }
            };
        }

        [Route("edit")]
        [HttpPost]
        public async Task<SensorDetailsDto> EditAsync(UpdateSensorDto input)
        {
            await _sensorRepository.UpdateAsync(input.Id, input.Name);
            var sensor = await _sensorRepository.GetAsync(input.Id);

            _cacheService.RemoveDatas("getAllSensors-*");
            _cacheService.RemoveDatas($"getSensorById-{input.Id}");

            return new SensorDetailsDto()
            {
                Id = sensor.Id,
                Name = sensor.Name,
                GateDetails = new GateDetailsDto()
                {
                    Id = sensor.GateId
                }
            };
        }

        [HttpDelete("delete/{id}")]
        public async Task DeleteAsync(Guid id)
        {
            await _sensorRepository.DeleteAsync(id);
            _cacheService.RemoveDatas("getAllSensors-*");
        }

        [Route("getById/{id}")]
        [HttpPost]
        public async Task<SensorDetailsDto> GetByIdAsync(Guid id)
        {
            var cacheKey = $"getSensorById-";
            cacheKey = cacheKey + $"{id}";

            var cacheData = _cacheService.GetData<SensorDetailsDto>(cacheKey);

            if (cacheData != null)
            {
                return cacheData;
            }

            var sensor = await _sensorRepository.GetAsync(id);

            cacheData = new SensorDetailsDto()
            {
                Id = sensor.Id,
                Name = sensor.Name,
                GateDetails = new GateDetailsDto()
                {
                    Id = sensor.GateId,
                    Name = sensor.Gate.Name
                }
            };
            _cacheService.SetData<SensorDetailsDto>(cacheKey, cacheData, DateTime.Now.AddSeconds(30));

            return cacheData;
        }


        [Route("getAll")]
        [HttpPost]
        public async Task<GetAllSensorsDto> GetAllAsync(GetAllDto input)
        {
            var cacheKey = $"getAllSensors-{input.SkipCount}-{input.MaxResultCount}-{input.Sorting}";
            var cacheData = _cacheService.GetData<GetAllSensorsDto>(cacheKey);

            if (cacheData != null)
            {
                return cacheData;
            }

            var result = new List<SensorDetailsDto>();

            var sensorsQueryabe = _sensorRepository.GetAll();

            var totalCount = await sensorsQueryabe.CountAsync();

            if (input != null)
            {
                sensorsQueryabe = sensorsQueryabe.OrderBy(p => p.Gate.Name).Skip(input.SkipCount).Take(input.MaxResultCount);
            }

            if (sensorsQueryabe.Count() > 0)
            {
                result = sensorsQueryabe.Select(s => new SensorDetailsDto()
                {
                    GateDetails = new GateDetailsDto()
                    {
                        Name = s.Gate.Name,
                        Id = s.Gate.Id
                    },
                    Id = s.Id,
                    Name = s.Name
                }).OrderBy(p => p.GateDetails.Name).ToList();
            }

            cacheData = new GetAllSensorsDto { Items = result, TotalCount = totalCount };
            _cacheService.SetData<GetAllSensorsDto>(cacheKey, cacheData, DateTime.Now.AddSeconds(30));
            return cacheData;
        }

        [Route("getAllWithDetailsExcelReport")]
        [HttpPost]
        public async Task GetAllWithDetailsExcelReportAsync(GetAllSensorWithDetailsReportInputDto input)
        {
            var allSensorWithDetailsQueryable = _sensorRepository.GetAllWithDetails();
            allSensorWithDetailsQueryable = FilterQuery(allSensorWithDetailsQueryable, input.GateIds, input.FromDate, input.ToDate);
            var allSensorWithDetails = await allSensorWithDetailsQueryable
                                                .OrderBy(p => p.Gate.Name)
                                                .ToListAsync();

            var getAllSensorWithDetails = GetAllSensorWithDetails(allSensorWithDetails, input.FromDate, input.ToDate);
            var sensorDetails = getAllSensorWithDetails.Items;

            CreateExcelReport(sensorDetails);
        }

        private void CreateExcelReport(List<SensorDetailsDto> sensorDetails)
        {
            string excelFilePath = System.IO.Path.GetFullPath(System.IO.Path.Combine("Report", "Excel", "Export", "SensorWithDetails.xlsx"));
            FileInfo excelFile = new FileInfo(excelFilePath);

            DeleteIfFileExists(excelFile);

            using (ExcelPackage excelPackage = new ExcelPackage(excelFile))
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("SensorWithDetails");

                // Define column headers
                var columnHeaders = new Dictionary<Expression<Func<SensorDetailsDto, object>>, string>
                {
                    { obj => obj.Name, "Sensor name" },
                    { obj => obj.GateDetails.Name, "Gate name" },
                    { obj => obj.GateDetails.EntryCount, "Gate entry count" },
                    { obj => obj.GateDetails.ExitCount, "Gate exit count" }
                    // Add more mappings for additional properties
                };

                // Set column headers dynamically using the mapping
                int columnIndex = 1;
                foreach (var kvp in columnHeaders)
                {
                    worksheet.Cells[1, columnIndex].Value = kvp.Value;
                    worksheet.Cells[1, columnIndex].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[1, columnIndex].Style.Font.Bold = true;
                    columnIndex++;
                }

                columnIndex = 1; 
                // Populate data
                for (int i = 0; i < sensorDetails.Count; i++)
                {
                    int rowIndex = i + 2;
                    foreach (var kvp in columnHeaders)
                    {
                        var property = kvp.Key.Compile();
                        var value = property.Invoke(sensorDetails[i]);
                        worksheet.Cells[rowIndex, columnIndex].Value = value;
                        worksheet.Cells[rowIndex, columnIndex].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        columnIndex++;
                    }
                    columnIndex = 1; // Reset columnIndex for the next row
                }

                // Save the Excel file
                excelPackage.SaveAs(excelFile);
            }
        }

        private void DeleteIfFileExists(FileInfo excelFile)
        {
            if (excelFile.Exists)
            {
                excelFile.Delete();
            }
        }

        [Route("getAllWithDetailsPdfReport")]
        [HttpPost]
        public async Task GetAllWithDetailsPdfReportAsync(GetAllSensorWithDetailsReportInputDto input)
        {
            var allSensorWithDetailsQueryable = _sensorRepository.GetAllWithDetails();
            allSensorWithDetailsQueryable = FilterQuery(allSensorWithDetailsQueryable, input.GateIds, input.FromDate, input.ToDate);
            var allSensorWithDetails = await allSensorWithDetailsQueryable
                                                .OrderBy(p => p.Gate.Name)
                                                .ToListAsync();

            var getAllSensorWithDetails = GetAllSensorWithDetails(allSensorWithDetails, input.FromDate, input.ToDate);

            CreatePdfReport(getAllSensorWithDetails);
        }

        private void CreatePdfReport(GetAllSensorWithDetailsOutputDto sensorDetails)
        {
            // https://scrybercore.readthedocs.io/en/latest/1_overview/1_gui_controller_full.html
            // https://scrybercore.readthedocs.io/en/latest/1_overview/7_parameters_and_expressions.html

            string workingDirectory = System.Environment.CurrentDirectory;
            var path = System.IO.Path.Combine(workingDirectory, "Resources\\SensorWithDetails.html");
            var output = System.IO.Path.GetFullPath(System.IO.Path.Combine("Report", "Pdf", "SensorWithDetails.pdf"));

            using (var doc = Document.ParseDocument(path))
            {
                doc.Params["model"] = new
                {
                    sensorWithDetails = sensorDetails
                };

                //And save it to a file or a stream
                using (var stream = new System.IO.FileStream(output, System.IO.FileMode.Create))
                {
                    doc.SaveAsPDF(stream);
                }
            }
        }

        [Route("getAllWithDetails")]
        [HttpPost]
        public async Task<GetAllSensorWithDetailsOutputDto> GetAllWithDetailsAsync(GetAllSensorWithDetailsInputDto input)
        {
            var cacheKey = $"getAllSensorWithDetails-{input.SkipCount}-{input.MaxResultCount}-{input.Sorting}-{input.FromDate}-{input.ToDate}-";
            foreach(var gateId in input.GateIds)
            {
                cacheKey = cacheKey + $"{gateId}";
            }
            var cacheData = _cacheService.GetData<GetAllSensorWithDetailsOutputDto>(cacheKey);

            if (cacheData != null)
            {
                return cacheData;
            }

            if (input.FromDate != null && input.ToDate != null)
            {
                if(input.FromDate > input.ToDate)
                {
                    throw new Exception("From date must be less than To date");
                }
            }

            var allSensorWithDetailsQueryable = _sensorRepository.GetAllWithDetails();

            allSensorWithDetailsQueryable = FilterQuery(allSensorWithDetailsQueryable, input.GateIds, input.FromDate, input.ToDate);

            var totalCount = await allSensorWithDetailsQueryable.CountAsync();

            var allSensorWithDetails = await allSensorWithDetailsQueryable
                                                .OrderBy(p => p.Gate.Name)
                                                .Skip(input.SkipCount)
                                                .Take(input.MaxResultCount)
                                                .ToListAsync();

            var getAllSensorWithDetails = GetAllSensorWithDetails(allSensorWithDetails, input.FromDate, input.ToDate);
            getAllSensorWithDetails.TotalCount = totalCount;

            cacheData = getAllSensorWithDetails;
            _cacheService.SetData(cacheKey, cacheData, DateTime.Now.AddSeconds(30));
            return cacheData;
        }

        private GetAllSensorWithDetailsOutputDto GetAllSensorWithDetails(List<Sensor> allSensorWithDetails, 
            DateTime? fromDate,
            DateTime? toDate)
        {
            var getAllSensorWithDetails = new GetAllSensorWithDetailsOutputDto();
            var allSensorDetails = new List<SensorDetailsDto>();

            foreach (var sensorWithDetails in allSensorWithDetails)
            {
                var sensorDetails = new SensorDetailsDto();
                sensorDetails.Id = sensorWithDetails.Id;
                sensorDetails.Name = sensorWithDetails.Name;

                var gateDetails = new GateDetailsDto();
                gateDetails.Id = sensorWithDetails.Gate.Id;
                gateDetails.Name = sensorWithDetails.Gate.Name;

                var gateEntries = sensorWithDetails.Gate.GateEntries;
                var gateExits = sensorWithDetails.Gate.GateExits;

                if (fromDate != null && toDate != null)
                {
                    gateEntries = sensorWithDetails.Gate.GateEntries.Where(p => p.TimeStamp >= fromDate && p.TimeStamp <= toDate).ToList();
                    gateExits = sensorWithDetails.Gate.GateExits.Where(p => p.TimeStamp >= fromDate && p.TimeStamp <= toDate).ToList();
                }
                else if (fromDate != null && toDate == null)
                {
                    gateEntries = sensorWithDetails.Gate.GateEntries.Where(p => p.TimeStamp >= fromDate).ToList();
                    gateExits = sensorWithDetails.Gate.GateExits.Where(p => p.TimeStamp >= fromDate).ToList();
                }
                else if (fromDate == null && toDate != null)
                {
                    gateEntries = sensorWithDetails.Gate.GateEntries.Where(p => p.TimeStamp <= toDate).ToList();
                    gateExits = sensorWithDetails.Gate.GateExits.Where(p => p.TimeStamp <= toDate).ToList();
                }

                gateDetails.EntryCount = gateEntries.Sum(p => p.NumberOfPeople);
                gateDetails.ExitCount = gateExits.Sum(p => p.NumberOfPeople);

                sensorDetails.GateDetails = gateDetails;

                allSensorDetails.Add(sensorDetails);
            }

            getAllSensorWithDetails.Items = allSensorDetails;

            return getAllSensorWithDetails;
        }

        private IQueryable<Sensor> FilterQuery(IQueryable<Sensor> allSensorWithDetailsQueryable, Guid[] gateIds, 
            DateTime? fromDate, 
            DateTime? toDate)
        {
            if (gateIds.Count() > 0)
                allSensorWithDetailsQueryable = allSensorWithDetailsQueryable.Where(s => gateIds.Contains(s.Gate.Id));

            if (fromDate != null && toDate != null)
            {
                allSensorWithDetailsQueryable = allSensorWithDetailsQueryable
                    .Where(s => s.Gate.GateEntries.Any(p => p.TimeStamp >= fromDate && p.TimeStamp <= toDate) ||
                        s.Gate.GateExits.Any(p => p.TimeStamp >= fromDate && p.TimeStamp <= toDate));
            }
            else if (fromDate != null && toDate == null)
            {
                allSensorWithDetailsQueryable = allSensorWithDetailsQueryable
                    .Where(s => s.Gate.GateEntries.Any(p => p.TimeStamp >= fromDate) ||
                        s.Gate.GateExits.Any(p => p.TimeStamp >= fromDate));
            }
            else if (fromDate == null && toDate != null)
            {
                allSensorWithDetailsQueryable = allSensorWithDetailsQueryable
                    .Where(s => s.Gate.GateEntries.Any(p => p.TimeStamp <= toDate) ||
                        s.Gate.GateExits.Any(p => p.TimeStamp <= toDate));
            }

            return allSensorWithDetailsQueryable;
        }
    }
}
