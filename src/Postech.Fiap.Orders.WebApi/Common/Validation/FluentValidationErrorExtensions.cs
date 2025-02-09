using FluentValidation;
using Postech.Fiap.Orders.WebApi.Common.ResultPattern;

namespace Postech.Fiap.Orders.WebApi.Common.Validation;

[ExcludeFromCodeCoverage]
public static class FluentValidationErrorExtensions
{
    public static IRuleBuilderOptions<T, TProperty> WithError<T, TProperty>(this IRuleBuilderOptions<T, TProperty> rule,
        Error error)
    {
        if (error is null) throw new ArgumentNullException(nameof(error), "The error is required");

        return rule.WithErrorCode(error.Code).WithMessage(error.Message);
    }
}