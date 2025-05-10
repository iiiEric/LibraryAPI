using AutoMapper;
using LibraryAPI.Data;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using LibraryAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly SignInManager<User> _signInManager;
        private readonly IUsersService _usersServicies;
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IMapper _mapper;

        public UsersController(UserManager<User> userManager, IConfiguration configuration, SignInManager<User> signInManager, 
            IUsersService usersServicies, ApplicationDbContext applicationDbContext, IMapper mapper)
        {
            this._userManager = userManager;
            this._configuration = configuration;
            this._signInManager = signInManager;
            this._usersServicies = usersServicies;
            this._applicationDbContext = applicationDbContext;
            this._mapper = mapper;
        }

        [HttpGet]
        [Authorize("Admin")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> Get()
        {
            var users = await _applicationDbContext.Users.ToListAsync();
            var usersDTO = _mapper.Map<IEnumerable<UserDTO>>(users);
            return Ok(usersDTO);
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthenticationResponseDTO>> Register(UserCredentialsDTO userCredentialsDTO)
        {
            var user = new User
            {
                UserName = userCredentialsDTO.Email,
                Email = userCredentialsDTO.Email
            };

            var result = await _userManager.CreateAsync(user, userCredentialsDTO.Password!);

            if (result.Succeeded)
            {
                var authenticationResponse = await CreateToken(userCredentialsDTO);
                return Ok(authenticationResponse);
            }
            else
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("RegistrationError", error.Description);
                
                return ValidationProblem();
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthenticationResponseDTO>> Login(UserCredentialsDTO userCredentialsDTO)
        {
            var user = await _userManager.FindByEmailAsync(userCredentialsDTO.Email);
            if (user is null)
                ReturnIncorrectLogin();

            var result = await _signInManager.CheckPasswordSignInAsync(user!, userCredentialsDTO.Password!, false);
            if (result.Succeeded)
            {
                var authenticationResponse = await CreateToken(userCredentialsDTO);
                return Ok(authenticationResponse);
            }
            else
            {
                return ReturnIncorrectLogin();
            }
        }

        [HttpGet("refresh-token")]
        [Authorize]
        public async Task<ActionResult<AuthenticationResponseDTO>> RefreshToken()
        {
            var user = await _usersServicies.GetCurrentUser();
            if (user is null)
                return NotFound();

            var userCredentialsDTO = new UserCredentialsDTO
            {
                Email = user.Email!,
                Password = null // Password is not needed for token refresh
            };

            var authenticationResponse = await CreateToken(userCredentialsDTO);
            return Ok(authenticationResponse);
        }

        [HttpPost("create-admin")]
        [Authorize(Policy = "SuperAdmin")]
        public async Task<ActionResult> CreateAdmin(ClaimUpdateDTO claimUpdateDTO)
        {
            var user = await _userManager.FindByEmailAsync(claimUpdateDTO.Email);
            if (user is null)
                return NotFound();

            await _userManager.AddClaimAsync(user, new Claim("Admin", "true"));
            return NoContent();
        }

        [HttpPost("delete-admin")]
        [Authorize(Policy = "SuperAdmin")]
        public async Task<ActionResult> DeleteAdmin(ClaimUpdateDTO claimUpdateDTO)
        {
            var user = await _userManager.FindByEmailAsync(claimUpdateDTO.Email);
            if (user is null)
                return NotFound();

            await _userManager.RemoveClaimAsync(user, new Claim("Admin", "true"));
            return NoContent();
        }

        [HttpPut]
        [Authorize]
        public async Task<ActionResult> Put(UserUpdateDTO userUpdateDTO)
        {
            var user = await _usersServicies.GetCurrentUser();
            if (user is null)
                return NotFound();

            user.DateOfBirth = userUpdateDTO.DateOfBirth;
            
            await _userManager.UpdateAsync(user);
            return NoContent(); 
        }

        private ActionResult ReturnIncorrectLogin()
        {
            ModelState.AddModelError("LoginError", "Invalid credentials");
            return ValidationProblem();
        }

        private async Task<AuthenticationResponseDTO> CreateToken(UserCredentialsDTO userCredentialsDTO)
        {
            var claimsCollection = new List<Claim>
            {
                new Claim(ClaimTypes.Email, userCredentialsDTO.Email)
            };
            var user = await _userManager.FindByEmailAsync(userCredentialsDTO.Email);
            var claims = await _userManager.GetClaimsAsync(user!);
            claimsCollection.AddRange(claims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWTKey"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddDays(31);

            var securityToken = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claimsCollection,
                expires: expiration,
                signingCredentials: credentials
            );

            var token = new JwtSecurityTokenHandler().WriteToken(securityToken);

            return new AuthenticationResponseDTO
            {
                Token = token,
                Expiration = expiration
            };
        }
    }
}
