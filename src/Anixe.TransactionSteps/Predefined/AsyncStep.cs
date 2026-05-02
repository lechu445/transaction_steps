using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anixe.TransactionSteps.Predefined
{
  public class AsyncStep : StepBase, IStep
  {
    private readonly Func<IStep, Task> action;
    private readonly Predicate<IStep> canProcessPredicate;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncStep"/> class with the specified asynchronous action and an optional
    /// predicate to determine if the step can be processed.
    /// </summary>
    /// <param name="action">The asynchronous action to execute for this step. The action receives the current step as its parameter and
    /// returns a Task representing the asynchronous operation.</param>
    /// <param name="canProcessPredicate">An optional predicate that determines whether the step can be processed. If null, the step is always considered
    /// processable.</param>
    /// <exception cref="ArgumentNullException">Thrown if action is null.</exception>
    public AsyncStep(Func<IStep, Task> action, Predicate<IStep>? canProcessPredicate = null)
    {
      this.action = action ?? throw new ArgumentNullException(nameof(action));
      this.canProcessPredicate = canProcessPredicate ?? this.True;
    }

    public bool CanProcess() => this.canProcessPredicate(this);

    public bool IsAsync() => true;

    public void Process() => throw new NotSupportedException();

    public Task ProcessAsync(CancellationToken token)
    {
      if (token.IsCancellationRequested)
      {
        return Task.FromCanceled(token);
      }

      return this.action(this);
    }

    private bool True(IStep step) => true;
  }
}
