using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Micro.Health;

/// <inheritdoc cref="AddAzureServiceBusTopicHealthCheck"/>
public static class XAzureServiceBusHealthCheck
{
	/// <summary> Checks if the <paramref name="topicName"/> can be reached.</summary>
	/// <remarks> Requires a Reference to the Microsoft.Azure.ServiceBus.nuGet Package</remarks>
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
	public static IHealthChecksBuilder AddAzureServiceBusTopicHealthCheck(this IHealthChecksBuilder builder
		, string azureConnectionString, string topicName
		, string checkName = default, HealthStatus failureStatus = HealthStatus.Degraded
		, IEnumerable<string> tags = default, TimeSpan? timeout = default)
		=> builder.AddCheck(checkName ?? $"Azure Service Bus: {topicName}",
			new AzureServiceBusHealthCheck(azureConnectionString, topicName), failureStatus, tags, timeout);
}

public class AzureServiceBusHealthCheck : IHealthCheck
{
	readonly ManagementClient ManagementClient_;
	readonly string TopicName_;

	public AzureServiceBusHealthCheck(string connectionString, string topicName)
	{
		ManagementClient_ = new ManagementClient(connectionString);
		TopicName_ = topicName;
	}

	public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
	{
		try
		{
			_ = await ManagementClient_.GetTopicRuntimeInfoAsync(TopicName_, cancellationToken);

			return HealthCheckResult.Healthy();
		}
		catch (Exception e)
		{
			return new HealthCheckResult(context.Registration.FailureStatus, exception: e);
		}
	}
}