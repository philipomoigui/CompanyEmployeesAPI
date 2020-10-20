using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CompanyEmployees.ActionFilters;
using Contracts;
using Entities.DTOs;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Repository;

namespace CompanyEmployees.Controllers
{
    [Route("api/companies/{companyId}/employees")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly ILoggerManager _loggerManager;
        private readonly IMapper _mapper;

        public EmployeesController(IRepositoryManager repositoryManager, ILoggerManager loggerManager, IMapper mapper)
        {
            _repositoryManager = repositoryManager;
            _loggerManager = loggerManager;
            _mapper = mapper;
        }

        public async Task<IActionResult> GetEmployeesForCompany(Guid companyId)
        {
            var company = await _repositoryManager.Company.GetCompanyAsync(companyId, trackChanges: false);

            if(company == null)
            {
                _loggerManager.LogInfo($"Company with id: {companyId} does not exist in our database");
                return NotFound();
            }

            var employeesFromDb = await _repositoryManager.Employee.GetEmployeesAsync(companyId, trackChanges: false);

            var employeeDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesFromDb);
            return Ok(employeeDto);
        }

        [HttpGet("{employeeId}", Name ="GetEmployeeFromCompany")]
        public async Task<IActionResult> GetEmployeeForCompany(Guid companyId, Guid employeeId)
        {
            var company = await _repositoryManager.Company.GetCompanyAsync(companyId, trackChanges: false);

            if (company == null)
            {
                _loggerManager.LogInfo($"Company with id: {companyId} does not exist in our database");
                return NotFound();
            }

            var employeeFromDb = await _repositoryManager.Employee.GetEmployeeAsync(companyId, employeeId, trackChanges: false);
            
            if(employeeFromDb == null)
            {
                _loggerManager.LogInfo($"Employee with id: {employeeId} does not exist in our database");
                return NotFound();
            }

            var employeeDto = _mapper.Map<EmployeeDto>(employeeFromDb);
            return Ok(employeeDto);
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateEmployeeForCompany(Guid companyId, [FromBody] CreateEmployeeDto employee)
        {
          
            var company = await _repositoryManager.Company.GetCompanyAsync(companyId, trackChanges: false);
            if(company == null)
            {
                _loggerManager.LogError($"Company with id: {companyId} does not exist in our database");
                return NotFound();
            }

            var employeeEntity = _mapper.Map<Employee>(employee);
            _repositoryManager.Employee.CreateEmployee(companyId, employeeEntity);
            await _repositoryManager.SaveAsync();

            var employeeToReturn = _mapper.Map<EmployeeDto>(employeeEntity);

            return CreatedAtRoute("GetEmployeeFromCompany", new { companyId, employeeId = employeeToReturn.EmployeeId }, employeeToReturn);
        }


        [HttpDelete("{id}")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistAttribute))]
        public async Task<IActionResult> DeleteEmployeeForCompany(Guid companyId, Guid id)
        {
            var employee = HttpContext.Items["employee"] as Employee;

            _repositoryManager.Employee.DeleteEmployee(employee);
            await _repositoryManager.SaveAsync();

            return NoContent();

        }

        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistAttribute))]
        public async Task<IActionResult> UpdateEmployeeForCompany(Guid companyId, Guid id, [FromBody] UpdateEmployeeDto employee)
        {
            var employeeEntity = HttpContext.Items["employee"] as Employee;
            _mapper.Map(employee, employeeEntity);
            await _repositoryManager.SaveAsync();

            return NoContent();
        }

        [HttpPatch("{id}")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistAttribute))]
        public async Task<ActionResult> PartialEmployeeforCompanyUpdate(Guid companyId, Guid id, [FromBody] JsonPatchDocument<UpdateEmployeeDto> patchDoc)
        {
            if (patchDoc == null)
            {
                _loggerManager.LogError("The PatchDoc Object received from client is null");
                return BadRequest("The patchDoc Object is null");
            }

            var employeeEntity = HttpContext.Items["employee"] as Employee;

            var employeeToPatch = _mapper.Map<UpdateEmployeeDto>(employeeEntity);
            patchDoc.ApplyTo(employeeToPatch, ModelState);

            TryValidateModel(employeeToPatch);

            if (!ModelState.IsValid)
            {
                _loggerManager.LogError("Invalid Model State for UpdateEmployeeDto Object");
                return UnprocessableEntity(ModelState);
            }

            _mapper.Map(employeeToPatch, employeeEntity);
            await _repositoryManager.SaveAsync();

            return NoContent();
        }
    }
}
