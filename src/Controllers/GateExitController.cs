using GateEntryExit.Domain;
using GateEntryExit.Domain.Manager;
using GateEntryExit.Dtos.Gate;
using GateEntryExit.Dtos.GateEntry;
using GateEntryExit.Dtos.GateExit;
using GateEntryExit.Dtos.Shared;
using GateEntryExit.Helper;
using GateEntryExit.Repositories;
using GateEntryExit.Repositories.Interfaces;
using GateEntryExit.Service.Cache;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GateEntryExit.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/gateExit")]
    public class GateExitController : ControllerBase
    {
        private readonly IGateExitRepository _gateExitRepository;

        private readonly IGateExitManager _gateExitManager;

        private readonly IGuidGenerator _guidGenerator;

        private readonly ICacheService _cacheService;

        public GateExitController(IGateExitRepository gateExitRepository,
            IGateExitManager gateExitManager,
            IGuidGenerator guidGenerator,
            ICacheService cacheService)
        {
            _gateExitRepository = gateExitRepository;
            _gateExitManager = gateExitManager;
            _guidGenerator = guidGenerator;
            _cacheService = cacheService;
        }

        [Route("create")]
        [HttpPost]
        public async Task<GateExitDto> CreateAsync(CreateGateExitDto input)
        {
            var gateExit = _gateExitManager.Create(_guidGenerator.Create(), input.GateId, input.NumberOfPeople, input.TimeStamp);
            await _gateExitRepository.InsertAsync(gateExit);
            _cacheService.RemoveDatas("getAllGateExits-*");

            return new GateExitDto()
            {
                Id = gateExit.Id,
                NumberOfPeople = gateExit.NumberOfPeople,
                TimeStamp = gateExit.TimeStamp,
                GateId = gateExit.GateId
            };
        }

        [Route("getAll")]
        [HttpPost]
        public async Task<GetAllGateExitsDto> GetAllAsync(GetAllDto input)
        {
            var cacheKey = $"getAllGateExits-{input.SkipCount}-{input.MaxResultCount}-{input.Sorting}";
            var cacheData = _cacheService.GetData<GetAllGateExitsDto>(cacheKey);

            if (cacheData != null)
            {
                return cacheData;
            }

            var result = new List<GateExitDto>();

            var gateExitsQueryabe = _gateExitRepository.GetAll();

            var totalCount = await gateExitsQueryabe.CountAsync();

            if (input != null)
            {
                gateExitsQueryabe = gateExitsQueryabe.OrderBy(p => p.Gate.Name).Skip(input.SkipCount).Take(input.MaxResultCount);
            }

            if (gateExitsQueryabe.Count() > 0)
            {
                result = gateExitsQueryabe.Select(g => new GateExitDto
                {
                    GateName = g.Gate.Name,
                    GateId = g.Gate.Id,
                    NumberOfPeople = g.NumberOfPeople,
                    TimeStamp = g.TimeStamp,
                    Id = g.Id
                }).OrderBy(p => p.GateName).ToList();
            }

            cacheData = new GetAllGateExitsDto { Items = result, TotalCount = totalCount };
            _cacheService.SetData(cacheKey, cacheData, DateTime.Now.AddSeconds(30));
            return cacheData;
        }

        [Route("getById/{id}")]
        [HttpPost]
        public async Task<GateExitDto> GetByIdAsync(Guid id)
        {
            var cacheKey = $"getGateExitById-";
            cacheKey = cacheKey + $"{id}";

            var cacheData = _cacheService.GetData<GateExitDto>(cacheKey);

            if (cacheData != null)
            {
                return cacheData;
            }

            var gateExit = await _gateExitRepository.GetAsync(id);

            cacheData = new GateExitDto()
            {
                Id = gateExit.Id,
                NumberOfPeople = gateExit.NumberOfPeople,
                TimeStamp = gateExit.TimeStamp,
                GateId = gateExit.GateId,
                GateName = gateExit.Gate.Name
            };
            _cacheService.SetData<GateExitDto>(cacheKey, cacheData, DateTime.Now.AddSeconds(30));

            return cacheData;
        }

        [Route("edit")]
        [HttpPost]
        public async Task<GateExitDto> EditAsync(UpdateGateExitDto input)
        {
            await _gateExitRepository.UpdateAsync(input.Id, input.TimeStamp, input.NumberOfPeople);
            var gateExit = await _gateExitRepository.GetAsync(input.Id);

            _cacheService.RemoveDatas("getAllGateExits-*");
            _cacheService.RemoveDatas($"getGateExitById-{input.Id}");

            return new GateExitDto()
            {
                Id = gateExit.Id,
                NumberOfPeople = gateExit.NumberOfPeople,
                TimeStamp = gateExit.TimeStamp,
                GateId = gateExit.GateId
            };
        }

        [HttpDelete("delete/{id}")]
        public async Task DeleteAsync(Guid id)
        {
            await _gateExitRepository.DeleteAsync(id);
            _cacheService.RemoveDatas("getAllGateExits-*");
        }
    }
}
