using System;
using System.Collections.Generic;
using RezervisiMe.RezervisiMe.API.Infrastructure;
using RezervisiMe.RezervisiMe.API.Models;
using RezervisiMe.RezervisiMe.API.Models.Dto;
using RezervisiMe.RezervisiMe.API.Models.Requests;

namespace RezervisiMe.RezervisiMe.API.Services
{
    public interface IAccommodationService
    {
        Result<AccommodationDto> Create(Guid hostId, CreateAccommodationRequest req);
        Result<AccommodationDto> Update(Guid id, UpdateAccommodationRequest req,
            Guid currentUserId, UserRole currentUserRole);
        Result Delete(Guid id, Guid currentUserId, UserRole currentUserRole);
        Result<AccommodationDto> GetById(Guid id);
        IEnumerable<AccommodationDto> Search(AccommodationSearchCriteria criteria);
    }
}