using FluentValidation;

using Menu.Application.DTOs.Auth;

namespace Menu.Application.Validators.Auth;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]")
            .Matches("[0-9]")
            .Matches("[^a-zA-Z0-9]");
        RuleFor(x => x.ConfirmPassword).Equal(x => x.Password);
        RuleFor(x => x.RestaurantId).NotEmpty();
    }
}
