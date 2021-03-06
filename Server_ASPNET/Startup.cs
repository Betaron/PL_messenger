using System.IO;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VectorChat.Utilities;

namespace VectorChat.ServerASPNET
{
	public class Startup
	{
		private ILogger consoleLogger = LoggerFactory.Create(builder =>
		{
			builder.AddConsole();
			builder.AddDebug();
		}).CreateLogger<Startup>();

		private ILogger fileLogger = new FileLogger(Path.Combine(Directory.GetCurrentDirectory(), "logs", "StartupLog.log"));

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddCors();
			services.AddControllers();
		}

		/// <summary>
		/// Set:
		/// <list type="bullet">
		/// <item>Usage of development environment</item>
		/// <item>Method called on application stop</item>
		/// <item>Controllers mapping</item>
		/// <item>Handler for default (or incorrect) route request</item>
		/// </list>
		/// </summary>
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			lifetime.ApplicationStopping.Register(() =>
			{
				consoleLogger.Log(LogLevel.Warning, "Saving files...");

				consoleLogger.Log(LogLevel.Warning, "Saving groups.json ...");
				if (Server.GroupsList.Count > 0)
				{
					FileWorker.SaveToFile(Path.Combine(Directory.GetCurrentDirectory(), "groups.json"), Server.GroupsList);
				}

				consoleLogger.Log(LogLevel.Warning, "Saving messages...");
				if (Server.AllMessagesCount > 0)
					foreach (var pair in Server.groupsStorage)
					{
						string messagesFilePath = Path.Combine(Directory.GetCurrentDirectory(), "MessagesStorage", $"groupID{pair.Key}", "messages.json");

						// create file with empty list if not exists
						if (!File.Exists(Path.Combine(messagesFilePath)))
						{
							FileWorker.SaveToFile(Path.Combine(messagesFilePath), new List<Message>());
						}

						List<Message> loadedMessages = FileWorker.LoadFromFile<List<Message>>(messagesFilePath);
						loadedMessages.AddRange(pair.Value.messages);

						FileWorker.SaveToFile(messagesFilePath, loadedMessages);
					}
					
				consoleLogger.Log(LogLevel.Warning, "Finished saving files");
			});

			app.UseRouting();

			app.UseCors(builder =>
			{
				//builder.WithOrigins("http://192.168.1.38", "http://192.168.1.47", "http://192.168.1.1", "http://localost", "http://127.0.0.1");
				builder.AllowAnyOrigin();
			});

			app.UseEndpoints(endpoints =>
			{
				//endpoints.MapGet("/", async context => { await context.Response.WriteAsync("Hello World!"); });
				endpoints.MapControllers();
			});

			app.Run(async context =>
			{
				context.Response.StatusCode = StatusCodes.Status404NotFound;
				await context.Response.WriteAsync("<h1>404 Not Found</h1>");

				#region Logging
				if (Server.config.EnableFileLogging)
				{
					fileLogger.Log(LogLevel.Information, "[{0}] {1,6} {2} {3}",
						System.DateTime.Now.ToString("HH:mm:ss dd.MM.yyyy"),
						context.Request.Method,
						context.Response.StatusCode,
						context.Request.Path
					);
				}
				consoleLogger.Log(
					LogLevel.Debug,
					"{0,6} {1} {2}",
					context.Request.Method,
					context.Response.StatusCode,
					context.Request.Path
				);
				#endregion
			});
		}
	}
}
