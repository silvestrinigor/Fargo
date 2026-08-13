using UnitsNet;

namespace Fargo.Application.Articles;

public sealed record ArticleContainerDto(
    Mass? MaxMass = null
);
