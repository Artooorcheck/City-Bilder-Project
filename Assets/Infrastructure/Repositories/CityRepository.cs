using System;
using System.Collections.Generic;
using CityBuilder.Application.Interfaces;
using CityBuilder.Domain;

namespace CityBuilder.Infrastructure.Repositories
{
    public sealed class CityRepository : ICityRepository
    {
        private CityState _city;

        public CityRepository(CityState city)
        {
            _city = city;
        }

        public CityState City => _city;

        public Building GetBuilding(Guid id) => _city.GetBuilding(id);

        public IReadOnlyCollection<Building> GetAll() => _city.Buildings;

        public void Replace(CityState cityState)
        {
            _city = cityState;
        }
    }
}
