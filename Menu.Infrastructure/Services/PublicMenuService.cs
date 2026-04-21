using Microsoft.EntityFrameworkCore;

using Menu.Application.Common.Exceptions;
using Menu.Application.DTOs.Public;
using Menu.Application.Interfaces;
using Menu.Domain.Interfaces;

namespace Menu.Infrastructure.Services;

public class PublicMenuService : IPublicMenuService
{
    private readonly IUnitOfWork _unitOfWork;

    public PublicMenuService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<PublicRestaurantDto>> GetAllRestaurantsAsync(CancellationToken ct = default)
    {
        var restaurants = await _unitOfWork.Restaurants.Query()
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .Select(r => new PublicRestaurantDto(
                r.Id, r.Name, r.Slug, r.Description,
                r.LogoUrl, r.Address, r.Phone))
            .ToListAsync(ct);

        return restaurants;
    }

    public async Task<PublicRestaurantDto> GetRestaurantBySlugAsync(string slug, CancellationToken ct = default)
    {
        var restaurant = await _unitOfWork.Restaurants.Query()
            .Where(r => r.Slug == slug && r.IsActive)
            .Select(r => new PublicRestaurantDto(
                r.Id, r.Name, r.Slug, r.Description,
                r.LogoUrl, r.Address, r.Phone))
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException("Restaurant not found.");

        return restaurant;
    }

    public async Task<PublicRestaurantDto> GetRestaurantByIdAsync(Guid id, CancellationToken ct = default)
    {
        var restaurant = await _unitOfWork.Restaurants.Query()
            .Where(r => r.Id == id && r.IsActive)
            .Select(r => new PublicRestaurantDto(
                r.Id, r.Name, r.Slug, r.Description,
                r.LogoUrl, r.Address, r.Phone))
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException("Restaurant not found.");

        return restaurant;
    }

    public async Task<List<PublicMenuDto>> GetMenusByRestaurantAsync(Guid restaurantId, CancellationToken ct = default)
    {
        var menus = await _unitOfWork.Menus.Query()
            .Where(m => m.RestaurantId == restaurantId && m.IsActive)
            .OrderBy(m => m.Name)
            .Select(m => new PublicMenuDto(m.Id, m.Name))
            .ToListAsync(ct);

        return menus;
    }

    public async Task<PublicMenuFullDto> GetFullMenuAsync(Guid menuId, bool includeOptions = true, CancellationToken ct = default)
    {
        var query = _unitOfWork.Menus.Query()
            .Include(m => m.Restaurant)
            .Include(m => m.Categories.Where(c => !c.IsDeleted))
                .ThenInclude(c => c.Items.Where(i => !i.IsDeleted && i.IsAvailable))
            .Where(m => m.Id == menuId && m.IsActive);

        if (includeOptions)
        {
            query = query
                .Include(m => m.Categories.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.Items.Where(i => !i.IsDeleted && i.IsAvailable))
                        .ThenInclude(i => i.Options.Where(o => !o.IsDeleted))
                            .ThenInclude(o => o.Values.Where(v => !v.IsDeleted));
        }

        var menu = await query
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException("Menu not found.");

        var restaurant = new PublicRestaurantDto(
            menu.Restaurant.Id, menu.Restaurant.Name, menu.Restaurant.Slug,
            menu.Restaurant.Description, menu.Restaurant.LogoUrl,
            menu.Restaurant.Address, menu.Restaurant.Phone);

        var categories = menu.Categories
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new PublicCategoryWithItemsDto(
                c.Id, c.Name, c.NameAr, c.DisplayOrder,
                c.Items
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => new PublicMenuItemDto(
                        i.Id, i.Name, i.NameAr,
                        i.Description, i.DescriptionAr,
                        i.Price, i.ImageUrl, i.IsAvailable,
                        i.DisplayOrder,
                        includeOptions ? i.Options
                            .OrderBy(o => o.Name)
                            .Select(o => new PublicItemOptionDto(
                                o.Id, o.Name, o.NameAr,
                                o.IsRequired, o.MinSelections, o.MaxSelections,
                                o.SelectionType.ToString(),
                                o.Values
                                    .OrderBy(v => v.DisplayOrder)
                                    .Select(v => new PublicOptionValueDto(
                                        v.Id, v.Value, v.ValueAr,
                                        v.PriceModifier, v.IsDefault, v.DisplayOrder))
                                    .ToList()))
                            .ToList() : null))
                    .ToList()))
            .ToList();

        return new PublicMenuFullDto(menu.Id, menu.Name, restaurant, categories);
    }

    public async Task<PublicMenuItemDetailDto> GetItemDetailAsync(Guid itemId, CancellationToken ct = default)
    {
        var item = await _unitOfWork.MenuItems.Query()
            .Include(i => i.Options.Where(o => !o.IsDeleted))
                .ThenInclude(o => o.Values.Where(v => !v.IsDeleted))
            .Include(i => i.Images.Where(img => !img.IsDeleted))
            .Where(i => i.Id == itemId && i.IsAvailable)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException("Menu item not found.");

        var options = item.Options
            .OrderBy(o => o.Name)
            .Select(o => new PublicItemOptionDto(
                o.Id, o.Name, o.NameAr,
                o.IsRequired, o.MinSelections, o.MaxSelections,
                o.SelectionType.ToString(),
                o.Values
                    .OrderBy(v => v.DisplayOrder)
                    .Select(v => new PublicOptionValueDto(
                        v.Id, v.Value, v.ValueAr,
                        v.PriceModifier, v.IsDefault, v.DisplayOrder))
                    .ToList()))
            .ToList();

        var images = item.Images
            .OrderBy(img => img.DisplayOrder)
            .Select(img => img.ImageUrl)
            .ToList();

        return new PublicMenuItemDetailDto(
            item.Id, item.Name, item.NameAr,
            item.Description, item.DescriptionAr,
            item.Price, item.ImageUrl, item.IsAvailable,
            options, images);
    }

    public async Task<List<PublicMenuItemDto>> SearchItemsAsync(string query, Guid? restaurantId, CancellationToken ct = default)
    {
        var lowerQuery = query.ToLower();

        var itemsQuery = _unitOfWork.MenuItems.Query()
            .Include(i => i.Category)
                .ThenInclude(c => c.Menu)
            .Where(i => i.IsAvailable &&
                (i.Name.ToLower().Contains(lowerQuery) ||
                 (i.NameAr != null && i.NameAr.Contains(query)) ||
                 (i.Description != null && i.Description.ToLower().Contains(lowerQuery))));

        if (restaurantId.HasValue)
        {
            itemsQuery = itemsQuery.Where(i => i.Category.Menu.RestaurantId == restaurantId.Value);
        }

        var items = await itemsQuery
            .OrderBy(i => i.Name)
            .Take(20)
            .Select(i => new PublicMenuItemDto(
                i.Id, i.Name, i.NameAr,
                i.Description, i.DescriptionAr,
                i.Price, i.ImageUrl, i.IsAvailable,
                i.DisplayOrder,
                null))
            .ToListAsync(ct);

        return items;
    }
}
