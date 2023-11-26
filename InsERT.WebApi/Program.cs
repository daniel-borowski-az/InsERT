using InsERT.WebApi.Database;
using InsERT.WebApi.Filters;
using InsERT.WebApi.Jobs;
using InsERT.WebApi.Repositories;
using InsERT.WebApi.Services;
using Quartz;
using Quartz.AspNetCore;

namespace InsERT.WebApi
{
    public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);
			builder.Host.ConfigureLogging(logging =>
			{
				logging.ClearProviders();
				logging.AddConsole();
			});

			// Add services to the container.
			builder.Services.AddSingleton<DbContext>();
			builder.Services.AddSingleton<ICurrencyRateService, CurrencyRateService>();
			builder.Services.AddSingleton<ICurrencyRateRepository, CurrencyRateRepository>();
			builder.Services.AddSingleton<IWalletRepository, WalletRepository>();
			builder.Services.AddSingleton<CurrencyRateUpdaterJob>();
			builder.Services.AddSingleton<WalletExceptionFilter>();
			builder.Services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();
			;
			builder.Services.AddQuartz(q =>
			{
				q.UseMicrosoftDependencyInjectionJobFactory();
				var jobKey = new JobKey("CurrencyRateUpdaterJob");
				q.AddJob<CurrencyRateUpdaterJob>(opts => opts.WithIdentity(jobKey));


				q.AddTrigger(opts => opts
					.ForJob(jobKey)
					.WithIdentity($"{jobKey}-trigger")
					.WithCronSchedule(builder.Configuration.GetValue<string>("Settings:CronSchedule")));

			});
			builder.Services.AddQuartzServer(options =>
			{
				options.WaitForJobsToComplete = true;
			});
			var app = builder.Build();
			using (var serviceScope = app.Services.GetService<IServiceScopeFactory>().CreateScope())
			{
				var context = serviceScope.ServiceProvider.GetRequiredService<DbContext>();
				context.EnsureCreated();
			}

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();

			app.UseAuthorization();


			app.MapControllers();

			app.Run();
		}


	}
}