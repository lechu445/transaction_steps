using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Anixe.TransactionSteps.Helpers;

namespace Anixe.TransactionSteps
{
  /// <summary>
  /// Provides functionality to execute a sequence of steps, with support for asynchronous and synchronous execution,
  /// error handling, and cancellation.
  /// </summary>
  /// <remarks>The <see cref="StepIterator{T}"/> class is designed to process a sequence of steps, represented
  /// by <see cref="IStep"/>  objects, in a defined order. It supports both synchronous and asynchronous execution of
  /// steps, and provides mechanisms  for handling errors and respecting cancellation requests. The class also tracks
  /// execution statistics for each step.</remarks>
  /// <typeparam name="T">The type of the context object that implements <see cref="IPropertyBag"/>. This context is used to share data
  /// between steps and is updated as steps are processed.</typeparam>
  public class StepIterator<T> : IStepIterator<T>
    where T : IPropertyBag
  {
    private readonly T context;
    private readonly List<StepStat> stats;

    /// <summary>
    /// Initializes a new instance of the <see cref="StepIterator{T}"/> class with the specified context.
    /// </summary>
    /// <param name="context">The context object used to initialize the iterator. This parameter cannot be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <see langword="null"/>.</exception>
    public StepIterator(T context)
    {
      this.context = context ?? throw new ArgumentNullException(nameof(context));
      this.stats = [];
    }

    public List<StepStat> Stats => this.stats;

    /// <summary>
    /// Executes a sequence of steps asynchronously, with optional error handling and cancellation support.
    /// </summary>
    /// <remarks>This method processes each step in the provided sequence, invoking either its synchronous or
    /// asynchronous execution method based on the step's configuration.  If an exception occurs and an <paramref
    /// name="errorHandler"/> is provided, the error handler step will be executed to handle the exception.  The method
    /// respects the <paramref name="token"/> for cancellation, but synchronous steps with <see
    /// cref="IStep.MustProcessAfterCancel"/> set to <see langword="true"/> will still be executed even after
    /// cancellation is requested.</remarks>
    /// <param name="services">An <see cref="IServiceProvider"/> instance used to resolve dependencies for each step. If null, an
    /// InvalidOperationException will be thrown when trying to resolve a service.</param>
    /// <param name="steps">A <see cref="LinkedList{T}"/> of <see cref="IStep"/> objects representing the sequence of steps to execute.
    /// Cannot be null.</param>
    /// <param name="errorHandler">An optional <see cref="IStep"/> to execute if an exception occurs during the processing of the steps. Can be
    /// null.</param>
    /// <param name="token">A <see cref="CancellationToken"/> that can be used to cancel the execution of the steps. If cancellation is
    /// requested, only synchronous steps marked with <see cref="IStep.MustProcessAfterCancel"/> will be executed.</param>
    /// <returns>A <typeparamref name="T"/> representing the final context after all steps have been processed.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="steps"/> is null.</exception>
    public async Task<T> IterateAllAsync(
      IServiceProvider? services,
      LinkedList<IStep> steps,
      IStep? errorHandler,
      CancellationToken token)
    {
      if (steps == null)
      {
        throw new ArgumentNullException(nameof(steps));
      }

      if (services == null)
      {
        services = AlwaysThrowingServiceProvider.Instance;
      }

      var currentNode = steps.First;
      this.stats.Clear();
      try
      {
        while (currentNode != null)
        {
          var step = RetrieveStep(steps, currentNode, services);
          if (!token.IsCancellationRequested || step.MustProcessAfterCancel)
          {
            if (step.IsAsync())
            {
              await ExecuteStepAsync(step, token).ConfigureAwait(false);
            }
            else
            {
              ExecuteStep(step);
            }

            if (step.BreakProcessing)
            {
              break;
            }
          }

          currentNode = currentNode.Next;
        }
      }
      catch (Exception ex) when (errorHandler != null)
      {
        this.context.Set<Exception>(ex);
        errorHandler.Services = services;
        await ExecuteStepAsync(errorHandler, token).ConfigureAwait(false);
      }

      return this.context;
    }

    /// <summary>
    /// Executes a sequence of steps, optionally handling errors with a specified error handler.
    /// </summary>
    /// <remarks>The method processes each step in the provided linked list sequentially. If a step sets its
    /// <c>BreakProcessing</c> property to <see langword="true"/>, the iteration stops immediately. <para> If an
    /// exception is thrown during the execution of a step and an <paramref name="errorHandler"/> is provided, the error
    /// handler step is executed with the exception set in the context. </para></remarks>
    /// <param name="services">An optional <see cref="IServiceProvider"/> instance used to resolve dependencies for the steps. Can be <see
    /// langword="null"/> if no services are required.</param>
    /// <param name="steps">A linked list of steps to execute. Cannot be <see langword="null"/>.</param>
    /// <param name="errorHandler">An optional step to execute if an exception occurs during the processing of the steps. Can be <see
    /// langword="null"/> if no error handling is needed.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="steps"/> is <see langword="null"/>.</exception>
    public void IterateAll(
      IServiceProvider? services,
      LinkedList<IStep> steps,
      IStep? errorHandler)
    {
      if (steps == null)
      {
        throw new ArgumentNullException(nameof(steps));
      }

      if (services == null)
      {
        services = AlwaysThrowingServiceProvider.Instance;
      }

      var currentNode = steps.First;
      this.stats.Clear();
      try
      {
        while (currentNode != null)
        {
          var step = RetrieveStep(steps, currentNode, services);
          ExecuteStep(step);
          if (step.BreakProcessing)
          {
            break;
          }

          currentNode = currentNode.Next;
        }
      }
      catch (Exception ex) when (errorHandler != null)
      {
        this.context.Set<Exception>(ex);
        errorHandler.Services = services;
        ExecuteStep(errorHandler);
      }
    }

    protected Task<T> Failed(Exception? ex) => Task.FromResult(this.context);

    protected void TakeStats(IStep step) => this.stats.Add(StepStat.CreateFromStep(step));

    private static IStep RetrieveStep(LinkedList<IStep> steps, LinkedListNode<IStep> currentNode, IServiceProvider services)
    {
      var step = currentNode.Value;
      step.Services = services;
      step.Neighbourood = steps;
      step.Current = currentNode;
      return step;
    }

    private async Task ExecuteStepAsync(IStep step, CancellationToken token)
    {
      EnsureGenericContext(step);

      var startTimestamp = Stopwatch.GetTimestamp();
      if (step.CanProcess())
      {
        if (step.IsAsync())
        {
          await step.ProcessAsync(token).ConfigureAwait(false);
        }
        else
        {
          step.Process();
        }

        var tt = StopwatchHelper.GetElapsedTime(startTimestamp).TotalMilliseconds;
        step.WasFired = true;
        step.TimeTaken = tt;
        TakeStats(step);
      }
    }

    private void ExecuteStep(IStep step)
    {
      EnsureGenericContext(step);

      if (step.CanProcess())
      {
        var startTimestamp = Stopwatch.GetTimestamp();
        if (step.IsAsync())
        {
          throw new NotSupportedException("Use IterateAllAsync method");
        }
        else
        {
          step.Process();
        }

        var tt = StopwatchHelper.GetElapsedTime(startTimestamp).TotalMilliseconds;
        step.WasFired = true;
        step.TimeTaken = tt;
        TakeStats(step);
      }
    }

    private void EnsureGenericContext(IStep step)
    {
      if (step is IStep<T> stepGeneric)
      {
        stepGeneric.Context = this.context;
      }
    }
  }
}
