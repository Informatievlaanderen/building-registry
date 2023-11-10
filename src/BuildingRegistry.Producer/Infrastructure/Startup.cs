namespace BuildingRegistry.Producer.Infrastructure
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Be.Vlaanderen.Basisregisters.Projector.Controllers;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using SqlStreamStore;

    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseHealthChecks("/health");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("v1/projections", async context =>
                {
                    var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
                    var manager = app.ApplicationServices.GetRequiredService<IConnectedProjectionsManager>();
                    var streamStore = app.ApplicationServices.GetRequiredService<IStreamStore>();

                    var baseUri = configuration.GetValue<string>("BaseUrl");

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
                                baseUri));
                            return list;
                        });

                    var streamPosition = await streamStore.ReadHeadPosition();

                    var projectionResponseList = new ProjectionResponseList(responses, baseUri)
                    {
                        StreamPosition = streamPosition
                    };

                    await context.Response.WriteAsJsonAsync(projectionResponseList);
                });
            });
        }
    }
}
