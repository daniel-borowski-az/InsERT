using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace InsERT.WebApi.Filters
{
	public class ConvertValidationFilter : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			const string sourceCurrencyCodeKey = "sourceCurrencyCode";
			const string destinationCurrencyCodeKey = "destinationCurrencyCode";
			const string amountKey = "amount";

			var error = string.Empty;
			if (!context.ActionArguments.ContainsKey(sourceCurrencyCodeKey) || string.IsNullOrWhiteSpace(context.ActionArguments[sourceCurrencyCodeKey] as string))
			{
				error = $"{sourceCurrencyCodeKey} cannot empty";
			}

			if (!context.ActionArguments.ContainsKey(destinationCurrencyCodeKey) || string.IsNullOrWhiteSpace(context.ActionArguments[destinationCurrencyCodeKey] as string))
			{
				error = $"{destinationCurrencyCodeKey} cannot empty";
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
