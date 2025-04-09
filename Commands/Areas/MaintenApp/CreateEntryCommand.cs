using _200SXContact.Interfaces;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Interfaces.Areas.Data;
using _200SXContact.Models.Areas.MaintenApp;
using _200SXContact.Models.Areas.UserContent;
using _200SXContact.Models.DTOs.Areas.MaintenApp;

namespace _200SXContact.Commands.Areas.MaintenApp
{
    public class CreateEntryCommand(ClaimsPrincipal user, ReminderItemDto entryDto) : IRequest<CreateEntryResult>
    {
        public ClaimsPrincipal User { get; } = user;
        public ReminderItemDto EntryDto { get; } = entryDto;
    }
    public class CreateEntryCommandHandler : IRequestHandler<CreateEntryCommand, CreateEntryResult>
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
        public async Task<CreateEntryResult> Handle(CreateEntryCommand request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("MaintenApp || Started adding entry in MaintenApp dash view", "Info", "");

            CreateEntryCommandValidator validator = new CreateEntryCommandValidator();
            ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                if (validationResult.Errors.Any(e => e.PropertyName.Contains("DueDate")))
                {
                    await _loggerService.LogAsync("MaintenApp || Due date is invalid when creating entry", "Error", "");

                    return CreateEntryResult.InvalidDueDate;
                }

                await _loggerService.LogAsync("MaintenApp || Validation error(s): " + string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)), "Error", "");

                return CreateEntryResult.InvalidDueDate;
            }

            string? userEmail = request.User.FindFirstValue(ClaimTypes.Email);
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail, cancellationToken);

            if (user == null)
            {
                await _loggerService.LogAsync("MaintenApp || User is null when creating entry in MaintenApp dash view", "Error", "");

                return CreateEntryResult.UserNotFound;
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

            return CreateEntryResult.Success;
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
    }
    public enum CreateEntryResult
    {
        Success,
        UserNotFound,
        InvalidDueDate
    }
}
