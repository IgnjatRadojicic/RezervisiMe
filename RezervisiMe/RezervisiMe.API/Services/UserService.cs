using RezervisiMe.RezervisiMe.API.Infrastructure;
using RezervisiMe.RezervisiMe.API.Models;
using RezervisiMe.RezervisiMe.API.Models.Dto;
using RezervisiMe.RezervisiMe.API.Models.Requests;
using RezervisiMe.RezervisiMe.API.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RezervisiMe.RezervisiMe.API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _users;
        private readonly IReservationRepository _reservations;

        public UserService(IUserRepository users, IReservationRepository reservations)
        {
            _users = users ?? throw new ArgumentNullException(nameof(users));
            _reservations = reservations ?? throw new ArgumentNullException(nameof(reservations));
        }


        public Result<UserDto> Register(RegisterRequest req, bool createdByAdmin)
        {
            if (req == null)
                return Error.Validation("Telo zahteva je obavezno");
            if (string.IsNullOrWhiteSpace(req.Username))
                return Error.Validation("Username je obavezan");
            if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 6)
                return Error.Validation("Lozinka mora imati bar 6 karaktera");
            if (string.IsNullOrWhiteSpace(req.FirstName))
                return Error.Validation("Ime je obavezno");
            if (string.IsNullOrWhiteSpace(req.LastName))
                return Error.Validation("Prezime je obavezno");
            if (string.IsNullOrWhiteSpace(req.Email))
                return Error.Validation("Email je obavezan");
            if (req.DateOfBirth == default || req.DateOfBirth > DateTime.UtcNow)
                return Error.Validation("Datum rođenja nije validan");

            if (_users.UsernameExists(req.Username))
                return Error.Conflict("Username već postoji");

            var role = createdByAdmin ? req.Role : UserRole.Gost;
            if (role == UserRole.Administrator)
                return Error.Forbidden("Administrator se ne može kreirati kroz API");

            var user = new User
            {
                UserName = req.Username.Trim(),
                PasswordHash = PasswordHasher.Hash(req.Password),
                FirstName = req.FirstName.Trim(),
                LastName = req.LastName.Trim(),
                Email = req.Email.Trim(),
                DateOfBirth = req.DateOfBirth,
                Gender = req.Gender,
                Role = role
            };

            _users.Add(user);
            return MapToDto(user);   
        }


        public Result<UserDto> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return Error.Validation("Username i lozinka su obavezni");

            var user = _users.GetByUsername(username);

            if (user == null || !PasswordHasher.Verify(password, user.PasswordHash))
                return Error.Unauthorized("Neispravno korisničko ime ili lozinka");

            return MapToDto(user);
        }

        public Result<UserDto> GetById(Guid userId)
        {
            var user = _users.GetById(userId);
            if (user == null) return Error.NotFound("Korisnik ne postoji");
            return MapToDto(user);
        }

        public Result<UserDto> UpdateProfile(Guid userId, UpdateProfileRequest req)
        {
            if (req == null) return Error.Validation("Telo zahteva je obavezno");

            var user = _users.GetById(userId);
            if (user == null) return Error.NotFound("Korisnik ne postoji");

            if (string.IsNullOrWhiteSpace(req.FirstName))
                return Error.Validation("Ime je obavezno");
            if (string.IsNullOrWhiteSpace(req.LastName))
                return Error.Validation("Prezime je obavezno");
            if (string.IsNullOrWhiteSpace(req.Email))
                return Error.Validation("Email je obavezan");
            if (req.DateOfBirth == default || req.DateOfBirth > DateTime.UtcNow)
                return Error.Validation("Datum rođenja nije validan");

            user.FirstName = req.FirstName.Trim();
            user.LastName = req.LastName.Trim();
            user.Email = req.Email.Trim();
            user.DateOfBirth = req.DateOfBirth;
            user.Gender = req.Gender;

            if (!string.IsNullOrWhiteSpace(req.NewPassword))
            {
                if (req.NewPassword.Length < 6)
                    return Error.Validation("Nova lozinka mora imati bar 6 karaktera");
                user.PasswordHash = PasswordHasher.Hash(req.NewPassword);
            }

            _users.Update(user);
            return MapToDto(user);
        }


        public Result DeleteUser(Guid userId)
        {
            var user = _users.GetById(userId);
            if (user == null) return Error.NotFound("Korisnik ne postoji");

            if (user.Role == UserRole.Administrator)
                return Error.Forbidden("Administrator se ne može obrisati");

            if (user.Role == UserRole.Gost)
            {
                var toCancel = _reservations.GetByGuest(userId)
                    .Where(r => r.Status == ReservationStatus.KREIRANA
                             || r.Status == ReservationStatus.ODOBRENA)
                    .ToList();

                foreach (var r in toCancel)
                {
                    r.Status = ReservationStatus.OTKAZANA;
                    _reservations.Update(r);
                }
            }


            _users.SoftDelete(userId);
            return Result.Success();
        }


        public IEnumerable<UserDto> Search(UserSearchCriteria c)
        {
            c = c ?? new UserSearchCriteria();

            var q = _users.GetAll().Where(u => u.Role != UserRole.Administrator);

            if (!string.IsNullOrWhiteSpace(c.FirstName))
                q = q.Where(u => !string.IsNullOrEmpty(u.FirstName)
                    && u.FirstName.IndexOf(c.FirstName, StringComparison.OrdinalIgnoreCase) >= 0);

            if (!string.IsNullOrWhiteSpace(c.LastName))
                q = q.Where(u => !string.IsNullOrEmpty(u.LastName)
                    && u.LastName.IndexOf(c.LastName, StringComparison.OrdinalIgnoreCase) >= 0);

            if (c.DateOfBirthFrom.HasValue)
                q = q.Where(u => u.DateOfBirth >= c.DateOfBirthFrom.Value);

            if (c.DateOfBirthTo.HasValue)
                q = q.Where(u => u.DateOfBirth <= c.DateOfBirthTo.Value);

            if (c.Role.HasValue)
                q = q.Where(u => u.Role == c.Role.Value);

            var desc = string.Equals(c.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
            switch ((c.SortBy ?? "").ToLowerInvariant())
            {
                case "dateofbirth":
                    q = desc ? q.OrderByDescending(u => u.DateOfBirth) : q.OrderBy(u => u.DateOfBirth);
                    break;
                case "role":
                    q = desc ? q.OrderByDescending(u => u.Role) : q.OrderBy(u => u.Role);
                    break;
                case "name":
                default:
                    q = desc
                        ? q.OrderByDescending(u => u.FirstName).ThenByDescending(u => u.LastName)
                        : q.OrderBy(u => u.FirstName).ThenBy(u => u.LastName);
                    break;
            }

            return q.Select(MapToDto).ToList();
        }

        private static UserDto MapToDto(User u)
        {
            return new UserDto
            {
                Id = u.Id,
                UserName = u.UserName,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                DateOfBirth = u.DateOfBirth,
                Gender = u.Gender,
                Role = u.Role
            };
        }
    }
}