using AUCAPulse.Data;
using AUCAPulse.Models;
using Microsoft.EntityFrameworkCore;

namespace AUCAPulse.Services
{
    public class MessageService : IMessageService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public MessageService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<ChatMessage> SendMessageAsync(int senderId, int receiverId, string content)
        {
            var message = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            // Create a notification for the receiver
            var sender = await _context.Users.FindAsync(senderId);
            await _notificationService.CreateNotificationAsync(new DTOs.Request.CreateNotificationDto
            {
                UserId = receiverId,
                Title = "New Message",
                Message = $"You received a new message from {sender?.Name ?? "a colleague"}.",
                Type = NotificationType.INFO
            });

            return message;
        }

        public async Task<List<ChatMessage>> GetConversationAsync(int user1Id, int user2Id)
        {
            return await _context.ChatMessages
                .Where(m => (m.SenderId == user1Id && m.ReceiverId == user2Id) ||
                            (m.SenderId == user2Id && m.ReceiverId == user1Id))
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<List<ChatSummaryDto>> GetChatSummariesAsync(int userId)
        {
            // Get all unique users this user has chatted with
            var senderIds = await _context.ChatMessages
                .Where(m => m.ReceiverId == userId)
                .Select(m => m.SenderId)
                .Distinct()
                .ToListAsync();

            var receiverIds = await _context.ChatMessages
                .Where(m => m.SenderId == userId)
                .Select(m => m.ReceiverId)
                .Distinct()
                .ToListAsync();

            var allOtherUserIds = senderIds.Union(receiverIds).Distinct().ToList();
            var summaries = new List<ChatSummaryDto>();

            foreach (var otherId in allOtherUserIds)
            {
                var otherUser = await _context.Users.FindAsync(otherId);
                if (otherUser == null) continue;

                var lastMessage = await _context.ChatMessages
                    .Where(m => (m.SenderId == userId && m.ReceiverId == otherId) ||
                                (m.SenderId == otherId && m.ReceiverId == userId))
                    .OrderByDescending(m => m.SentAt)
                    .FirstOrDefaultAsync();

                if (lastMessage == null) continue;

                var unreadCount = await _context.ChatMessages
                    .CountAsync(m => m.SenderId == otherId && m.ReceiverId == userId && !m.IsRead);

                summaries.Add(new ChatSummaryDto
                {
                    OtherUserId = otherId,
                    OtherUserName = otherUser.Name,
                    LastMessage = lastMessage.Content,
                    LastMessageTime = lastMessage.SentAt,
                    UnreadCount = unreadCount,
                    LastMessageIsFromMe = lastMessage.SenderId == userId,
                    LastMessageIsRead = lastMessage.IsRead
                });
            }

            return summaries.OrderByDescending(s => s.LastMessageTime).ToList();
        }

        public async Task MarkAsReadAsync(int receiverId, int senderId)
        {
            var unreadMessages = await _context.ChatMessages
                .Where(m => m.ReceiverId == receiverId && m.SenderId == senderId && !m.IsRead)
                .ToListAsync();

            foreach (var msg in unreadMessages)
            {
                msg.IsRead = true;
                msg.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}
