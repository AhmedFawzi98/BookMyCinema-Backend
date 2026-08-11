using System.Net.NetworkInformation;

namespace BookMyCinema.Application.Features.Tickets.GetTicketById;

internal static class GetTicketByIdValidationErrors
{
    public const string TitleRequiredCode = "GetTicketById.Title.Required";
    public const string TitleRequiredMessage = "Title is required.";
}
