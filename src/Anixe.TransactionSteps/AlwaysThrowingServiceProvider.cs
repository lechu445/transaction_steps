using System;

namespace Anixe.TransactionSteps;

internal sealed class AlwaysThrowingServiceProvider : IServiceProvider
{
  public static readonly AlwaysThrowingServiceProvider Instance = new();

  private AlwaysThrowingServiceProvider() { }

  public object? GetService(Type serviceType) => throw new InvalidOperationException("There is no service provider provided. Provide it as parameter into IterateAll(Async).");
}
