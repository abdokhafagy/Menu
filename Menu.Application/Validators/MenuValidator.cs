using FluentValidation;

using Menu.Application.DTOs.Menu;

namespace Menu.Application.Validators;

public class MenuValidator : AbstractValidator<CreateMenuDto>
{
    public MenuValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RestaurantId).NotEmpty();
    }
}
