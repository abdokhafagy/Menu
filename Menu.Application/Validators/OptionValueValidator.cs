using FluentValidation;

using Menu.Application.DTOs.OptionValue;

namespace Menu.Application.Validators;

public class OptionValueValidator : AbstractValidator<CreateOptionValueDto>
{
    public OptionValueValidator()
    {
        RuleFor(x => x.Value).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PriceModifier).GreaterThanOrEqualTo(0);
    }
}
