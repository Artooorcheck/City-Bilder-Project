using CityBuilder.Application.Interfaces;
using CityBuilder.Domain;

namespace CityBuilder.Infrastructure.Repositories
{
    public sealed class EconomyRepository : IEconomyRepository
    {
        private EconomyState _economy;

        public EconomyRepository(EconomyState economy)
        {
            _economy = economy;
        }

        public EconomyState Economy => _economy;

        public void Replace(EconomyState economyState)
        {
            _economy = economyState;
        }
    }
}
