﻿using GateEntryExit.Domain;
using GateEntryExit.Domain.Manager;
using GateEntryExit.Dtos.Gate;
using GateEntryExit.Dtos.GateEntry;
using GateEntryExit.Dtos.Shared;
using GateEntryExit.Helper;
using GateEntryExit.Repositories;
using GateEntryExit.Repositories.Interfaces;
using GateEntryExit.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace GateEntryExit.Controllers
{
    [Route("api/gate")]
    [ApiController]
    public class GateController : ControllerBase
    {
        private readonly IGateRepository _gateRepository;

        private readonly IGateManager _gateManager;

        private readonly IGuidGenerator _guidGenerator;

        private readonly ICacheService _cacheService;

        public GateController(IGateRepository gateRepository,
            IGateManager gateManager,
            IGuidGenerator guidGenerator,
            ICacheService cacheService)
        {
            _gateRepository = gateRepository;
            _gateManager = gateManager;
            _guidGenerator = guidGenerator;
            _cacheService = cacheService;
        }

        [Route("getAll")]
        [HttpPost]
        public async Task<GetAllGatesDto> GetAllAsync(GetAllDto input)
        {
            var cacheKey = $"getAllGate-{input.SkipCount}-{input.MaxResultCount}-{input.Sorting}";
            var cacheData = _cacheService.GetData<GetAllGatesDto>(cacheKey);

            if (cacheData != null)
            {
                return cacheData;
            }

            cacheData = await GetAllGatesAsync(input);
            _cacheService.SetData<GetAllGatesDto>(cacheKey, cacheData, DateTime.Now.AddSeconds(30));

            return cacheData;
        }

        [Route("getAllById")]
        [HttpPost]
        public async Task<GetAllGatesDto> GetAllByIdAsync(Guid[] gateIds)
        {
            var result = new List<GateDetailsDto>();

            var cacheKey = $"getAllGateByIds-";
            foreach (var gateId in gateIds)
            {
                cacheKey = cacheKey + $"{gateId}";
            }
            var cacheData = _cacheService.GetData<GetAllGatesDto>(cacheKey);

            if (cacheData != null)
            {
                return cacheData;
            }

            var gateQueryabe = _gateRepository.GetAll(gateIds);
            if (gateQueryabe.Count() > 0)
            {
                result = gateQueryabe.Select(g => new GateDetailsDto
                {
                    Name = g.Name,
                    Id = g.Id
                }).OrderBy(p => p.Name).ToList();
            }        

            cacheData = new GetAllGatesDto { Items = result };
            _cacheService.SetData<GetAllGatesDto>(cacheKey, cacheData, DateTime.Now.AddSeconds(30));

            return cacheData;
        }

        private async Task<GetAllGatesDto> GetAllGatesAsync(GetAllDto input)
        {
            var result = new List<GateDetailsDto>();

            var gateQueryabe = _gateRepository.GetAll();

            var totalCount = await gateQueryabe.CountAsync();

            if (input != null)
            {
                gateQueryabe = gateQueryabe.OrderBy(p => p.Name).Skip(input.SkipCount).Take(input.MaxResultCount);
            }

            if (gateQueryabe.Count() > 0)
            {
                result = gateQueryabe.Select(g => new GateDetailsDto
                {
                    Name = g.Name,
                    Id = g.Id
                }).OrderBy(p => p.Name).ToList();
            }

            return new GetAllGatesDto { Items = result, TotalCount = totalCount };
        }

        [Route("create")]
        [HttpPost]
        public async Task<GateDto> CreateAsync(CreateGateDto input)
        {
            var gate = await _gateManager.CreateAsync(_guidGenerator.Create(), input.Name);
            await _gateRepository.InsertAsync(gate);

            _cacheService.RemoveDatas("getAllGate-*");
            //_cacheService.RemoveDatas("getAllGatePost-*");

            return new GateDto()
            {
                Name = gate.Name,
                Id = gate.Id
            };
        }

        [Route("edit")]
        [HttpPost]
        public async Task<GateDto> EditAsync(UpdateGateDto input)
        {
            await _gateRepository.UpdateAsync(input.Id , input.Name);
            var gate = await _gateRepository.GetAsync(input.Id);

            _cacheService.RemoveDatas("getAllGate-*");
            //_cacheService.RemoveDatas("getAllGatePost-*");

            return new GateDto()
            {
                Name = gate.Name,
                Id = gate.Id
            };
        }

        [HttpDelete("delete/{id}")]
        public async Task DeleteAsync(Guid id)
        {
            await _gateRepository.DeleteAsync(id);
            _cacheService.RemoveDatas("getAllGate-*");
            _cacheService.RemoveDatas("getAllGatePost-*");
        }
    }
}