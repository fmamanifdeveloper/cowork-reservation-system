using Cowork.Domain.Entities;

namespace Cowork.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string Generate(AppUser user);
}