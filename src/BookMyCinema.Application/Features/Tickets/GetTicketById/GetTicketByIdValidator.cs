using FluentValidation;

namespace BookMyCinema.Application.Features.Tickets.GetTicketById;

internal class GetTicketByIdValidator : AbstractValidator<TicketDto>
{
    public GetTicketByIdValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithErrorCode(GetTicketByIdValidationErrors.TitleRequiredCode)
            .WithMessage(GetTicketByIdValidationErrors.TitleRequiredMessage);
    }
}
