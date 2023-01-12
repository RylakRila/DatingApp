using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;

        public AccountController(DataContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }
        
        [HttpPost("register")] // POST api/account/register
        public async Task<ActionResult<RegisteredDTO>> Register(RegisterDTO registerDTO) 
        {
            if (await UserExists(registerDTO.UserName)) return BadRequest("Username is taken"); 
            
            using var hmac = new HMACSHA512();
            
            var user = new AppUser
            {
                UserName = registerDTO.UserName,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)),
                PasswordSalt = hmac.Key
            };
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            return new RegisteredDTO
            {
                Username = user.UserName
            };
        }
        
        [HttpPost("login")] // POST api/account/login
        public async Task<ActionResult<LoggedDTO>> Login(LoginDTO loginDTO)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => 
                x.UserName.ToLower() == loginDTO.Username.ToLower());
            
            if (user == null) return Unauthorized("Credentials are invalid");
            
            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));
            
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Credentials are invalid");
            }
            
            return new LoggedDTO
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }
        
        private async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(x => 
                x.UserName.ToLower() == username.ToLower());
        }
    }   
}
