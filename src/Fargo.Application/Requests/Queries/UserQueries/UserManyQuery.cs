using Fargo.Application.Commom;
using Fargo.Application.Extensions;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Repositories;

namespace Fargo.Application.Requests.Queries.UserQueries
{
    public sealed record UserManyQuery(
            DateTime? TemporalAsOf = null,
            Pagination Pagination = default
            ) : IQuery<IEnumerable<UserResponseModel>>;

    public sealed class UserManyQueryHandler(
            IUserReadRepository repository
            ) : IQueryHandler<UserManyQuery, IEnumerable<UserResponseModel>>
    {
        public async Task<IEnumerable<UserResponseModel>> Handle(
                UserManyQuery query,
                CancellationToken cancellationToken = default
                )
        {
            var users = await repository.GetMany(
                    query.TemporalAsOf,
                    query.Pagination,
                    cancellationToken
                    );

            return [.. users.Select(u => u.ToResponse())];
        }
    }
}