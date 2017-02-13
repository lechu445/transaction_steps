using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anixe.TransactionSteps.Predefined
{
  public class Step : StepBase, IStep
  {
    private Action<IStep> action;
    private Predicate<IStep> canProcessPredicate;

    public Step(Action<IStep> action, Predicate<IStep> canProcessPredicate = null)
    {
      this.action = action;        
      this.canProcessPredicate = canProcessPredicate ?? True;
    }

    public bool CanProcess()
    {
        return this.canProcessPredicate(this);
    }

    public bool IsAsync()
    {
        return false;
    }

    public void Process()
    {
      if(this.action != null)
      {
        this.action(this);
      }
    }

    public async Task ProcessAsync(CancellationToken token)
    {
      #if !NET45
      await Task.FromException(new NotImplementedException());
      #else
      var ctx = new TaskCompletionSource<bool>();
      ctx.SetException(new NotImplementedException());
      await ctx.Task;
      #endif
    }

    private bool True(IStep step)
    {
      return true;
    }    
  }
}