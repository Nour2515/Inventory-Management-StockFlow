using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StockFlow.Data;
using StockFlow.DTOs;
using StockFlow.Interfaces;
using StockFlow.Models;

namespace StockFlow.Services
{
    public class AuthServices : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly AppDbContext _context;

        public AuthServices(UserManager<User> userManager, SignInManager<User> signInManager, AppDbContext context, ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _tokenService = tokenService;
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
            //cookies
            //await _signInManager.SignInAsync(user, false);

            //default role is customer
            var roleResult =await _userManager.AddToRoleAsync(user, "Customer"); 
            if (!roleResult.Succeeded)
                throw new Exception("Failed to assign default role.");

            var accessToken = _tokenService.GenerateAccessToken(user);

            var refreshToken = _tokenService.GenerateRefreshToken(user);

            _context.RefreshTokens.Add(refreshToken);

            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };
        }
        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                throw new Exception("Invalid email or password.");
            if (!user.IsActive)
                throw new Exception("User account is inactive.");
            var passwordValid = await _userManager.CheckPasswordAsync(user,request.Password);
            if (!passwordValid)
                throw new Exception("Invalid email or password.");

            var accessToken = _tokenService.GenerateAccessToken(user);

            var refreshToken = _tokenService.GenerateRefreshToken(user);

            _context.RefreshTokens.Add(refreshToken);

            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };
        }
        public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
        {
            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedToken == null)
                throw new Exception("Invalid refresh token.");

            if (storedToken.RevokedAt != null)
                throw new Exception("Refresh token has been revoked.");

            if (storedToken.ExpiresAt <= DateTime.UtcNow)
                throw new Exception("Refresh token has expired.");

            if (!storedToken.User.IsActive)
                throw new Exception("User account is inactive.");

            // Revoke old refresh token
            storedToken.RevokedAt = DateTime.UtcNow;

            var newAccessToken =
                _tokenService.GenerateAccessToken(storedToken.User);

            var newRefreshToken =
                _tokenService.GenerateRefreshToken(storedToken.User);

            _context.RefreshTokens.Add(newRefreshToken);

            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token
            };
        }

        public async Task RevokeTokenAsync(string refreshToken)
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedToken == null)    
                throw new Exception("Invalid refresh token.");

            if (storedToken.RevokedAt != null)
                return;

            storedToken.RevokedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task CleanupRefreshTokensAsync()
        {
            var cutoffDate =DateTime.UtcNow.AddDays(-7);


            var oldTokens = await _context.RefreshTokens
                    .Where(rt =>rt.ExpiresAt <= cutoffDate||(rt.RevokedAt != null && rt.RevokedAt <= cutoffDate))
                    .ToListAsync();


            if (oldTokens.Count == 0)
                return;


            _context.RefreshTokens.RemoveRange(oldTokens);


            await _context.SaveChangesAsync();
        }
    }
}
