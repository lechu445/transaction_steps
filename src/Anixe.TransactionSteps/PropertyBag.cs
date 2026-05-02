using System;
using System.Collections.Generic;
using System.Linq;
using Anixe.TransactionSteps.Helpers;

namespace Anixe.TransactionSteps
{
  /// <summary>
  /// Represents a container for storing and retrieving properties by type or name.
  /// </summary>
  /// <remarks>The <see cref="PropertyBag"/> class provides a flexible mechanism for managing properties that
  /// can be accessed either by their type or by a string name. It supports adding, retrieving, updating, and removing
  /// properties, as well as cloning the property bag with optional exclusions.  This class is particularly useful in
  /// scenarios where a loosely-typed collection of properties is needed, such as in extensibility frameworks or
  /// metadata storage.</remarks>
  public class PropertyBag : IPropertyBag
  {
    protected readonly Dictionary<Type, object?> typedProperties;
    protected readonly Dictionary<string, object?> namedProperties;

    public PropertyBag()
    {
      this.namedProperties = new Dictionary<string, object?>();
      this.typedProperties = new Dictionary<Type, object?>();
    }

    private PropertyBag(Dictionary<Type, object?> typedProperties, Dictionary<string, object?> namedProperties)
    {
      this.namedProperties = new Dictionary<string, object?>(namedProperties);
      this.typedProperties = new Dictionary<Type, object?>(typedProperties);
    }

    private PropertyBag(IEnumerable<KeyValuePair<Type, object?>> typedProperties, Dictionary<string, object?> namedProperties)
    {
      this.namedProperties = new Dictionary<string, object?>(namedProperties);
#if NETSTANDARD2_0
      this.typedProperties = new Dictionary<Type, object?>();
      foreach (var prop in typedProperties)
      {
        this.typedProperties.Add(prop.Key, prop.Value);
      }
#else
      this.typedProperties = new Dictionary<Type, object?>(typedProperties);
#endif
    }

    public bool Contains<T>()
    {
      return this.typedProperties.ContainsKey(typeof(T));
    }

    public bool Contains<T>(string name)
    {
      if (name is null)
      {
        ThrowHelper.ThrowArgumentNullException(nameof(name));
      }

      return this.namedProperties.TryGetValue(name!, out var item) && item is T;
    }

    public virtual void Set<T>(T property)
    {
      var type = typeof(T);
      this.typedProperties[type] = property;
    }

    public virtual void Set<T>(string name, T property)
    {
      if (name is null)
      {
        ThrowHelper.ThrowArgumentNullException(nameof(name));
      }

      this.namedProperties[name!] = property;
    }

    public T? Get<T>()
    {
      var type = typeof(T);
      if (this.typedProperties.TryGetValue(type, out var item))
      {
        return (T?)item;
      }

      return default;
    }

    public T? Get<T>(string name)
    {
      if (name is null)
      {
        ThrowHelper.ThrowArgumentNullException(nameof(name));
      }

      if (this.namedProperties.TryGetValue(name!, out var item))
      {
        return (T?)item;
      }

      return default;
    }

    public T GetRequired<T>()
    {
      var type = typeof(T);
      if (this.typedProperties.TryGetValue(type, out var item))
      {
        return (T)item!;
      }

      ThrowRequired();
      return default!; // never reached

      static void ThrowRequired() => throw new InvalidOperationException($"{typeof(PropertyBag).Name} requires to contain type {typeof(T).Name}");
    }

    public T GetRequired<T>(string name)
    {
      if (name is null)
      {
        ThrowHelper.ThrowArgumentNullException(nameof(name));
      }

      if (this.namedProperties.TryGetValue(name!, out var item))
      {
        return (T)item!;
      }

      ThrowRequired();
      return default!; // never reached

      static void ThrowRequired() => throw new InvalidOperationException($"{typeof(PropertyBag).Name} requires to contain type {typeof(T).Name} with provided name");
    }

    public bool TryGet<T>(out T value)
    {
      var type = typeof(T);
      if (this.typedProperties.TryGetValue(type, out var item))
      {
        value = (T)item!;
        return true;
      }

      value = default!;
      return false;
    }

    public bool TryGet<T>(string name, out T value)
    {
      if (name is null)
      {
        ThrowHelper.ThrowArgumentNullException(nameof(name));
      }

      if (this.namedProperties.TryGetValue(name!, out var item))
      {
        value = (T)item!;
        return true;
      }

      value = default!;
      return false;
    }

    public void Unset<T>()
    {
      var type = typeof(T);
      this.typedProperties.Remove(type);
    }

    public void Unset<T>(string name)
    {
      if (name is null)
      {
        ThrowHelper.ThrowArgumentNullException(nameof(name));
      }

      if (this.namedProperties.TryGetValue(name!, out var item))
      {
        this.namedProperties.Remove(name);
      }
    }

    public IPropertyBag Clone()
    {
      return new PropertyBag(this.typedProperties, this.namedProperties);
    }

    public IPropertyBag Clone(Type[] exclude)
    {
      if (exclude is null)
      {
        throw new ArgumentNullException(nameof(exclude));
      }

      var cloned = this.typedProperties.Where(kvp => !Array.Exists(exclude, item => item == kvp.Key));
      return new PropertyBag(
        cloned,
        this.namedProperties);
    }
  }
}
