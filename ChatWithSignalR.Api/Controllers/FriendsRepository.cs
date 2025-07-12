using ChatWithSignalR.Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ChatWithSignalR.Api.Controllers
{
    public class FriendsRepository(IFriendRequestRepository friendRequestRepository) : ControllerBase
    {
        private readonly IFriendRequestRepository _friendRequestRepository = friendRequestRepository;

        // GET /friends
        [Authorize]
        [HttpGet("friends")]
        public async Task<IActionResult> GetFriends()
        {
            var stringUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (stringUserId == null || !Guid.TryParse(stringUserId, out Guid userId))
            {
                return BadRequest("Invalid user ID.");
            }

            var friends = await _friendRequestRepository.GetFriendsAsync(userId);

            return Ok(new { friends });
        }
    }
}
