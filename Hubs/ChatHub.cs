using _200SXContact.Data;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Models.Areas.Chat;
using _200SXContact.Models.DTOs.Areas.Chat;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace _200SXContact.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        public static Dictionary<string, string?> ActiveSessions = new Dictionary<string, string?>();
        public ChatHub(ApplicationDbContext context, IMapper mapper, IEmailService emailService)
        {
            _context = context;
            _mapper = mapper;
            _emailService = emailService;
        }
        public override async Task OnConnectedAsync()
        {
            string connectionId = Context.ConnectionId;
            // Log connection for debugging
            var userName = Context.User?.Identity?.Name ?? "Unknown";
            Debug.WriteLine($"Connection {connectionId}: Authenticated user: {userName}");

            // Ensure the user is authenticated
            if (string.IsNullOrEmpty(userName))
            {
                // Optionally, disconnect or throw an error
                throw new HubException("Unauthenticated users are not allowed to connect.");
            }

            // Add connection to a group using the persistent identifier
            await Groups.AddToGroupAsync(connectionId, userName);

            // For admin role, add to AdminGroup if needed
            if (Context.User.IsInRole("Admin"))
            {
                await Groups.AddToGroupAsync(connectionId, "AdminGroup");
            }

            // Create or update the chat session in the database using the authenticated user's ID
            var existingSession = await _context.ChatSessions.FirstOrDefaultAsync(cs => cs.UserId == userName);
            if (existingSession == null)
            {
                var newSession = new ChatSession
                {
                    SessionId = userName,
                    ConnectionId = connectionId,
                    UserId = userName,
                    UserName = null, // or set to userName if you prefer
                    IsAnswered = false,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ChatSessions.Add(newSession);
            }
            else
            {
                existingSession.ConnectionId = connectionId;
                existingSession.LastUpdatedAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();

            // Optionally, send notifications to admin group
            await _emailService.NotifyNewChatSessionAsync(connectionId);
            await Clients.Group("AdminGroup").SendAsync("NewChatSession", connectionId);

            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string sessionId = Context.ConnectionId;

            if (ActiveSessions.ContainsKey(sessionId))
            {
                ActiveSessions.Remove(sessionId);
            }

            await base.OnDisconnectedAsync(exception);
        }
        public async Task SetUserName(string userName)
        {
            string sessionId = Context.ConnectionId;
            ActiveSessions[sessionId] = userName;
            ChatSession? session = await _context.ChatSessions.FirstOrDefaultAsync(s => s.SessionId == sessionId);
            
            if (session != null)
            {
                session.UserName = userName;
                session.LastUpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            await Clients.Group("AdminGroup").SendAsync("UpdateChatSession", sessionId, userName);
        }
        public async Task SendMessage(ChatMessageDto chatMessageDto)
        {
            string persistentId = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(persistentId))
            {
                throw new HubException("Unauthenticated users are not allowed to send messages.");
            }
            chatMessageDto.SessionId = persistentId;
            chatMessageDto.UserName = persistentId;
            ChatMessage chatMessage = _mapper.Map<ChatMessage>(chatMessageDto);
            chatMessage.SentAt = DateTime.UtcNow;
            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            await Clients.Group(persistentId).SendAsync("ReceiveMessage", _mapper.Map<ChatMessageDto>(chatMessage));
        }
        public async Task AdminSendMessageToSession(string sessionId, string message)
        {
            ChatMessageDto chatMessageDto = new ChatMessageDto
            {
                UserName = "Admin",
                Message = message,
                SentAt = DateTime.UtcNow
            };
            ChatMessage chatMessage = _mapper.Map<ChatMessage>(chatMessageDto);
            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();
            ChatMessageDto savedDto = _mapper.Map<ChatMessageDto>(chatMessage);
            await Clients.Group(sessionId).SendAsync("ReceiveMessage", savedDto);
        }
        public async Task JoinAdminGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "AdminGroup");
        }
        public async Task JoinChatSession(string sessionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
        }
        public async Task LoadPreviousMessages()
        {
            string userId = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("Unauthenticated users are not allowed.");
            }

            var session = await _context.ChatSessions.FirstOrDefaultAsync(cs => cs.SessionId == userId);
            if (session != null)
            {
                List<ChatMessage> messages = await _context.ChatMessages
                    .Where(m => m.SessionId == session.SessionId)
                    .OrderBy(m => m.SentAt)
                    .ToListAsync();

                List<ChatMessageDto> messageDtos = _mapper.Map<List<ChatMessageDto>>(messages);
                await Clients.Caller.SendAsync("LoadMessages", messageDtos);
            }
            else
            {
                await Clients.Caller.SendAsync("LoadMessages", new List<ChatMessageDto>());
            }
        }

    }
}
