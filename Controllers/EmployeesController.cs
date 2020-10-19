﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts;
using Entities.DTOs;
using Entities.Models;
using Microsoft.AspNetCore.Http;
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

        public IActionResult GetEmployeesFromCompany(Guid companyId)
        {
            var company = _repositoryManager.Company.GetCompany(companyId, trackChanges: false);

            if(company == null)
            {
                _loggerManager.LogInfo($"Company with id: {companyId} does not exist in our database");
                return NotFound();
            }

            var employeesFromDb = _repositoryManager.Employee.GetEmployees(companyId, trackChanges: false);

            var employeeDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesFromDb);
            return Ok(employeeDto);
        }

        [HttpGet("{employeeId}", Name ="GetEmployeeFromCompany")]
        public IActionResult GetEmployeeFromCompany(Guid companyId, Guid employeeId)
        {
            var company = _repositoryManager.Company.GetCompany(companyId, trackChanges: false);

            if (company == null)
            {
                _loggerManager.LogInfo($"Company with id: {companyId} does not exist in our database");
                return NotFound();
            }

            var employeeFromDb = _repositoryManager.Employee.GetEmployee(companyId, employeeId, trackChanges: false);
            
            if(employeeFromDb == null)
            {
                _loggerManager.LogInfo($"Employee with id: {employeeId} does not exist in our database");
                return NotFound();
            }

            var employeeDto = _mapper.Map<EmployeeDto>(employeeFromDb);
            return Ok(employeeDto);
        }

        [HttpPost]
        public IActionResult CreateEmployeeForCompany(Guid companyId, [FromBody] CreateEmployeeDto employee)
        {
            if(employee == null)
            {
                _loggerManager.LogError("CreateEmployeeDto object sent from client is null");
                return BadRequest("CreateEmployeeDto is null");
            }

            var company = _repositoryManager.Company.GetCompany(companyId, trackChanges: false);
            if(company == null)
            {
                _loggerManager.LogError($"Company with id: {companyId} does not exist in our database");
                return NotFound();
            }

            var employeeEntity = _mapper.Map<Employee>(employee);
            _repositoryManager.Employee.CreateEmployee(companyId, employeeEntity);
            _repositoryManager.Save();

            var employeeToReturn = _mapper.Map<EmployeeDto>(employeeEntity);

            return CreatedAtRoute("GetEmployeeFromCompany", new { companyId, employeeId = employeeToReturn.EmployeeId }, employeeToReturn);
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteEmployeeForCompany(Guid companyId, Guid id)
        {
            var company = _repositoryManager.Company.GetCompany(companyId, trackChanges: false);
            if(company == null)
            {
                _loggerManager.LogInfo($"Company with id: {companyId} does not exist in our database");
                return NotFound();
            }

            var employee = _repositoryManager.Employee.GetEmployee(companyId, id, trackChanges: false);
            if (employee == null)
            {
                _loggerManager.LogInfo($"Employee with id: {companyId} does not exist in our database");
                return NotFound();
            }

            _repositoryManager.Employee.DeleteEmployee(employee);
            _repositoryManager.Save();

            return NoContent();

        }
    }
}
