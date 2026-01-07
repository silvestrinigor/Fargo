using Fargo.Application.Dtos;
using Fargo.Domain.Entities.Models.Abstracts;

namespace Fargo.Application.Extensions
{
    public static class ModelExtension
    {
        extension(Model model)
        {
            public ModelDto ToDto()
            {
                return new ModelDto(
                    model.Guid,
                    model.ModelType
                    );
            }
        }
    }
}
