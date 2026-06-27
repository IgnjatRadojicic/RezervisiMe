using System;
using System.Collections.Generic;
using System.Linq;
using RezervisiMe.RezervisiMe.API.Infrastructure;
using RezervisiMe.RezervisiMe.API.Models;
using RezervisiMe.RezervisiMe.API.Models.Dto;
using RezervisiMe.RezervisiMe.API.Models.Requests;
using RezervisiMe.RezervisiMe.API.Repositories.Interfaces;

namespace RezervisiMe.RezervisiMe.API.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviews;
        private readonly IReservationRepository _reservations;
        private readonly IUserRepository _users;

        public ReviewService(
            IReviewRepository reviews,
            IReservationRepository reservations,
            IUserRepository users)
        {
            _reviews = reviews ?? throw new ArgumentNullException(nameof(reviews));
            _reservations = reservations ?? throw new ArgumentNullException(nameof(reservations));
            _users = users ?? throw new ArgumentNullException(nameof(users));
        }

        public Result<ReviewDto> Create(Guid reviewerId, CreateReviewRequest req)
        {
            if (req == null) return Error.Validation("Telo zahteva je obavezno");
            if (string.IsNullOrWhiteSpace(req.Title)) return Error.Validation("Naslov je obavezan");
            if (string.IsNullOrWhiteSpace(req.Content)) return Error.Validation("Sadržaj je obavezan");
            if (req.Rating < 1 || req.Rating > 5)
                return Error.Validation("Ocena mora biti između 1 i 5");

            var hasCompleted = _reservations.GetByGuest(reviewerId)
                .Any(r => r.AccommodationId == req.AccommodationId
                       && r.Status == ReservationStatus.ZAVRSENA);
            if (!hasCompleted)
                return Error.Forbidden(
                    "Recenzija je moguća samo za objekat koji si koristio (završena rezervacija)");

            var review = new Review
            {
                AccommodationId = req.AccommodationId,
                ReviewerId = reviewerId,
                Title = req.Title.Trim(),
                Content = req.Content.Trim(),
                Rating = req.Rating,
                ImagePath = req.ImagePath,
                Status = ReviewStatus.KREIRANA
            };
            _reviews.Add(review);

            var reviewer = _users.GetById(reviewerId);
            return MapToDto(review, reviewer);
        }

        public Result<ReviewDto> Update(Guid reviewId, UpdateReviewRequest req, Guid currentUserId)
        {
            if (req == null) return Error.Validation("Telo zahteva je obavezno");

            var r = _reviews.GetById(reviewId);
            if (r == null) return Error.NotFound("Recenzija ne postoji");
            if (r.ReviewerId != currentUserId)
                return Error.Forbidden("Možeš da menjaš samo svoje recenzije");
            if (string.IsNullOrWhiteSpace(req.Title)) return Error.Validation("Naslov je obavezan");
            if (string.IsNullOrWhiteSpace(req.Content)) return Error.Validation("Sadržaj je obavezan");
            if (req.Rating < 1 || req.Rating > 5)
                return Error.Validation("Ocena mora biti između 1 i 5");

            r.Title = req.Title.Trim();
            r.Content = req.Content.Trim();
            r.Rating = req.Rating;
            if (req.ImagePath != null) r.ImagePath = req.ImagePath;
            r.Status = ReviewStatus.KREIRANA;

            _reviews.Update(r);

            var reviewer = _users.GetById(r.ReviewerId);
            return MapToDto(r, reviewer);
        }

        public Result Delete(Guid reviewId, Guid currentUserId, UserRole currentUserRole)
        {
            var r = _reviews.GetById(reviewId);
            if (r == null) return Error.NotFound("Recenzija ne postoji");
            if (currentUserRole == UserRole.Gost && r.ReviewerId != currentUserId)
                return Error.Forbidden("Možeš da brišeš samo svoje recenzije");

            _reviews.SoftDelete(reviewId);
            return Result.Success();
        }

        public Result<ReviewDto> Approve(Guid reviewId)
        {
            var r = _reviews.GetById(reviewId);
            if (r == null) return Error.NotFound("Recenzija ne postoji");
            r.Status = ReviewStatus.ODOBRENA;
            _reviews.Update(r);

            var reviewer = _users.GetById(r.ReviewerId);
            return MapToDto(r, reviewer);
        }

        public Result<ReviewDto> Reject(Guid reviewId)
        {
            var r = _reviews.GetById(reviewId);
            if (r == null) return Error.NotFound("Recenzija ne postoji");
            r.Status = ReviewStatus.ODBIJENA;
            _reviews.Update(r);

            var reviewer = _users.GetById(r.ReviewerId);
            return MapToDto(r, reviewer);
        }

        public Result<ReviewDto> GetById(Guid id)
        {
            var r = _reviews.GetById(id);
            if (r == null) return Error.NotFound("Recenzija ne postoji");

            var reviewer = _users.GetById(r.ReviewerId);
            return MapToDto(r, reviewer);
        }

        public IEnumerable<ReviewDto> GetForAccommodation(Guid accommodationId, UserRole? currentUserRole)
        {
            var q = _reviews.GetByAccommodation(accommodationId).AsEnumerable();
            if (currentUserRole != UserRole.Administrator)
                q = q.Where(r => r.Status == ReviewStatus.ODOBRENA);

            var list = q.ToList();
            if (list.Count == 0) return new List<ReviewDto>();

            var usersById = _users.GetAllIncludingDeleted().ToDictionary(u => u.Id);

            return list.Select(r =>
            {
                usersById.TryGetValue(r.ReviewerId, out var reviewer);
                return MapToDto(r, reviewer);
            }).ToList();
        }

        public IEnumerable<ReviewDto> GetByReviewer(Guid reviewerId)
        {
            var list = _reviews.GetByReviewer(reviewerId).ToList();
            if (list.Count == 0) return new List<ReviewDto>();

            var reviewer = _users.GetById(reviewerId);
            return list.Select(r => MapToDto(r, reviewer)).ToList();
        }

        public IEnumerable<ReviewDto> GetAll(ReviewStatus? statusFilter)
        {
            var q = _reviews.GetAll().AsEnumerable();
            if (statusFilter.HasValue)
                q = q.Where(r => r.Status == statusFilter.Value);

            var list = q.ToList();
            if (list.Count == 0) return new List<ReviewDto>();

            var usersById = _users.GetAllIncludingDeleted().ToDictionary(u => u.Id);

            return list.Select(r =>
            {
                usersById.TryGetValue(r.ReviewerId, out var reviewer);
                return MapToDto(r, reviewer);
            }).ToList();
        }

        private static ReviewDto MapToDto(Review r, User reviewer)
        {
            return new ReviewDto
            {
                Id = r.Id,
                AccommodationId = r.AccommodationId,
                ReviewerId = r.ReviewerId,
                ReviewerUserName = reviewer?.UserName,
                Title = r.Title,
                Content = r.Content,
                Rating = r.Rating,
                ImagePath = r.ImagePath,
                Status = r.Status
            };
        }
    }
}