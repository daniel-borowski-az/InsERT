using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace InsERT.WebApi.Filters
{
	public class WalletExceptionFilter : IExceptionFilter
	{
		public void OnException(ExceptionContext context)
		{
			var response = new { error = context.Exception.Message };
			var payload = JsonSerializer.Serialize(response);
			var contentResult = new ContentResult
			{
				Content = payload,
				ContentType = "application/json",
			};

			if (context.Exception is KeyNotFoundException)
			{
				contentResult.StatusCode = StatusCodes.Status404NotFound;
				context.Result = contentResult;
			}
			else if(context.Exception is InvalidOperationException)
			{
				contentResult.StatusCode = StatusCodes.Status400BadRequest;
				context.Result = contentResult;
			}
			else
			{
				contentResult.StatusCode = StatusCodes.Status500InternalServerError;
			}
			context.ExceptionHandled = true;
		}
	}
}
