using FluentValidation;

using Menu.Application.DTOs.ItemOption;

namespace Menu.Application.Validators;

public class ItemOptionValidator : AbstractValidator<CreateItemOptionDto>
{
    public ItemOptionValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MinSelections).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxSelections).GreaterThanOrEqualTo(x => x.MinSelections);
    }
}
