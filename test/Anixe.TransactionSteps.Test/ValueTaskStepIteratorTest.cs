#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anixe.TransactionSteps.Predefined;
using Xunit;

namespace Anixe.TransactionSteps.Test
{
  public class ValueTaskStepIteratorTest
  {
    private readonly PropertyBag ctx;
    private readonly ValueTaskStepIterator<IPropertyBag> subject;

    public ValueTaskStepIteratorTest()
    {
      this.ctx = new PropertyBag();
      this.subject = new ValueTaskStepIterator<IPropertyBag>();
    }

    public static IValueTaskStep CreateSyncStep(Action action, bool mustProcessAfterCancel = false)
    {
      return new CtxValueTaskStep<IPropertyBag>(
        syncAction: _ => action(),
        asyncAction: null,
        canProcessPredicate: () => true)
      {
        MustProcessAfterCancel = mustProcessAfterCancel,
      };
    }

    public static IValueTaskStep CreateAsyncStep(Action action, bool mustProcessAfterCancel = false)
    {
      return new CtxValueTaskStep<IPropertyBag>(
        syncAction: null,
        asyncAction: async (_, _) => action(),
        canProcessPredicate: () => true)
      {
        MustProcessAfterCancel = mustProcessAfterCancel,
      };
    }

    public static IValueTaskStep CreateStepWithPredicate(Action action, Func<bool> canProcessPredicate, bool mustProcessAfterCancel = false)
    {
      return new CtxValueTaskStep<IPropertyBag>(
        syncAction: _ => action(),
        asyncAction: null,
        canProcessPredicate: canProcessPredicate)
      {
        MustProcessAfterCancel = mustProcessAfterCancel,
      };
    }

    public static IValueTaskStep CreateStepWithBothActions(Action syncAction, Action asyncAction, bool mustProcessAfterCancel = false)
    {
      return new CtxValueTaskStep<IPropertyBag>(
        syncAction: _ => syncAction(),
        asyncAction: async (_, _) => asyncAction(),
        canProcessPredicate: () => true)
      {
        MustProcessAfterCancel = mustProcessAfterCancel,
      };
    }

    [Fact]
    public async Task Should_Return_Empty_Stats_When_Steps_Are_Empty()
    {
      var steps = Array.Empty<IValueTaskStep>();
      var stats = await this.subject.IterateAllAsync(this.ctx, steps, CancellationToken.None);
      Assert.Empty(stats);
    }

    [Fact]
    public async Task Should_IterateAllAsync_Over_All_Sync_Steps()
    {
      var calledSteps = new List<string>();
      var steps = new IValueTaskStep[]
      {
        CreateSyncStep(() => calledSteps.Add("Step1")),
        CreateSyncStep(() => calledSteps.Add("Step2")),
      };
      await this.subject.IterateAllAsync(this.ctx, steps, CancellationToken.None);
      Assert.True(steps.All(s => s.WasFired));
      Assert.Equal(["Step1", "Step2"], calledSteps);
    }

    [Fact]
    public async Task Should_IterateAllAsync_Over_All_Async_Steps()
    {
      var calledSteps = new List<string>();
      var steps = new IValueTaskStep[]
      {
        CreateAsyncStep(() => calledSteps.Add("AsyncStep1")),
        CreateAsyncStep(() => calledSteps.Add("AsyncStep2")),
      };
      await this.subject.IterateAllAsync(this.ctx, steps, CancellationToken.None);
      Assert.True(steps.All(s => s.WasFired));
      Assert.Equal(["AsyncStep1", "AsyncStep2"], calledSteps);
    }

    [Fact]
    public async Task Should_IterateAllAsync_Over_Async_Steps_Which_Must_Process_After_Cancel()
    {
      using (var cts = new CancellationTokenSource())
      {
        var calledSteps = new List<string>();
        var steps = new IValueTaskStep[]
        {
          CreateAsyncStep(() =>
          {
            calledSteps.Add("AsyncStep1");
            cts.Cancel();
          }),
          CreateAsyncStep(() =>
          {
            calledSteps.Add("AsyncStep2");
          },
          mustProcessAfterCancel: true),
        };
        await this.subject.IterateAllAsync(this.ctx, steps, cts.Token);
        Assert.True(steps.All(s => s.WasFired));
        Assert.Equal(["AsyncStep1", "AsyncStep2"], calledSteps);
      }
    }

