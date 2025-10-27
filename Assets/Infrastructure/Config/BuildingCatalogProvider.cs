using System.Collections.Generic;
using System.Linq;
using CityBuilder.Application.Interfaces;
using CityBuilder.Domain;

namespace CityBuilder.Infrastructure.Config
{
    public sealed class BuildingCatalogProvider : IBuildingCatalog
    {
        private readonly Dictionary<string, BuildingTypeDefinition> _definitions;

        public BuildingCatalogProvider(BuildingCatalogConfig config)
        {
            _definitions = config.BuildDefinitions().ToDictionary(def => def.Id);
        }

        public IEnumerable<BuildingTypeDefinition> All => _definitions.Values;

        public BuildingTypeDefinition GetById(string id)
        {
            if (!_definitions.TryGetValue(id, out var definition))
            {
                throw new DomainException($"Building type '{id}' not found in catalog.");
            }

            return definition;
        }
    }
}
