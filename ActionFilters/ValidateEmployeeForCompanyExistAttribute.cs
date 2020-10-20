using Contracts;
using LoggerService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyEmployees.ActionFilters
{
    public class ValidateEmployeeForCompanyExistAttribute : IAsyncActionFilter
    {
        private readonly ILoggerManager _loggerManager;
        private readonly IRepositoryManager _repositoryManager;

        public ValidateEmployeeForCompanyExistAttribute(ILoggerManager loggerManager, IRepositoryManager repositoryManager)
        {
            _loggerManager = loggerManager;
            _repositoryManager = repositoryManager;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var method = context.HttpContext.Request.Method;
            var trackchanges = (method.Equals("PUT") || method.Equals("PATCH")) ? true : false;

            var companyId = (Guid)context.ActionArguments["companyId"];
            var id = (Guid)context.ActionArguments["id"];

            var company = await _repositoryManager.Company.GetCompanyAsync(companyId, trackchanges);
            if(company == null)
            {
                _loggerManager.LogInfo($"Company with the id: {companyId} does not exist in our database");
                context.Result = new NotFoundResult();
            }

            var employee = await _repositoryManager.Employee.GetEmployeeAsync(companyId, id, trackchanges);
            if(employee == null)
            {
                _loggerManager.LogInfo($"Employee with the id: {id} does not exist in our database");
                context.Result = new NotFoundResult();
            } else
            {
                context.HttpContext.Items.Add("employee", employee);
                await next();
            }

            
        }
    }
}
