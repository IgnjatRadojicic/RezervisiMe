using RezervisiMe.RezervisiMe.API.Models;
using RezervisiMe.RezervisiMe.API.Repositories;
using RezervisiMe.RezervisiMe.API.Repositories.Interfaces;

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
    }
}