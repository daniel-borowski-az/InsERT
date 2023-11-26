using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace InsERT.WebApi.Filters
{
	public class DepositWithrawValidationFilter : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			const string codeKey = "code";
			const string amountKey = "amount";
			var error = string.Empty;

			if (!context.ActionArguments.ContainsKey(codeKey) || string.IsNullOrWhiteSpace(context.ActionArguments[codeKey] as string))
			{
				error = $"{codeKey} cannot empty";
			}

			if (!context.ActionArguments.ContainsKey(amountKey) || (context.ActionArguments[amountKey] as decimal?).GetValueOrDefault() <= 0)
			{
				error = $"{amountKey} must be greater than 0";
			}

			if (!string.IsNullOrEmpty(error))
			{
				var contentResult = new ContentResult
				{
					Content = JsonSerializer.Serialize(error),
					ContentType = "application/json",
					StatusCode = StatusCodes.Status400BadRequest
				};
				context.Result = contentResult;
			}

			base.OnActionExecuting(context);
		}
	}
}
