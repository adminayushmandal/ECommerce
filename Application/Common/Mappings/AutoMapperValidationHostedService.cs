using AutoMapper;
using Microsoft.Extensions.Hosting;

namespace Application.Common.Mappings;

internal sealed class AutoMapperValidationHostedService(IConfigurationProvider configurationProvider) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        configurationProvider.AssertConfigurationIsValid();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
