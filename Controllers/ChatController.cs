using AUCAPulse.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AUCAPulse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public ChatController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value 
                              ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int senderId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var message = await _messageService.SendMessageAsync(senderId, request.ReceiverId, request.Content);
            return Ok(message);
        }

        [HttpGet("conversation/{otherUserId}")]
        public async Task<IActionResult> GetConversation(int otherUserId)
        {
            var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value 
                              ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var messages = await _messageService.GetConversationAsync(currentUserId, otherUserId);
            
            // Mark messages as read when opening the conversation
            await _messageService.MarkAsReadAsync(currentUserId, otherUserId);
            
            return Ok(messages);
        }

        [HttpGet("summaries")]
        public async Task<IActionResult> GetChatSummaries()
        {
            var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value 
                              ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var summaries = await _messageService.GetChatSummariesAsync(currentUserId);
            return Ok(summaries);
        }

        [HttpPost("mark-read/{senderId}")]
        public async Task<IActionResult> MarkAsRead(int senderId)
        {
            var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value 
                              ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            await _messageService.MarkAsReadAsync(currentUserId, senderId);
            return Ok();
        }

        public class SendMessageRequest
        {
            public int ReceiverId { get; set; }
            public string Content { get; set; } = string.Empty;
        }
    }
}
