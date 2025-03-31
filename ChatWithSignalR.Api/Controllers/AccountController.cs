using ChatWithSignalR.Api.Models;
using ChatWithSignalR.Api.Users.Infrastucture;
using ChatWithSignalR.UsersDb;
using Microsoft.AspNetCore.Mvc;

namespace ChatWithSignalR.Api.Controllers
{
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly UsersDbContext _usersDbContext;
        private readonly TokenProvider _tokenProvider;

        public AccountController(ILogger<AccountController> logger, UsersDbContext usersDbContext, TokenProvider tokenProvider)
        {
            _logger = logger;
            _usersDbContext = usersDbContext;
            _tokenProvider = tokenProvider;
        }

        // POST /account/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // Validar el request
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid request: {0}", request);
                return BadRequest(ModelState);
            }
            // Validar el usuario
            var user = _usersDbContext.Users.FirstOrDefault(u => u.Email == request.Email && u.Password == request.Password);
            if (user == null)
            {
                _logger.LogError("Invalid credentials for user: {0}", request.Email);
                return Unauthorized();
            }
            // Crear el token
            var token = _tokenProvider.Create(user);
            return Ok(new { token });
        }

        // POST /account/register
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            // Validar el request
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid request: {0}", request);
                return BadRequest(ModelState);
            }
            // Validar si el email ya está registrado
            if (_usersDbContext.Users.Any(u => u.Email == request.Email))
            {
                _logger.LogError("Email already registered: {0}", request.Email);
                return Conflict();
            }
            // Crear el usuario
            var user = new User
            {
                Id = Guid.NewGuid(),
                Nickname = request.Nickname,
                Email = request.Email,
                Password = request.Password
            };
            _usersDbContext.Users.Add(user);
            _usersDbContext.SaveChanges();
            // Crear el token
            var token = _tokenProvider.Create(user);
            return Ok(new { token });
        }
    }
}
