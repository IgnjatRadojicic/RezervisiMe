using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RezervisiMe.RezervisiMe.API.Models.Dto
{
    public class ReviewDto
    {
        public Guid Id { get; set; }
        public Guid AccommodationId { get; set; }
        public string AccommodationName { get; set; }
        public Guid ReviewerId { get; set; }
        public string ReviewerUserName { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
        public string ImagePath { get; set; }
        public ReviewStatus Status { get; set; } = ReviewStatus.KREIRANA;
    }
}