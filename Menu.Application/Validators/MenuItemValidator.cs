using FluentValidation;

using Menu.Application.DTOs.MenuItem;

namespace Menu.Application.Validators;

public class MenuItemValidator : AbstractValidator<CreateMenuItemDto>
{
    public MenuItemValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}
