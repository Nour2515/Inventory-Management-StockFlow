using Azure.Core;
using Microsoft.AspNetCore.Identity;
using StockFlow.Data;
using StockFlow.DTOs;
using StockFlow.Models;

namespace StockFlow.Services
{
    public class AuthServices
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly AppDbContext _context;

        public AuthServices(UserManager<User> userManager, SignInManager<User> signInManager, AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request) { 
        var finduser=await _userManager.FindByEmailAsync(request.Email);
            if (finduser != null)
            {
                throw new Exception("Email is already registered");
            }
            User user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                UserName = request.Email,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            IdentityResult result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                throw new Exception("Failed to create user");
            }
            await _signInManager.SignInAsync(user, false);
            //default role is customer
            await _userManager.AddToRoleAsync(user, "Customer");

            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };
        }

    }
}
