using GateEntryExit.Domain.Manager;
using GateEntryExit.Dtos.GateEntry;
using GateEntryExit.Domain;
using GateEntryExit.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GateEntryExit.Helper;
using GateEntryExit.Dtos.Gate;
using GateEntryExit.Repositories;
using Microsoft.EntityFrameworkCore;
using GateEntryExit.Dtos.Shared;
using Scryber.OpenType.SubTables;
using GateEntryExit.Service.Cache;
using Microsoft.AspNetCore.Authorization;

namespace GateEntryExit.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/gateEntry")]
    public class GateEntryController : ControllerBase
    {
        private readonly IGateEntryRepository _gateEntryRepository;

        private readonly IGateEntryManager _gateEntryManager;

        private readonly IGuidGenerator _guidGenerator;

        private readonly ICacheService _cacheService;

        public GateEntryController(IGateEntryRepository gateEntryRepository,
            IGateEntryManager gateEntryManager,
             IGuidGenerator guidGenerator,
             ICacheService cacheService)
        {
            _gateEntryRepository = gateEntryRepository;
            _gateEntryManager = gateEntryManager;
            _guidGenerator = guidGenerator;
            _cacheService = cacheService;
        }

        [Route("create")]
        [HttpPost]
        public async Task<GateEntryDto> CreateAsync(CreateGateEntryDto input)
        {
            var gateEntry = _gateEntryManager.Create(_guidGenerator.Create(), input.GateId, input.NumberOfPeople, input.TimeStamp);
            await _gateEntryRepository.InsertAsync(gateEntry);

            _cacheService.RemoveDatas("getAllGateEntries-*");

            return new GateEntryDto()
            {
                Id = gateEntry.Id,
                NumberOfPeople = gateEntry.NumberOfPeople,
                TimeStamp = gateEntry.TimeStamp,
                GateId = gateEntry.GateId
            };
        }

        [Route("getAll")]
        [HttpPost]
        public async Task<GetAllGateEntriesDto> GetAllAsync(GetAllDto input)
        {
            var cacheKey = $"getAllGateEntries-{input.SkipCount}-{input.MaxResultCount}-{input.Sorting}";
            var cacheData = _cacheService.GetData<GetAllGateEntriesDto>(cacheKey);

            if (cacheData != null)
            {
                return cacheData;
            }

            var result = new List<GateEntryDto>();

            var gateEntriesQueryabe = _gateEntryRepository.GetAll();

            var totalCount = await gateEntriesQueryabe.CountAsync();

            if (input != null)
            {
                gateEntriesQueryabe = gateEntriesQueryabe.OrderBy(p => p.Gate.Name).Skip(input.SkipCount).Take(input.MaxResultCount);
            }

            if (gateEntriesQueryabe.Count() > 0)
            {
                result = gateEntriesQueryabe.Select(g => new GateEntryDto
                {
                    GateName = g.Gate.Name,
                    GateId = g.Gate.Id,
                    NumberOfPeople = g.NumberOfPeople,
                    TimeStamp = g.TimeStamp,
                    Id = g.Id
                }).OrderBy(p => p.GateName).ToList();
            }

            cacheData = new GetAllGateEntriesDto { Items = result, TotalCount = totalCount };
            _cacheService.SetData(cacheKey, cacheData, DateTime.Now.AddSeconds(30));
            return cacheData;
        }

        [Route("getById/{id}")]
        [HttpPost]
        public async Task<GateEntryDto> GetByIdAsync(Guid id)
        {
            var cacheKey = $"getGateEntryById-";
            cacheKey = cacheKey + $"{id}";

            var cacheData = _cacheService.GetData<GateEntryDto>(cacheKey);

            if (cacheData != null)
            {
                return cacheData;
            }

            var gateEntry = await _gateEntryRepository.GetAsync(id);

            cacheData = new GateEntryDto()
            {
                Id = gateEntry.Id,
                NumberOfPeople = gateEntry.NumberOfPeople,
                TimeStamp = gateEntry.TimeStamp,
                GateId = gateEntry.GateId,
                GateName = gateEntry.Gate.Name
            };
            _cacheService.SetData<GateEntryDto>(cacheKey, cacheData, DateTime.Now.AddSeconds(30));

            return cacheData;
        }

        [Route("edit")]
        [HttpPost]
        public async Task<GateEntryDto> EditAsync(UpdateGateEntryDto input)
        {
            await _gateEntryRepository.UpdateAsync(input.Id, input.TimeStamp, input.NumberOfPeople);
            var gateEntry = await _gateEntryRepository.GetAsync(input.Id);

            _cacheService.RemoveDatas("getAllGateEntries-*");
            _cacheService.RemoveDatas($"getGateEntryById-{input.Id}");

            return new GateEntryDto()
            {
                Id = gateEntry.Id,
                NumberOfPeople = gateEntry.NumberOfPeople,
                TimeStamp = gateEntry.TimeStamp,
                GateId = gateEntry.GateId
            };
        }

        [HttpDelete("delete/{id}")]
        public async Task DeleteAsync(Guid id)
        {
            await _gateEntryRepository.DeleteAsync(id);
            _cacheService.RemoveDatas("getAllGateEntries-*");
        }
    }
}
