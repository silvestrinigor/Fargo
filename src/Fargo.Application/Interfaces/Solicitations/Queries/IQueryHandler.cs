namespace Fargo.Application.Interfaces.Solicitations.Queries
{
    public interface IQueryHandler<TQuery, TReturn> where TQuery : IQuery<TQuery, TReturn>
    {
        TReturn Handle(TQuery query);
    }
}
