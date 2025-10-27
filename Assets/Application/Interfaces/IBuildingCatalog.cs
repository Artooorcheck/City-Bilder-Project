using System.Collections.Generic;
using CityBuilder.Domain;

namespace CityBuilder.Application.Interfaces
{
    public interface IBuildingCatalog
    {
        IEnumerable<BuildingTypeDefinition> All { get; }

        BuildingTypeDefinition GetById(string id);
    }
}
