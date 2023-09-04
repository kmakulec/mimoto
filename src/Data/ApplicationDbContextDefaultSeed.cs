using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Data
{
    public class ApplicationDbContextDefaultSeed
    {
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher = new PasswordHasher<ApplicationUser>();
        private readonly ApplicationDbContext _appDbContext;

        public ApplicationDbContextDefaultSeed(ApplicationDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task SeedAsync()
        {
            if (!_appDbContext.Users.Any())
            {
                await _appDbContext.Users.AddAsync(GetDefaultUser());
                await _appDbContext.SaveChangesAsync();
            }
        }

        private ApplicationUser GetDefaultUser()
        {
            var user = new ApplicationUser()
            {
                Email = "gokus007@o2.pl",
                Id = Guid.NewGuid().ToString(),
                PhoneNumber = "1234567890",
                UserName = "gokus007@o2.pl",
                NormalizedEmail = "GOKUS007@O2.PL",
                NormalizedUserName = "GOKUS007@O2.PL",
                SecurityStamp = Guid.NewGuid().ToString("D"),
                EmailConfirmed = true
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, "Pass@word1");

            return user;
        }
    }
}
