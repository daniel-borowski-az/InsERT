using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace InsERT.WebApi.Filters
{
	public class CreateWalletValidationFilter : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			const string walletNameKey = "name";

			if (!context.ActionArguments.ContainsKey(walletNameKey) || string.IsNullOrWhiteSpace(context.ActionArguments[walletNameKey] as string))
			{
				var error = $"{walletNameKey} cannot empty";
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
