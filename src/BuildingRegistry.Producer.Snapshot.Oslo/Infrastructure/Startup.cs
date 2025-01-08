namespace BuildingRegistry.Producer.Snapshot.Oslo.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Mime;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Formatters.Json;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Be.Vlaanderen.Basisregisters.Projector.Controllers;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using SqlStreamStore;

    /// <summary>
    /// Extract to Library when working correctly
    /// </summary>
    public static class ApplicationBuilderProjectorExtensions
    {
        public static IApplicationBuilder UseProjectorEndpoints(
            this IApplicationBuilder builder,
            string baseUrl,
            JsonSerializerSettings? jsonSerializerSettings)
        {
            ArgumentNullException.ThrowIfNull(baseUrl);

            builder.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/v1/projections", async context => { await GetProjections(builder, context, baseUrl, jsonSerializerSettings); });
                endpoints.MapGet("/projections", async context => { await GetProjections(builder, context, baseUrl, jsonSerializerSettings); });

                endpoints.MapPost("/projections/start/all", async context => { await StartAll(builder, context); });
                endpoints.MapPost("/v1/projections/start/all", async context => { await StartAll(builder, context); });

                endpoints.MapPost("/projections/start/{projectionId}", async context
                    => await StartProjection(builder, context.Request.RouteValues["projectionId"].ToString(), context));
                endpoints.MapPost("/v1/projections/start/{projectionId}", async context
                    => await StartProjection(builder, context.Request.RouteValues["projectionId"].ToString(), context));

                endpoints.MapPost("/projections/stop/all", async context => { await StopAll(builder, context); });
                endpoints.MapPost("/v1/projections/stop/all", async context => { await StopAll(builder, context); });

                endpoints.MapPost("/projections/stop/{projectionId}", async context
                    => await StopProjection(builder, context.Request.RouteValues["projectionId"].ToString(), context));
                endpoints.MapPost("/v1/projections/stop/{projectionId}", async context
                    => await StopProjection(builder, context.Request.RouteValues["projectionId"].ToString(), context));
            });

            return builder;
        }

        private static async Task StopProjection(IApplicationBuilder app, string? projectionId, HttpContext context)
        {
            var manager = app.ApplicationServices.GetRequiredService<IConnectedProjectionsManager>();
            if (!manager.Exists(projectionId))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Invalid projection Id.");
                return;
            }

            await manager.Stop(projectionId, CancellationToken.None);
            context.Response.StatusCode = StatusCodes.Status202Accepted;
        }

        private static async Task StopAll(IApplicationBuilder app, HttpContext context)
        {
            var manager = app.ApplicationServices.GetRequiredService<IConnectedProjectionsManager>();
            await manager.Stop(CancellationToken.None);

            context.Response.StatusCode = StatusCodes.Status202Accepted;
        }

        private static async Task StartProjection(IApplicationBuilder app, string? projectionId, HttpContext context)
        {
            var manager = app.ApplicationServices.GetRequiredService<IConnectedProjectionsManager>();

            if (!manager.Exists(projectionId))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Invalid projection Id.");
                return;
            }

            await manager.Start(projectionId, CancellationToken.None);
            context.Response.StatusCode = StatusCodes.Status202Accepted;
        }

        private static async Task StartAll(IApplicationBuilder app, HttpContext context)
        {
            var manager = app.ApplicationServices.GetRequiredService<IConnectedProjectionsManager>();
            await manager.Start(CancellationToken.None);

            context.Response.StatusCode = StatusCodes.Status202Accepted;
        }

        private static async Task GetProjections(
            IApplicationBuilder app,
            HttpContext context,
            string baseUrl,
            JsonSerializerSettings? jsonSerializerSettings = null)
        {
            var manager = app.ApplicationServices.GetRequiredService<IConnectedProjectionsManager>();
            var streamStore = app.ApplicationServices.GetRequiredService<IStreamStore>();

            var registeredConnectedProjections = manager
                .GetRegisteredProjections()
                .ToList();
            var projectionStates = await manager.GetProjectionStates(CancellationToken.None);
            var responses = registeredConnectedProjections.Aggregate(
                new List<ProjectionResponse>(),
                (list, projection) =>
                {
                    var projectionState = projectionStates.SingleOrDefault(x => x.Name == projection.Id);
                    list.Add(new ProjectionResponse(
                        projection,
                        projectionState,
                        baseUrl));
                    return list;
                });

            var streamPosition = await streamStore.ReadHeadPosition();

            var projectionResponseList = new ProjectionResponseList(responses, baseUrl)
            {
                StreamPosition = streamPosition
            };

            var json = JsonConvert.SerializeObject(projectionResponseList, jsonSerializerSettings ?? new JsonSerializerSettings());

            context.Response.Headers.ContentType = MediaTypeNames.Application.Json;
            await context.Response.WriteAsync(json);
        }
    }

    public sealed class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseCors();

            app.UseHealthChecks("/health");

            var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
            var baseUri = configuration.GetValue<string>("BaseUrl").TrimEnd('/');
            app.UseProjectorEndpoints(baseUri, new JsonSerializerSettings().ConfigureDefaultForApi());
        }
    }
}
