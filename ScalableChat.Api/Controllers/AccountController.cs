using ScalableChat.Api.Entities;
using ScalableChat.Api.Infrastucture;
using ScalableChat.Api.Models;
using ScalableChat.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ScalableChat.Api.Controllers
{
    public class AccountController(ILogger<AccountController> logger, IUserRepository userRepository, TokenProvider tokenProvider, IPasswordHasher passwordHasher) : ControllerBase
    {
        private readonly ILogger<AccountController> _logger = logger;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly TokenProvider _tokenProvider = tokenProvider;
        private readonly IPasswordHasher _passwordHasher = passwordHasher;

        // POST /account/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Validar el request
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid request: {0}", request);
                return BadRequest(ModelState);
            }
            // Validar el usuario
            var user = await _userRepository.GetByNickname(request.Nickname);
            if (user == null)
            {
                _logger.LogWarning("Invalid credentials for user: {0}", request.Nickname);
                return Unauthorized();
            }

            bool isCorrectPassword = _passwordHasher.Verify(request.Password, user.PasswordHash);
            if (!isCorrectPassword)
            {
                _logger.LogWarning("Invalid credentials for user: {0}", request.Nickname);
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
                _logger.LogWarning("Invalid request: {0}", request);
                return BadRequest(ModelState);
            }
            // Nickname already exists
            bool existsUser = await _userRepository.Exists(request.Nickname);
            if (existsUser)
            {
                _logger.LogWarning("Nickname already registered: {0}", request.Nickname);
                return Conflict();
            }
            // Crear el usuario
            var user = new UserEntity
            {
                Id = Guid.NewGuid(),
                Nickname = request.Nickname,
                PasswordHash = _passwordHasher.Hash(request.Password)
            };
            await _userRepository.Insert(user);
            // Crear el token
            var token = _tokenProvider.Create(user);
            return Ok(new { token });
        }
    }
}
