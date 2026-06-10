using System;
using System.Collections.Generic;
using RezervisiMe.RezervisiMe.API.Infrastructure;
using RezervisiMe.RezervisiMe.API.Models;
using RezervisiMe.RezervisiMe.API.Models.Dto;
using RezervisiMe.RezervisiMe.API.Models.Requests;

namespace RezervisiMe.RezervisiMe.API.Services
{
    public interface IReviewService
    {
        Result<ReviewDto> Create(Guid reviewerId, CreateReviewRequest req);
        Result<ReviewDto> Update(Guid reviewId, UpdateReviewRequest req, Guid currentUserId);
        Result Delete(Guid reviewId, Guid currentUserId, UserRole currentUserRole);
        Result<ReviewDto> Approve(Guid reviewId);
        Result<ReviewDto> Reject(Guid reviewId);
        IEnumerable<ReviewDto> GetForAccommodation(Guid accommodationId, UserRole? currentUserRole);
        IEnumerable<ReviewDto> GetByReviewer(Guid reviewerId);
        IEnumerable<ReviewDto> GetAll(ReviewStatus? statusFilter);
    }
}