    [Fact]
    public async Task Should_IterateAllAsync_Over_Async_Steps_Which_Must_Not_Process_After_Cancel()
    {
      using (var cts = new CancellationTokenSource())
      {
        var calledSteps = new List<string>();
        var steps = new IValueTaskStep[]
        {
          CreateAsyncStep(() =>
          {
            calledSteps.Add("AsyncStep1");
            cts.Cancel();
          }),
          CreateAsyncStep(() =>
          {
            calledSteps.Add("AsyncStep2");
          },
          mustProcessAfterCancel: false)
        };
        await this.subject.IterateAllAsync(this.ctx, steps, cts.Token);
        Assert.Single(steps, s => s.WasFired);
        Assert.Equal(["AsyncStep1"], calledSteps);
      }
    }

    [Fact]
    public async Task Should_Throw_ArgumentNullException_When_Steps_Are_Null()
    {
      await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        await this.subject.IterateAllAsync(this.ctx, null!, CancellationToken.None));
    }

    [Fact]
    public async Task Should_Throw_Exception_Which_Occurred_In_Step()
    {
      var calledSteps = new List<string>();
      var steps = new IValueTaskStep[]
      {
        CreateAsyncStep(() =>
        {
          calledSteps.Add("AsyncStep1");
          throw new Exception("Test exception");
        }),
        CreateAsyncStep(() =>
        {
          calledSteps.Add("AsyncStep2");
        })
      };

      var ex = await Assert.ThrowsAsync<Exception>(async () => await this.subject.IterateAllAsync(this.ctx, steps, CancellationToken.None));
      Assert.Equal("Test exception", ex.Message);
      Assert.Equal(["AsyncStep1"], calledSteps);
    }

    [Fact]
    public async Task Should_Not_Fire_Steps_When_CanProcessPredicate_Is_False()
    {
      var calledSteps = new List<string>();
      var steps = new IValueTaskStep[]
      {
        CreateStepWithPredicate(() => calledSteps.Add("Step1"), () => false),
        CreateStepWithPredicate(() => calledSteps.Add("Step2"), () => false)
      };
      await this.subject.IterateAllAsync(this.ctx, steps, CancellationToken.None);
      Assert.All(steps, s => Assert.False(s.WasFired));
      Assert.Empty(calledSteps);
    }

    [Fact]
    public async Task Should_Handle_Steps_With_Both_Sync_And_Async_Actions()
    {
      var called = new List<string>();
      var steps = new IValueTaskStep[]
      {
        CreateStepWithBothActions(() => called.Add("Sync"), () => called.Add("Async")),
      };
      await this.subject.IterateAllAsync(this.ctx, steps, CancellationToken.None);
      Assert.True(steps[0].WasFired);
      // Only async action should be called, as syncAction is only used if asyncAction is null
      Assert.Contains("Async", called);
    }

    [Fact]
    public async Task Should_Throw_ArgumentNullException_When_Context_Is_Null()
    {
      var steps = new IValueTaskStep[] { CreateSyncStep(() => { }) };
      await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        await this.subject.IterateAllAsync(null!, steps, CancellationToken.None));
    }

    [Fact]
    public async Task Should_IterateAllAsync_With_Mixed_MustProcessAfterCancel_Values()
    {
      using (var cts = new CancellationTokenSource())
      {
        var calledSteps = new List<string>();
        var steps = new IValueTaskStep[]
        {
          CreateAsyncStep(() => { calledSteps.Add("A"); cts.Cancel(); }, mustProcessAfterCancel: false),
          CreateAsyncStep(() => calledSteps.Add("B"), mustProcessAfterCancel: true),
          CreateAsyncStep(() => calledSteps.Add("C"), mustProcessAfterCancel: false),
        };
        await this.subject.IterateAllAsync(this.ctx, steps, cts.Token);
        Assert.True(steps[0].WasFired);
        Assert.True(steps[1].WasFired);
        Assert.False(steps[2].WasFired);
        Assert.Equal(["A", "B"], calledSteps);
      }
    }

    [Fact]
    public async Task Should_IterateAllAsync_With_NoOp_Steps()
    {
      var steps = new IValueTaskStep[]
      {
        CreateSyncStep(() => { }),
        CreateAsyncStep(() => { })
      };
      var stats = await this.subject.IterateAllAsync(this.ctx, steps, CancellationToken.None);
      Assert.All(steps, s => Assert.True(s.WasFired));
      Assert.NotNull(stats);
    }

    [Fact]
    public async Task Should_Propagate_OperationCanceledException()
    {
      var steps = new IValueTaskStep[]
      {
        CreateAsyncStep(() => throw new OperationCanceledException()),
      };
      await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        await this.subject.IterateAllAsync(this.ctx, steps, CancellationToken.None));
    }
  }
}
