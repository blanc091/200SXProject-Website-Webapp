using _200SXContact.Interfaces;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Interfaces.Areas.Data;
using _200SXContact.Models.Areas.MaintenApp;
using _200SXContact.Models.Areas.UserContent;
using _200SXContact.Models.DTOs.Areas.MaintenApp;
using static _200SXContact.Commands.Areas.MaintenApp.CreateEntryCommandHandler;

namespace _200SXContact.Commands.Areas.MaintenApp
{
    public class CreateEntryCommand(ClaimsPrincipal user, ReminderItemDto entryDto) : IRequest<CreateEntryCommandResult>
    {
        public ClaimsPrincipal User { get; } = user;
        public ReminderItemDto EntryDto { get; } = entryDto;
    }
    public class CreateEntryCommandHandler : IRequestHandler<CreateEntryCommand, CreateEntryCommandResult>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILoggerService _loggerService;
        private readonly IClientTimeProvider _clientTimeProvider;
        public CreateEntryCommandHandler(IClientTimeProvider clientTimeProvider, IApplicationDbContext context, IMapper mapper, ILoggerService loggerService)
        {
            _context = context;
            _mapper = mapper;
            _loggerService = loggerService;
            _clientTimeProvider = clientTimeProvider;
        }
        public async Task<CreateEntryCommandResult> Handle(CreateEntryCommand request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("MaintenApp || Started adding entry in MaintenApp dash view", "Info", "");

            CreateEntryCommandValidator validator = new CreateEntryCommandValidator();
            ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                CreateEntryCommandResult result = new CreateEntryCommandResult { Succeeded = false };
                result.Errors = validationResult.Errors.GroupBy(e => e.PropertyName).ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                await _loggerService.LogAsync("MaintenApp || Validation errors: " + string.Join("; ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")), "Error", "");

                return result;
            }

            string? userEmail = request.User.FindFirstValue(ClaimTypes.Email);
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail, cancellationToken);

            if (user == null)
            {
                await _loggerService.LogAsync("MaintenApp || User is null when creating entry in MaintenApp dash view", "Error", "");

                return new CreateEntryCommandResult
                {
                    Succeeded = false,
                    Errors = new Dictionary<string, string[]> {{ "User", new[] { "User not found." } }}
                };
            }

            ReminderItem newItem = _mapper.Map<ReminderItem>(request.EntryDto);
            newItem.DueDate = request.EntryDto.DueDate;
            DateTime clientTime = _clientTimeProvider.GetCurrentClientTime();

            newItem.CreatedAt = clientTime;
            newItem.UpdatedAt = clientTime;
            newItem.UserId = user.Id;

            await _context.Items.AddAsync(newItem, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await _loggerService.LogAsync("MaintenApp || Finished adding entry in MaintenApp dash view", "Info", "");

            return new CreateEntryCommandResult
            {
                Succeeded = true,
                Errors = new Dictionary<string, string[]>()
            };
        }
        public class CreateEntryCommandValidator : AbstractValidator<CreateEntryCommand>
        {
            public CreateEntryCommandValidator()
            {
                RuleFor(x => x.EntryDto).NotNull().WithMessage("Entry data is required !").SetValidator(new EntryDtoValidator());
            }
        }
        public class EntryDtoValidator : AbstractValidator<ReminderItemDto>
        {
            public EntryDtoValidator()
            {
                RuleFor(x => x.EntryItem).NotEmpty().WithMessage("Entry item is required !").MaximumLength(100).WithMessage("Entry item cannot exceed 100 characters !");

                RuleFor(x => x.EntryDescription).NotEmpty().WithMessage("Entry description is required !").MaximumLength(1000).WithMessage("Entry description cannot exceed 1000 characters !");

                RuleFor(x => x.DueDate).NotEqual(default(DateTime)).WithMessage("A valid due date is required !");
            }
        }
        public class CreateEntryCommandResult
        {
            public bool Succeeded { get; set; }
            public IDictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();
        }
    }   
}
