namespace Anixe.TransactionSteps
{
  /// <summary>
  /// Represents a step in a value task workflow that operates with a specific context.
  /// </summary>
  /// <typeparam name="T">The type of the context associated with this step.</typeparam>
  public interface IValueTaskStep<T> : IValueTaskStep
  {
    T Context { get; set; }
  }
}
