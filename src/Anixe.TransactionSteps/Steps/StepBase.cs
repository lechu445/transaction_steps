using System;
using System.Collections.Generic;

namespace Anixe.TransactionSteps
{
  /// <summary>
  /// Represents the base class for a processing step in a workflow or pipeline.
  /// </summary>
  /// <remarks>This abstract class provides common properties and functionality for derived step classes,
  /// including management of step name, processing state, and neighboring steps in a sequence.</remarks>
  public abstract class StepBase
  {
    private string? stepName;

    public IServiceProvider Services { get; set; } = AlwaysThrowingServiceProvider.Instance;

    /// <summary>
    /// Gets or sets the name of the step. The default is step type name.
    /// </summary>
    public string Name
    {
      get
      {
        if (string.IsNullOrEmpty(this.stepName))
        {
          this.stepName = GetDefaultStepName();
        }

        return this.stepName!;
      }
      set => this.stepName = value;
    }

    public double TimeTaken { get; set; }

    public int ProcessedItemsCount { get; set; }

    public LinkedList<IStep> Neighbourood { get; set; } = new LinkedList<IStep>();

    public LinkedListNode<IStep> Current { get; set; } = null!;

    public bool WasFired { get; set; }

    public bool BreakProcessing { get; set; }

    public bool MustProcessAfterCancel { get; set; } = false;

    private string GetDefaultStepName() => this.GetType().Name;
  }
}
