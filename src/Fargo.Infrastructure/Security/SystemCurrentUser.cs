using Fargo.Application.Security;

namespace Fargo.Infrastructure.Security
{
    public sealed class SystemCurrentUser
        : ICurrentUser
    {
        public bool IsAuthenticated
            => true;

        public Guid UserGuid
        {
            get
            {
                return Guid.Empty;
            }
        }
    }
}