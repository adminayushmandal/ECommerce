using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Mappings;

public static class MappingExtensions
{
    public static Task<List<TDestination>> ProjectToListAsync<TDestination>(
        this IQueryable source,
        IConfigurationProvider configurationProvider,
        CancellationToken cancellationToken = default)
        => source.ProjectTo<TDestination>(configurationProvider).ToListAsync(cancellationToken);
}
