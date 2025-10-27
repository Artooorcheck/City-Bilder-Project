using System;
using System.Collections.Generic;
using CityBuilder.Domain;

namespace CityBuilder.Application.Interfaces
{
    public interface ICityRepository
    {
        CityState City { get; }

        Building GetBuilding(Guid id);

        IReadOnlyCollection<Building> GetAll();

        void Replace(CityState cityState);
    }
}
