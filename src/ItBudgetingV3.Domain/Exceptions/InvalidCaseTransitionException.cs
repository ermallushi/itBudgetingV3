namespace ItBudgetingV3.Domain.Exceptions;

public sealed class InvalidCaseTransitionException(string message) : InvalidOperationException(message)
{
}
