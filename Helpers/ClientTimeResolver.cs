using _200SXContact.Interfaces;

namespace _200SXContact.Helpers
{
    public class ClientTimeResolver<TSource, TDestination> : IValueResolver<TSource, TDestination, DateTime>
    {
        private readonly IClientTimeProvider _clientTimeProvider;
        public ClientTimeResolver(IClientTimeProvider clientTimeProvider)
        {
            _clientTimeProvider = clientTimeProvider;
        }
        public DateTime Resolve(TSource source, TDestination destination, DateTime destMember, ResolutionContext context)
        {
            return _clientTimeProvider.GetCurrentClientTime();
        }
    }
}
