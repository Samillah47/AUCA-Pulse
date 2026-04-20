using AUCAPulse.Models;

namespace AUCAPulse.Services
{
    public interface IMessageService
    {
        Task<ChatMessage> SendMessageAsync(int senderId, int receiverId, string content);
        Task<List<ChatMessage>> GetConversationAsync(int user1Id, int user2Id);
        Task<List<ChatSummaryDto>> GetChatSummariesAsync(int userId);
        Task MarkAsReadAsync(int receiverId, int senderId);
    }

    public class ChatSummaryDto
    {
        public int OtherUserId { get; set; }
        public string OtherUserName { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
        public DateTime LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
        public bool LastMessageIsFromMe { get; set; }
        public bool LastMessageIsRead { get; set; }
    }
}
