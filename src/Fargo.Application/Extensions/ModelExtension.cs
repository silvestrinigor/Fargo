using Fargo.Application.Dtos;
using Fargo.Domain.Entities.Models.Abstracts;

namespace Fargo.Application.Extensions
{
    public static class ModelExtension
    {
        extension(Model model)
        {
            public ModelDto ToDto()
                => new(
                    model.Guid,
                    model.ModelType
                    );
        }
    }
}
