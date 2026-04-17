using Application.Common.Models;
using Application.Features.Lookup.Queries;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ECommerce.Server.Endpoints;

public sealed class Lookup : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetLookup)
            .WithSummary("Get application lookup")
            .WithDescription("Returns bootstrap lookup data for the web application, including the current user profile when the visitor is authenticated.")
            .Produces<LookupDto>();
    }

    private static async Task<Ok<LookupDto>> GetLookup(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var lookup = await sender.Send(new GetLookupQuery(), cancellationToken);
        return TypedResults.Ok(lookup);
    }
}
