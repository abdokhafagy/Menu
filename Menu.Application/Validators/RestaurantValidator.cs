using FluentValidation;

using Menu.Application.DTOs.Restaurant;

namespace Menu.Application.Validators;

public class RestaurantValidator : AbstractValidator<CreateRestaurantDto>
{
    public RestaurantValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Phone).MaximumLength(20);
    }
}
