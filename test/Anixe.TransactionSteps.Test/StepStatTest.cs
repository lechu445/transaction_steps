using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Anixe.TransactionSteps.Predefined;
using Xunit;

namespace Anixe.TransactionSteps.Test;

public class StepStatTest
{
  [Fact]
  public void Should_CreateStepStat_From_IStep()
  {
    var step = new Step(null);
    step.Name = "s1";
    step.TimeTaken = 12.34;
    step.ProcessedItemsCount = 7;

    var stat = StepStat.CreateFromStep(step);
    Assert.Equal("s1", stat.Name);
    Assert.Equal(12.34, stat.TimeTaken);
    Assert.Equal(7, stat.ProcessedItemsCount);
  }

  private class DummyValueTaskStep : IValueTaskStep
  {
    public string Name { get; set; } = string.Empty;
    public double TimeTaken { get; set; }
    public int ProcessedItemsCount { get; set; }
    public bool WasFired { get; set; }
    public bool BreakProcessing { get; set; }
    public bool MustProcessAfterCancel { get; set; }
    public bool CanProcess() => true;
    public ValueTask Process(CancellationToken token) => new ValueTask();
  }

  [Fact]
  public void Should_CreateStepStat_From_IValueTaskStep()
  {
    var step = new DummyValueTaskStep { Name = "v1", TimeTaken = 3.21, ProcessedItemsCount = 2 };
    var stat = StepStat.CreateFromStep(step);
    Assert.Equal("v1", stat.Name);
    Assert.Equal(3.21, stat.TimeTaken);
    Assert.Equal(2, stat.ProcessedItemsCount);
  }

  [Fact]
  public async Task Should_Fill_Stats_After_IterateAllAsync()
  {
    var ctx = new PropertyBag();
    var subject = new StepIterator<IPropertyBag>(ctx);
    var steps = new LinkedList<IStep>(
    [
      new CtxStep<IPropertyBag>((_) => { }, (_) => true),
      new CtxStep<IPropertyBag>((_) => { }, (_) => true),
    ]);

    await subject.IterateAllAsync(null, steps, null, CancellationToken.None);
    Assert.Equal(2, subject.Stats.Count);
    Assert.Contains(subject.Stats, s => s.Name == steps.First.Value.Name);
  }

  [Fact]
  public void Should_Fill_Stats_After_IterateAll()
  {
    var ctx = new PropertyBag();
    var subject = new StepIterator<IPropertyBag>(ctx);
    var steps = new LinkedList<IStep>(
    [
      new CtxStep<IPropertyBag>((_) => { }, (_) => true),
      new CtxStep<IPropertyBag>((_) => { }, (_) => true),
    ]);

    subject.IterateAll(null, steps, null);
    Assert.Equal(2, subject.Stats.Count);
    Assert.Contains(subject.Stats, s => s.Name == steps.First.Value.Name);
  }
}
