namespace Anixe.TransactionSteps
{
  /// <summary>
  /// Represents a step in a process that operates on a context of type <typeparamref name="T"/>.
  /// </summary>
  /// <remarks>This interface extends <see cref="IStep"/> to include a context of a specific type, allowing for
  /// more specialized operations within a process.</remarks>
  /// <typeparam name="T">The type of the context that this step operates on.</typeparam>
  public interface IStep<T> : IStep
  {
    T Context { get; set; }
  }
}
