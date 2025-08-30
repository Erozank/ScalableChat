using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScalableChat.Api.Repositories;
using System.Security.Claims;

namespace ScalableChat.Api.Controllers
{
    public class ChatsController : ControllerBase
    {
        private readonly IChatRepository _chatRepository;
        public ChatsController(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
        }

        [Authorize]
        [HttpPost("chat")]
        public async Task<IActionResult> CreateDirectChat(Guid friendId)
        {
            var stringUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (stringUserId == null || !Guid.TryParse(stringUserId, out Guid userId) || friendId == Guid.Empty)
            {
                return BadRequest("Invalid user ID.");
            }

            var chatId = await _chatRepository.CreateDirectChatIfNotExistsAsync(userId, friendId);
            return Ok(new { ChatId = chatId });
        }

        [Authorize]
        [HttpGet("chats")]
        public async Task<IActionResult> GetUserChats()
        {
            var stringUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (stringUserId == null || !Guid.TryParse(stringUserId, out Guid userId))
            {
                return BadRequest("Invalid user ID.");
            }

            var chats = await _chatRepository.GetUserChatsAsync(userId);
            return Ok(chats);
        }
    }
}
