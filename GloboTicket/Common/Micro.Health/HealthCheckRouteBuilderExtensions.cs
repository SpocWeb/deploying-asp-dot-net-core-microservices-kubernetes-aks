using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Micro.Health;

/// <inheritdoc cref="MapDefaultHealthChecks"/>
public static class HealthCheckRouteBuilderExtensions
{
    /// <summary> Simple Liveness Endpoint; always returns HTTP.OK </summary>
    /// <remarks> runs none of the registered HealthChecks. </remarks>
    public const string HealthLive = "/health/live";

    /// <summary> Simple Readiness Endpoint; returns HTTP.OK when </summary>
    /// <remarks>
    /// Runs all of the registered HealthChecks.
    /// Requires to register them using the following Code:
    /// </remarks>
    /// <example>
    /// <code lang="cs">
    /// services.AddHealthChecks()
    ///     .AddDbContextCheck&lt;EventCatalogDbContext>();
    ///     .AddAzureServiceBusTopicHealthCheck(Configuration["ServiceBusConnectionString"],
    ///         Configuration["OrderPaymentRequestMessageTopic"], "Order Payment Request Topic", HealthStatus.Unhealthy)
    ///     .AddUrlGroup(new Uri($"{config["ApiConfigs:EventCatalog:Uri"]}/health/live"),
    ///         "Event Catalog API", HealthStatus.Degraded, timeout: TimeSpan.FromSeconds(1));
    /// </code>
    /// </example>
    public const string HealthReady = "/health/ready";

    /// <summary> Registers <see cref="HealthLive"/> and <see cref="HealthReady"/> Endpoints </summary>
    /// <example>
    /// <code lang="cs">
    /// app.UseEndpoints(endpoints => {
    ///     endpoints.MapDefaultHealthChecks();
    /// }
    /// </code>
    /// </example>
    public static IEndpointRouteBuilder MapDefaultHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks(HealthLive, new HealthCheckOptions
        { //runs none of the registered HealthChecks
            Predicate = _ => false,
            ResponseWriter = WriteJsonResponse
        });
        endpoints.MapHealthChecks(HealthReady, new HealthCheckOptions
        { //runs all of the registered HealthChecks
            ResponseWriter = WriteJsonResponse
        });

        return endpoints;
    }

    /// <summary>Writes JSON of the <paramref name="report"/> </summary>
    public static Task WriteJsonResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var options = new JsonWriterOptions { Indented = true };

        using var writer = new Utf8JsonWriter(context.Response.BodyWriter, options);

        writer.WriteStartObject();
        writer.WriteString("status", report.Status.ToString());

        if (report.Entries.Count > 0)
        {
            writer.WriteStartArray("results");

            foreach (var (key, value) in report.Entries)
            {
                writer.WriteStartObject();
                writer.WriteString("key", key);
                writer.WriteString("status", value.Status.ToString());
                writer.WriteString("description", value.Description);
                writer.WriteStartArray("data");
                foreach (var (dataKey, dataValue) in value.Data.Where(d => d.Value is object))
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName(dataKey);
                    JsonSerializer.Serialize(writer, dataValue, dataValue.GetType());
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }

        writer.WriteEndObject();

        return Task.CompletedTask;
    }
}