using Fargo.Application.Security;
using Microsoft.AspNetCore.Http;

namespace Fargo.Infrastructure.Security
{
    public sealed class CurrentUser(
            IHttpContextAccessor httpContextAccessor
            ) : ICurrentUser
    {
        public Guid UserGuid => throw new NotImplementedException();

        public bool IsAuthenticated => throw new NotImplementedException();
    }
}