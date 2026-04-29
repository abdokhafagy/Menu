using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

using Menu.Application.Common.Exceptions;
using Menu.Application.DTOs.Public;
using Menu.Application.Interfaces;
using Menu.Domain.Interfaces;

namespace Menu.Infrastructure.Services;

public class PublicMenuService : IPublicMenuService
{
    private static readonly TimeSpan RestaurantCacheDuration = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan MenuListCacheDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan MenuSummaryCacheDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan MenuDetailCacheDuration = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan NewItemWindow = TimeSpan.FromDays(14);

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;

    public PublicMenuService(IUnitOfWork unitOfWork, IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public Task<List<PublicRestaurantDto>> GetAllRestaurantsAsync(CancellationToken ct = default)
        => GetOrCreateAsync(
            "public:restaurants:all",
            RestaurantCacheDuration,
            async token => await _unitOfWork.Restaurants.Query()
                .AsNoTracking()
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .Select(r => new PublicRestaurantDto(
                    r.Id, r.Name, r.Slug, r.Description,
                    r.LogoUrl, r.Address, r.Phone))
                .ToListAsync(token),
            ct);

    public Task<PublicRestaurantDto> GetRestaurantBySlugAsync(string slug, CancellationToken ct = default)
    {
        var normalizedSlug = slug.Trim();

        return GetOrCreateAsync(
            $"public:restaurant:slug:{normalizedSlug.ToLowerInvariant()}",
            RestaurantCacheDuration,
            async token => await _unitOfWork.Restaurants.Query()
                .AsNoTracking()
                .Where(r => r.Slug == normalizedSlug && r.IsActive)
                .Select(r => new PublicRestaurantDto(
                    r.Id, r.Name, r.Slug, r.Description,
                    r.LogoUrl, r.Address, r.Phone))
                .FirstOrDefaultAsync(token)
                ?? throw new NotFoundException("Restaurant not found."),
            ct);
    }

    public Task<PublicRestaurantDto> GetRestaurantByIdAsync(Guid id, CancellationToken ct = default)
        => GetOrCreateAsync(
            $"public:restaurant:id:{id}",
            RestaurantCacheDuration,
            async token => await _unitOfWork.Restaurants.Query()
                .AsNoTracking()
                .Where(r => r.Id == id && r.IsActive)
                .Select(r => new PublicRestaurantDto(
                    r.Id, r.Name, r.Slug, r.Description,
                    r.LogoUrl, r.Address, r.Phone))
                .FirstOrDefaultAsync(token)
                ?? throw new NotFoundException("Restaurant not found."),
            ct);

    public Task<List<PublicMenuDto>> GetMenusByRestaurantAsync(Guid restaurantId, CancellationToken ct = default)
        => GetOrCreateAsync(
            $"public:restaurant:{restaurantId}:menus",
            MenuListCacheDuration,
            async token => await _unitOfWork.Menus.Query()
                .AsNoTracking()
                .Where(m => m.RestaurantId == restaurantId && m.IsActive && m.Restaurant.IsActive)
                .OrderBy(m => m.Name)
                .Select(m => new PublicMenuDto(m.Id, m.Name))
                .ToListAsync(token),
            ct);

    public Task<PublicMenuSummaryDto> GetRestaurantMenuAsync(Guid restaurantId, Guid? menuId = null, CancellationToken ct = default)
        => GetOrCreateAsync(
            $"public:restaurant:{restaurantId}:menu:{menuId?.ToString() ?? "default"}",
            MenuSummaryCacheDuration,
            async token =>
            {
                var selectedMenu = await _unitOfWork.Menus.Query()
                    .AsNoTracking()
                    .Where(m => m.RestaurantId == restaurantId && m.IsActive && m.Restaurant.IsActive)
                    .Where(m => !menuId.HasValue || m.Id == menuId.Value)
                    .OrderBy(m => m.Name)
                    .Select(m => new
                    {
                        m.Id,
                        m.Name
                    })
                    .FirstOrDefaultAsync(token)
                    ?? throw new NotFoundException(menuId.HasValue
                        ? "Menu not found."
                        : "No active menus available for this restaurant.");

                var categoryRows = await _unitOfWork.Categories.Query()
                    .AsNoTracking()
                    .Where(c => c.MenuId == selectedMenu.Id)
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.Name)
                    .Select(c => new
                    {
                        c.Id,
                        c.Name,
                        c.NameAr,
                        c.DisplayOrder
                    })
                    .ToListAsync(token);

                if (categoryRows.Count == 0)
                {
                    return new PublicMenuSummaryDto(selectedMenu.Id, selectedMenu.Name, new List<PublicCategoryMenuSummaryDto>());
                }

                var categoryIds = categoryRows.Select(c => c.Id).ToList();
                var newItemThreshold = DateTime.UtcNow.Subtract(NewItemWindow);

                var itemRows = await _unitOfWork.MenuItems.Query()
                    .AsNoTracking()
                    .Where(i => categoryIds.Contains(i.CategoryId) && i.IsAvailable)
                    .OrderBy(i => i.DisplayOrder)
                    .ThenBy(i => i.Name)
                    .Select(i => new
                    {
                        i.CategoryId,
                        i.Id,
                        i.Name,
                        i.NameAr,
                        i.Description,
                        i.DescriptionAr,
                        i.Price,
                        i.ImageUrl,
                        i.IsAvailable,
                        i.CreatedAt,
                        i.DisplayOrder
                    })
                    .ToListAsync(token);

                var itemsByCategory = itemRows
                    .GroupBy(i => i.CategoryId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(i => new PublicMenuItemSummaryDto(
                            i.Id,
                            i.Name,
                            i.NameAr,
                            i.Description,
                            i.DescriptionAr,
                            i.Price,
                            OptimizeImageUrl(i.ImageUrl, width: 480, quality: 75),
                            i.IsAvailable,
                            false,
                            i.CreatedAt >= newItemThreshold,
                            i.DisplayOrder))
                            .ToList());

                var categories = categoryRows
                    .Select(c => new PublicCategoryMenuSummaryDto(
                        c.Id,
                        c.Name,
                        c.NameAr,
                        c.DisplayOrder,
                        itemsByCategory.TryGetValue(c.Id, out var items)
                            ? items
                            : new List<PublicMenuItemSummaryDto>()))
                    .ToList();

                return new PublicMenuSummaryDto(selectedMenu.Id, selectedMenu.Name, categories);
            },
            ct);

