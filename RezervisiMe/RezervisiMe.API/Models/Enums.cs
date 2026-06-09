using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RezervisiMe.RezervisiMe.API.Models
{
    public enum Gender
    {
        Muski,
        Zenski,
        Drugo
    }

    public enum UserRole
    {
        Gost,
        Domacin,
        Administrator
    }

    public enum AccommodationType
    {
        Hotel,
        Apartman,
        Hostel,
        Vila,
        Soba,
        Kuca,
        Motel
    }

    public enum ReservationStatus
    {
        KREIRANA,
        ODOBRENA,
        OTKAZANA,
        ZAVRSENA
    }

    public enum ReviewStatus
    {
        KREIRANA,
        ODOBRENA,
        ODBIJENA
    }
}