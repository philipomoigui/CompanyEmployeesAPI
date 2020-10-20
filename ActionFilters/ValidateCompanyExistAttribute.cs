using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyEmployees.ActionFilters
{
    public class ValidateCompanyExistAttribute : IAsyncActionFilter
    {
        private readonly ILoggerManager _loggerManager;
        private readonly IRepositoryManager _repositoryManager;

        public ValidateCompanyExistAttribute(ILoggerManager loggerManager, IRepositoryManager repositoryManager)
        {
            _loggerManager = loggerManager;
            _repositoryManager = repositoryManager;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var trackChanges = context.HttpContext.Request.Method.Equals("PUT");
            var id = (Guid)context.ActionArguments["id"];

            var company = await _repositoryManager.Company.GetCompanyAsync(id, trackChanges);
            if(company == null)
            {
                _loggerManager.LogInfo($"Company with id: {id} does not exist in our database");
                context.Result = new NotFoundResult();
            } else
            {
                context.HttpContext.Items.Add("company", company);
                await next();
            }
        }
    }
}
