using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CompanyEmployees.ModelBinders;
using Contracts;
using Entities.DTOs;
using Entities.Models;
using LoggerService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository;

namespace CompanyEmployees.Controllers
{
    [Route("api/Companies")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly ILoggerManager _loggerManager;
        private readonly IMapper _mapper;

        public CompaniesController(IRepositoryManager repositoryManager, ILoggerManager loggerManager, IMapper mapper)
        {
            _repositoryManager = repositoryManager;
            _loggerManager = loggerManager;
            _mapper = mapper;
        }

       [HttpGet]
       public IActionResult GetCompanies()
        {
            var companies =  _repositoryManager.Company.GetAllCompanies(trackChanges: false);

            var companyDto = _mapper.Map<IEnumerable<CompanyDto>>(companies);

            return Ok(companyDto);
        }

        [HttpGet("{id}", Name ="CompanyById")]
        public IActionResult GetCompany(Guid id)
        {
            var company = _repositoryManager.Company.GetCompany(id, trackChanges: false);

            if(company == null)
            {
                _loggerManager.LogError($"Company with id: {id} does not exist in the database");
                return NotFound();
            }
            else
            {
                var companyDto = _mapper.Map<CompanyDto>(company);
                return Ok(companyDto);
            }
        }


        [HttpPost]
        public IActionResult CreateCompany([FromBody] CreateCompanyDto company)
        {
            if(company == null)
            {
                _loggerManager.LogError("CreateCompanyDto object sent from client is null");
                return BadRequest("CreateCompanyDto is null");
            }

            var companyEntity = _mapper.Map<Company>(company);
            _repositoryManager.Company.CreateCompany(companyEntity);
            _repositoryManager.Save();

            var companyToReturn = _mapper.Map<CompanyDto>(companyEntity);

            return CreatedAtRoute("CompanyById", new {id = companyToReturn.CompanyId }, companyToReturn);
        }

        [HttpGet("collection/{ids}", Name = "GetCompanyCollection")]
        public IActionResult GetCompanyCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                _loggerManager.LogError("Parameter ids are null");
                return BadRequest("Parameter ids are null");
            }

            var companyEntities = _repositoryManager.Company.GetCompaniesByIds(ids, trackChanges: false);

            if (ids.Count() != companyEntities.Count())
            {
                _loggerManager.LogError("Some ids in the collection are not valid");
                return NotFound();
            }

            var companiesToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);
            return Ok(companiesToReturn);
        }

        [HttpPost("collection")]
        public IActionResult CreateCompanyCollection([FromBody] IEnumerable<CreateCompanyDto> companyCollection)
        {
            if(companyCollection == null)
            {
                _loggerManager.LogError("Company Collection sent from client is null");
                return BadRequest("Company Collection is null");
            }

            var companyEntities = _mapper.Map<IEnumerable<Company>>(companyCollection);

            foreach(Company company in companyEntities)
            {
                _repositoryManager.Company.CreateCompany(company);
            }
            _repositoryManager.Save();

            var companyCollectionToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);

            var ids = string.Join(",", companyCollectionToReturn.Select(c => c.CompanyId));

            return CreatedAtRoute("GetCompanyCollection", new { ids }, companyCollectionToReturn);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteCompany(Guid id)
        {
            var company = _repositoryManager.Company.GetCompany(id, trackChanges: false);
            if(company == null)
            {
                _loggerManager.LogInfo($"Company with the id: {id} sent from client does not exist in our database");
                return NotFound();
            }

            _repositoryManager.Company.DeleteCompany(company);
            _repositoryManager.Save();

            return NoContent();
        }

    }
}
