using Application.Common.Models;
using Application.Features.Users.Commands;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ECommerce.Server.Endpoints
{
    public class Users : EndpointGroupBase
    {
        public override void Map(RouteGroupBuilder groupBuilder)
        {
            groupBuilder.MapPost(Register, "register-profile")
                .WithSummary("Register user with profile")
                .WithDescription("Registers a new customer account with the required display name and identity credentials.")
                .Produces<UserDto>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status409Conflict);

            groupBuilder.MapIdentityApi<User>();
        }

        private static async Task<Created<UserDto>> Register(
            RegisterUserRequest request,
            ISender sender,
            CancellationToken cancellationToken)
        {
            var user = await sender.Send(
                new RegisterUserCommand(
                    request.DisplayName,
                    request.Email,
                    request.Password,
                    request.PhoneNumber),
                cancellationToken);

            return TypedResults.Created($"/api/Users/register-profile/{user.Id}", user);
        }

        public sealed record RegisterUserRequest(
            string DisplayName,
            string Email,
            string Password,
            string? PhoneNumber);
    }
}
