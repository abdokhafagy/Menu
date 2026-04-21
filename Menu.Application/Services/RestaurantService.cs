using AutoMapper;

using Menu.Application.DTOs.Restaurant;
using Menu.Application.Interfaces;
using Menu.Domain.Interfaces;

namespace Menu.Application.Services;

public class RestaurantService : CrudServiceBase<Menu.Domain.Entities.Restaurant, RestaurantDto, CreateRestaurantDto, UpdateRestaurantDto>, IRestaurantService
{
    public RestaurantService(IUnitOfWork unitOfWork, IMapper mapper)
        : base(unitOfWork, mapper, unitOfWork.Restaurants)
    {
    }
}
