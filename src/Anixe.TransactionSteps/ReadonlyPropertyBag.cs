namespace Anixe.TransactionSteps
{
  /// <summary>
  /// Represents a collection of properties that can be accessed by name or type, but cannot be modified once set.
  /// </summary>
  /// <remarks>This class extends <see cref="PropertyBag"/> to provide a read-only property storage mechanism.
  /// Once a property is set using the <see cref="Set{T}(T)"/> or <see cref="Set{T}(string, T)"/> methods, it cannot be
  /// changed or removed. This is useful for scenarios where immutability is required.</remarks>
  public class ReadonlyPropertyBag : PropertyBag
  {
    public override void Set<T>(T property)
    {
      var type = typeof(T);
      this.typedProperties.Add(type, property);
    }

    public override void Set<T>(string name, T property)
    {
      this.namedProperties.Add(name, property);
    }
  }
}
