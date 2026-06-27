using System;

namespace RezervisiMe.RezervisiMe.API.Models.Requests
{
    public class CreateReviewRequest
    {
        public Guid AccommodationId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
        public string ImagePath { get; set; }
    }

    public class UpdateReviewRequest
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
        public string ImagePath { get; set; }   
    }
}