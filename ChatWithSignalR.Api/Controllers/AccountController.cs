using ChatWithSignalR.Api.Infrastucture;
using ChatWithSignalR.Api.Models;
using ChatWithSignalR.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ChatWithSignalR.Api.Controllers
{
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IUserRepository _userRepository;
        private readonly TokenProvider _tokenProvider;
        private readonly IPasswordHasher _passwordHasher;

        public AccountController(ILogger<AccountController> logger, IUserRepository userRepository, TokenProvider tokenProvider, IPasswordHasher passwordHasher)
        {
            _logger = logger;
            _userRepository = userRepository;
            _tokenProvider = tokenProvider;
            _passwordHasher = passwordHasher;
        }

        // POST /account/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Validar el request
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid request: {0}", request);
                return BadRequest(ModelState);
            }
            // Validar el usuario
            var user = await _userRepository.GetByEmail(request.Email);
            if (user == null)
            {
                _logger.LogError("Invalid credentials for user: {0}", request.Email);
                return Unauthorized();
            }

            bool isCorrectPassword = _passwordHasher.Verify(request.Password, user.PasswordHash);
            if (!isCorrectPassword)
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
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Validar el request
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid request: {0}", request);
                return BadRequest(ModelState);
            }
            // Validar si el email ya está registradoç
            bool existsUser = await _userRepository.Exists(request.Email);
            if (existsUser)
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
                PasswordHash = _passwordHasher.Hash(request.Password)
            };
            await _userRepository.Insert(user);
            // Crear el token
            var token = _tokenProvider.Create(user);
            return Ok(new { token });
        }
    }
}
