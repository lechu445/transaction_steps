using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Anixe.TransactionSteps
{
  /// <summary>
  /// Defines an iterator for processing a sequence of steps, with support for asynchronous and synchronous execution.
  /// </summary>
  /// <typeparam name="T">The type of property bag used to store state information during iteration.</typeparam>
  public interface IStepIterator<T>
    where T : IPropertyBag
  {
    List<StepStat> Stats { get; }

    Task<T> IterateAllAsync(
      IServiceProvider? services,
      LinkedList<IStep> steps,
      IStep? errorHandler,
      CancellationToken token);

    void IterateAll(
      IServiceProvider? services,
      LinkedList<IStep> steps,
      IStep errorHandler);
  }
}