    public Task<PublicMenuFullDto> GetFullMenuAsync(Guid menuId, bool includeOptions = false, CancellationToken ct = default)
        => GetOrCreateAsync(
            $"public:menu:full:{menuId}:{includeOptions}",
            MenuSummaryCacheDuration,
            async token =>
            {
                var query = _unitOfWork.Menus.Query()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(m => m.Restaurant)
                    .Include(m => m.Categories.Where(c => !c.IsDeleted))
                        .ThenInclude(c => c.Items.Where(i => !i.IsDeleted && i.IsAvailable))
                    .Where(m => m.Id == menuId && m.IsActive && m.Restaurant.IsActive);

                if (includeOptions)
                {
                    query = query
                        .Include(m => m.Categories.Where(c => !c.IsDeleted))
                            .ThenInclude(c => c.Items.Where(i => !i.IsDeleted && i.IsAvailable))
                                .ThenInclude(i => i.Options.Where(o => !o.IsDeleted))
                                    .ThenInclude(o => o.Values.Where(v => !v.IsDeleted));
                }

                var menu = await query
                    .FirstOrDefaultAsync(token)
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
                                i.Price, OptimizeImageUrl(i.ImageUrl, width: 480, quality: 75), i.IsAvailable,
                                i.DisplayOrder,
                                includeOptions
                                    ? i.Options
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
                                        .ToList()
                                    : null))
                            .ToList()))
                    .ToList();

                return new PublicMenuFullDto(menu.Id, menu.Name, restaurant, categories);
            },
            ct);

    public Task<PublicMenuItemDetailDto> GetItemDetailAsync(Guid itemId, CancellationToken ct = default)
        => GetOrCreateAsync(
            $"public:menu-item:{itemId}:detail",
            MenuDetailCacheDuration,
            async token =>
            {
                var itemRow = await _unitOfWork.MenuItems.Query()
                    .AsNoTracking()
                    .Where(i => i.Id == itemId &&
                                i.IsAvailable &&
                                i.Category.Menu.IsActive &&
                                i.Category.Menu.Restaurant.IsActive)
                    .Select(i => new
                    {
                        i.Id,
                        i.Name,
                        i.NameAr,
                        i.Description,
                        i.DescriptionAr,
                        i.Price,
                        i.ImageUrl,
                        i.IsAvailable
                    })
                    .FirstOrDefaultAsync(token)
                    ?? throw new NotFoundException("Menu item not found.");

                var optionRows = await _unitOfWork.ItemOptions.Query()
                    .AsNoTracking()
                    .Where(o => o.MenuItemId == itemId)
                    .OrderBy(o => o.Name)
                    .Select(o => new
                    {
                        o.Id,
                        o.Name,
                        o.NameAr,
                        o.IsRequired,
                        o.MinSelections,
                        o.MaxSelections,
                        o.SelectionType
                    })
                    .ToListAsync(token);

                var optionIds = optionRows.Select(o => o.Id).ToList();
                List<(Guid ItemOptionId, PublicOptionValueDto Value)> valueRows;
                if (optionIds.Count == 0)
                {
                    valueRows = new List<(Guid ItemOptionId, PublicOptionValueDto Value)>();
                }
                else
                {
                    var rawValueRows = await _unitOfWork.OptionValues.Query()
                        .AsNoTracking()
                        .Where(v => optionIds.Contains(v.ItemOptionId))
                        .OrderBy(v => v.DisplayOrder)
                        .ThenBy(v => v.Value)
                        .Select(v => new
                        {
                            v.ItemOptionId,
                            Value = new PublicOptionValueDto(
                                v.Id,
                                v.Value,
                                v.ValueAr,
                                v.PriceModifier,
                                v.IsDefault,
                                v.DisplayOrder)
                        })
                        .ToListAsync(token);

                    valueRows = rawValueRows
                        .Select(v => (v.ItemOptionId, v.Value))
                        .ToList();
                }

                var valuesByOptionId = valueRows
                    .GroupBy(v => v.ItemOptionId)
                    .ToDictionary(g => g.Key, g => g.Select(v => v.Value).ToList());

                var options = optionRows
                    .Select(o => new PublicItemOptionDto(
                        o.Id,
                        o.Name,
                        o.NameAr,
                        o.IsRequired,
                        o.MinSelections,
                        o.MaxSelections,
                        o.SelectionType.ToString(),
                        valuesByOptionId.TryGetValue(o.Id, out var values)
                            ? values
                            : new List<PublicOptionValueDto>()))
                    .ToList();

                var images = await _unitOfWork.ItemImages.Query()
                    .AsNoTracking()
                    .Where(img => img.MenuItemId == itemId)
                    .OrderBy(img => img.DisplayOrder)
                    .Select(img => img.ImageUrl)
                    .ToListAsync(token);

                return new PublicMenuItemDetailDto(
                    itemRow.Id,
                    itemRow.Name,
                    itemRow.NameAr,
                    itemRow.Description,
                    itemRow.DescriptionAr,
                    itemRow.Price,
                    OptimizeImageUrl(itemRow.ImageUrl, width: 960, quality: 85),
                    itemRow.IsAvailable,
                    options,
                    images.Select(url => OptimizeImageUrl(url, width: 1280, quality: 85) ?? url).ToList());
            },
            ct);

    public async Task<List<PublicMenuItemDto>> SearchItemsAsync(string query, Guid? restaurantId, CancellationToken ct = default)
    {
        var normalizedQuery = query.Trim();
        var pattern = $"%{normalizedQuery}%";

        var itemsQuery = _unitOfWork.MenuItems.Query()
            .AsNoTracking()
            .Where(i => i.IsAvailable &&
                (EF.Functions.Like(i.Name, pattern) ||
                 (i.NameAr != null && EF.Functions.Like(i.NameAr, pattern)) ||
                 (i.Description != null && EF.Functions.Like(i.Description, pattern)) ||
                 (i.DescriptionAr != null && EF.Functions.Like(i.DescriptionAr, pattern))));

        if (restaurantId.HasValue)
        {
            itemsQuery = itemsQuery.Where(i =>
                i.Category.Menu.RestaurantId == restaurantId.Value &&
                i.Category.Menu.IsActive &&
                i.Category.Menu.Restaurant.IsActive);
        }

        var items = await itemsQuery
            .OrderBy(i => i.Name)
            .Take(20)
            .Select(i => new
            {
                i.Id,
                i.Name,
                i.NameAr,
                i.Description,
                i.DescriptionAr,
                i.Price,
                i.ImageUrl,
                i.IsAvailable,
                i.DisplayOrder
            })
            .ToListAsync(ct);

        return items
            .Select(i => new PublicMenuItemDto(
                i.Id,
                i.Name,
                i.NameAr,
                i.Description,
                i.DescriptionAr,
                i.Price,
                OptimizeImageUrl(i.ImageUrl, width: 480, quality: 75),
                i.IsAvailable,
                i.DisplayOrder,
                null))
            .ToList();
    }

    private Task<T> GetOrCreateAsync<T>(
        string cacheKey,
        TimeSpan cacheDuration,
        Func<CancellationToken, Task<T>> factory,
        CancellationToken ct)
        => _cache.GetOrCreateAsync(
                cacheKey,
                async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = cacheDuration;
                    return await factory(ct);
                })!;

    private static string? OptimizeImageUrl(string? imageUrl, int width, int quality)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return imageUrl;
        }

        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var absoluteUri))
        {
            return imageUrl;
        }

        if (!absoluteUri.Host.Contains("unsplash.com", StringComparison.OrdinalIgnoreCase))
        {
            return imageUrl;
        }

        return $"{absoluteUri.GetLeftPart(UriPartial.Path)}?auto=format&fit=crop&w={width}&q={quality}";
    }
}
