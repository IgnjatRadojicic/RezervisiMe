using RezervisiMe.RezervisiMe.API.Models;
using RezervisiMe.RezervisiMe.API.Repositories;
using RezervisiMe.RezervisiMe.API.Repositories.Interfaces;
using RezervisiMe.RezervisiMe.API.Services;

namespace RezervisiMe.RezervisiMe.API.Infrastructure
{
    public class Composition
    {
        private static readonly JsonFileStore<User> _userStore =
            new JsonFileStore<User>("users.json");
        private static readonly JsonFileStore<Accommodation> _accommodationStore =
            new JsonFileStore<Accommodation>("accommodations.json");
        private static readonly JsonFileStore<Reservation> _reservationStore =
            new JsonFileStore<Reservation>("reservations.json");
        private static readonly JsonFileStore<Review> _reviewStore =
            new JsonFileStore<Review>("reviews.json");

        public static readonly IUserRepository Users
            = new UserRepository(_userStore);
        public static readonly IAccommodationRepository Accommodations
            = new AccommodationRepository(_accommodationStore);
        public static readonly IReservationRepository Reservations
            = new ReservationRepository(_reservationStore);
        public static readonly IReviewRepository Reviews
            = new ReviewRepository(_reviewStore);

        public static readonly IUserService UserService
            = new UserService(Users, Reservations);
        public static readonly IAccommodationService AccommodationService
            = new AccommodationService(Accommodations, Reservations, Users, Reviews);
        public static readonly IReservationService ReservationService
            = new ReservationService(Reservations, Accommodations, Users);
        public static readonly IReviewService ReviewService
            = new ReviewService(Reviews, Reservations, Users);
    }
}
}