namespace Anixe.TransactionSteps.Benchmark
{
  internal sealed class SyncStep : SyncStepBase<IPropertyBag>, IStep<IPropertyBag>
  {
    public bool CanProcess()
    {
      return true;
    }

    public void Process()
    {
    }
  }
}
