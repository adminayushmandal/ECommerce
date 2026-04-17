using Application.Common.Models;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public sealed class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(
                dest => dest.Roles,
                opt => opt.MapFrom(src => src.UserRoles.Select(role => role.Role.Name).ToArray()));
    }
}
