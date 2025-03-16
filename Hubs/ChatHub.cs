using _200SXContact.Data;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Models.Areas.Chat;
using _200SXContact.Models.DTOs.Areas.Chat;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

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
            string sessionId = Context.ConnectionId;
            ActiveSessions[sessionId] = null;
            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
            await _emailService.NotifyNewChatSessionAsync(sessionId);
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

            if (ActiveSessions.ContainsKey(sessionId))
            {
                ActiveSessions[sessionId] = userName;
            }
        }
        public async Task SendMessage(ChatMessageDto chatMessageDto)
        {
            ChatMessage chatMessage = _mapper.Map<ChatMessage>(chatMessageDto);
            chatMessage.SentAt = DateTime.UtcNow;
            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();
            ChatMessageDto savedDto = _mapper.Map<ChatMessageDto>(chatMessage);
            string sessionId = Context.ConnectionId;
            await Clients.Group(sessionId).SendAsync("ReceiveMessage", savedDto);
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
            List<ChatMessage> messages = await _context.ChatMessages
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            List<ChatMessageDto> messageDtos = _mapper.Map<List<ChatMessageDto>>(messages);
            await Clients.Caller.SendAsync("LoadMessages", messageDtos);
        }
    }
}
