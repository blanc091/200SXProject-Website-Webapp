using _200SXContact.Helpers;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Interfaces.Areas.Data;
using _200SXContact.Models.Areas.Chat;
using _200SXContact.Models.DTOs.Areas.Chat;
using _200SXContact.Queries.Areas.Admin;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace _200SXContact.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public static Dictionary<string, string?> ActiveSessions = new Dictionary<string, string?>();
        public ChatHub(IHttpContextAccessor httpContextAccessor, IApplicationDbContext context, IMapper mapper, IEmailService emailService)
        {
            _context = context;
            _mapper = mapper;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
        }
        public override async Task OnConnectedAsync()
        {
            string connectionId = Context.ConnectionId;
            var userName = Context.User?.Identity?.Name ?? "Unknown";
            Debug.WriteLine($"Connection {connectionId}: Authenticated user: {userName}");

            if (string.IsNullOrEmpty(userName))
            {
                throw new HubException("Unauthenticated users are not allowed to connect.");
            }

            await Groups.AddToGroupAsync(connectionId, userName);

            if (Context.User.IsInRole("Admin"))
            {
                await Groups.AddToGroupAsync(connectionId, "AdminGroup");
            }
            DateTime clientTime = ClientTimeHelper.GetCurrentClientTime(_httpContextAccessor);
            var existingSession = await _context.ChatSessions.FirstOrDefaultAsync(cs => cs.UserId == userName);
            if (existingSession == null)
            {  
                ChatSession newSession = new ChatSession
                {
                    SessionId = userName,
                    ConnectionId = connectionId,
                    UserId = userName,
                    UserName = null,
                    IsAnswered = false,
                    CreatedAt = clientTime
                };
                _context.ChatSessions.Add(newSession);
            }
            else
            {
                existingSession.ConnectionId = connectionId;
                existingSession.LastUpdatedAt = clientTime;
            }
            await _context.SaveChangesAsync();

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
            DateTime clientTime = ClientTimeHelper.GetCurrentClientTime(_httpContextAccessor);

            if (session != null)
            {
                session.UserName = userName;
                session.LastUpdatedAt = clientTime;
                await _context.SaveChangesAsync();
            }

            await Clients.Group("AdminGroup").SendAsync("UpdateChatSession", sessionId, userName);
        }
        public async Task SendMessage(ChatMessageDto chatMessageDto)
        {
            DateTime clientTime = ClientTimeHelper.GetCurrentClientTime(_httpContextAccessor);
            string persistentId = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(persistentId))
            {
                throw new HubException("Unauthenticated users are not allowed to send messages.");
            }
            chatMessageDto.SessionId = persistentId;
            chatMessageDto.UserName = persistentId;
            ChatMessage chatMessage = _mapper.Map<ChatMessage>(chatMessageDto);
            chatMessage.SentAt = clientTime;
            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            await Clients.Group(persistentId).SendAsync("ReceiveMessage", _mapper.Map<ChatMessageDto>(chatMessage));
        }
        public async Task AdminSendMessageToSession(string sessionId, string message)
        {
            DateTime clientTime = ClientTimeHelper.GetCurrentClientTime(_httpContextAccessor);

            ChatMessageDto chatMessageDto = new ChatMessageDto
            {
                UserName = "Admin",
                Message = message,
                SentAt = clientTime
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
