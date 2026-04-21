using FluentValidation;

using Menu.Application.DTOs.Category;

namespace Menu.Application.Validators;

public class CategoryValidator : AbstractValidator<CreateCategoryDto>
{
    public CategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MenuId).NotEmpty();
    }
}
