using System.Threading;
using System.Threading.Tasks;

namespace Anixe.TransactionSteps
{
  /// <summary>
  /// Represents a step in a value task processing sequence, providing properties and methods to manage and execute the
  /// step.
  /// </summary>
  /// <remarks>This interface defines the structure for a processing step that can be part of a larger task
  /// sequence.  It includes properties to track the step's name, execution time, and processing status, as well as
  /// methods  to determine if the step can be processed and to execute the processing logic.</remarks>
  public interface IValueTaskStep
  {
    string Name { get; set; }

    /// <summary>
    /// Gets or sets the total time taken to complete the operation, measured in milliseconds.
    /// </summary>
    double TimeTaken { get; set; }

    int ProcessedItemsCount { get; set; }

    bool WasFired { get; set; }

    bool BreakProcessing { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether iterator must process step after cancellation occurs via CancellationToken. Flag is valid only for sync steps.
    /// </summary>
    bool MustProcessAfterCancel { get; set; }

    bool CanProcess();

    ValueTask Process(CancellationToken token);
  }
}
