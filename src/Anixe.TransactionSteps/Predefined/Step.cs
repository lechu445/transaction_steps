using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anixe.TransactionSteps.Predefined
{
  /// <summary>
  /// Represents a processing step that can be executed synchronously or checked for its ability to process.
  /// </summary>
  /// <remarks>This class provides a mechanism to define a processing action and a predicate to determine
  /// whether the step can be processed. The step is always synchronous, and asynchronous processing is not
  /// supported.</remarks>
  public class Step : StepBase, IStep
  {
    private readonly Action<IStep>? action;
    private readonly Predicate<IStep> canProcessPredicate;

    /// <summary>
    /// Initializes a new instance of the <see cref="Step"/> class with the specified action and an optional predicate to determine if
    /// the step can be processed.
    /// </summary>
    /// <param name="action">The action to execute when the step is processed. May be null if no action is required.</param>
    /// <param name="canProcessPredicate">A predicate that determines whether the step can be processed. If null, the step is always considered
    /// processable.</param>
    public Step(Action<IStep>? action, Predicate<IStep>? canProcessPredicate = null)
    {
      this.action = action;
      this.canProcessPredicate = canProcessPredicate ?? True;
    }

    public bool CanProcess() => this.canProcessPredicate(this);

    public bool IsAsync() => false;

    public void Process() => this.action?.Invoke(this);

    public Task ProcessAsync(CancellationToken token) => Task.FromException(new NotSupportedException());

    private static bool True(IStep step) => true;
  }
}
