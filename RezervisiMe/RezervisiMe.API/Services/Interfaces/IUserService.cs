using System;
using System.Collections.Generic;
using RezervisiMe.RezervisiMe.API.Infrastructure;
using RezervisiMe.RezervisiMe.API.Models.Dto;
using RezervisiMe.RezervisiMe.API.Models.Requests;

namespace RezervisiMe.RezervisiMe.API.Services
{
    public interface IUserService
    {
        Result<UserDto> Register(RegisterRequest req, bool createdByAdmin);
        Result<UserDto> Login(string username, string password);
        Result<UserDto> GetById(Guid userId);
        Result<UserDto> UpdateProfile(Guid userId, UpdateProfileRequest req);
        Result DeleteUser(Guid userId);
        IEnumerable<UserDto> Search(UserSearchCriteria criteria);
    }
}