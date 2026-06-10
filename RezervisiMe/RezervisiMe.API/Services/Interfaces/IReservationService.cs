using System;
using System.Collections.Generic;
using RezervisiMe.RezervisiMe.API.Infrastructure;
using RezervisiMe.RezervisiMe.API.Models;
using RezervisiMe.RezervisiMe.API.Models.Dto;
using RezervisiMe.RezervisiMe.API.Models.Requests;

namespace RezervisiMe.RezervisiMe.API.Services
{
    public interface IReservationService
    {
        Result<ReservationDto> Create(Guid guestId, CreateReservationRequest req);
        Result<ReservationDto> Cancel(Guid reservationId, Guid currentUserId, UserRole currentUserRole);
        Result<ReservationDto> Approve(Guid reservationId);
        Result<ReservationDto> Reject(Guid reservationId);
        Result<ReservationDto> GetById(Guid id);
        IEnumerable<ReservationDto> GetByGuest(Guid guestId, ReservationStatus? statusFilter);
        IEnumerable<ReservationDto> GetAll(ReservationStatus? statusFilter, Guid? accommodationIdFilter);
        void RefreshCompletedStatuses();
    }
}