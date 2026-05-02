using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Anixe.TransactionSteps
{
  /// <summary>
  /// Represents a step in a processing sequence, providing properties and methods to manage and execute the step.
  /// </summary>
  /// <remarks>The <see cref="IStep"/> interface defines the structure for a processing step, including its
  /// execution state, timing, and neighborhood relationships with other steps. It supports both synchronous and
  /// asynchronous processing.</remarks>
  public interface IStep
  {
    /// <summary>
    /// Gets or sets the service provider that is used to resolve service dependencies.
    /// </summary>
    IServiceProvider Services { get; set; }

    /// <summary>
    /// Gets or sets the name of the step.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Gets or sets the time taken for the operation, in milliseconds.
    /// </summary>
    double TimeTaken { get; set; }

    /// <summary>
    /// Gets or sets the number of items that have been processed.
    /// </summary>
    int ProcessedItemsCount { get; set; }

    /// <summary>
    /// Gets or sets the collection of neighboring steps in the linked list.
    /// </summary>
    LinkedList<IStep> Neighbourood { get; set; }

    /// <summary>
    /// Gets or sets the current node in the linked list of steps.
    /// </summary>
    LinkedListNode<IStep> Current { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the event has been triggered.
    /// </summary>
    bool WasFired { get; set; }

    bool BreakProcessing { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether iterator must process step after cancellation occurs via CancellationToken.
    /// </summary>
    bool MustProcessAfterCancel { get; set; }

    /// <summary>
    /// Determines whether the current operation can be processed.
    /// </summary>
    /// <returns><see langword="true"/> if the operation can be processed; otherwise, <see langword="false"/>.</returns>
    bool CanProcess();

    /// <summary>
    /// Determines whether the operation is asynchronous.
    /// </summary>
    /// <returns><see langword="true"/> if the operation is asynchronous; <see langword="false"/> when it's synchronous.</returns>
    bool IsAsync();

    /// <summary>
    /// Processes the asynchronous operation.
    /// </summary>
    /// <remarks>The operation can be cancelled by passing a cancellation token. Ensure to handle the
    /// cancellation appropriately in the calling code.</remarks>
    /// <param name="token">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous processing operation.</returns>
    Task ProcessAsync(CancellationToken token);

    /// <summary>
    /// Processes the synchronous operation.
    /// </summary>
    void Process();
  }
}
