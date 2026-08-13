using UnitsNet;

namespace Fargo.Application.Shared.Articles;

public sealed record ArticleContainerDto(
    Mass? MaxMass = null
);
