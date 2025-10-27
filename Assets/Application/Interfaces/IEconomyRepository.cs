using CityBuilder.Domain;

namespace CityBuilder.Application.Interfaces
{
    public interface IEconomyRepository
    {
        EconomyState Economy { get; }

        void Replace(EconomyState economyState);
    }
}
