using System;
using Xunit;

namespace Anixe.TransactionSteps.Test
{
  public class PropertyBagTest
  {
    [Fact]
    public void Should_Set_ReferenceType_Property()
    {
      var subject = new PropertyBag();
      var item = new Tuple<string, int>("test", 3);
      subject.Set<Tuple<string, int>>(item);

      var actual = subject.Get<Tuple<string, int>>();
      Assert.Same(item, actual);
    }

    [Fact]
    public void Should_Set_Property_Multiple_Times()
    {
      var subject = new PropertyBag();
      var item = new Tuple<string, int>("test", 3);
      subject.Set<Tuple<string, int>>(item);

      var item2 = new Tuple<string, int>("test2", 4);
      subject.Set<Tuple<string, int>>(item2);

      var actual = subject.Get<Tuple<string, int>>();
      Assert.Same(item2, actual);
    }

    [Fact]
    public void Should_Set_ValueType_Property()
    {
      var subject = new PropertyBag();
      const int item = 3;
      subject.Set<int>(item);

      var actual = subject.Get<int>();
      Assert.Equal(item, actual);
    }

    [Fact]
    public void Should_Contain_Type_When_Set()
    {
      var subject = new PropertyBag();
      subject.Set<Tuple<int>>(Tuple.Create(1));

      Assert.True(subject.Contains<Tuple<int>>());
      Assert.False(subject.Contains<int>());
    }

    [Fact]
    public void Should_Handle_Named_Properties()
    {
      var subject = new PropertyBag();
      subject.Set<string>("name", "value");

      Assert.True(subject.Contains<string>("name"));
      Assert.Equal("value", subject.Get<string>("name"));
    }

    [Fact]
    public void Contains_With_Null_Name_Throws()
    {
      var subject = new PropertyBag();
      Assert.Throws<ArgumentNullException>(() => subject.Contains<string>(null!));
    }

    [Fact]
    public void Get_Returns_Typed_Item()
    {
      var subject = new PropertyBag();
      subject.Set<int>(1);
      Assert.Equal(1, subject.Get<int>());
    }

    [Fact]
    public void GetRequired_Returns_Typed_Item()
    {
      var subject = new PropertyBag();
      subject.Set<int>(1);
      Assert.Equal(1, subject.GetRequired<int>());
    }

    [Fact]
    public void Get_Returns_Named_Item()
    {
      var subject = new PropertyBag();
      subject.Set<int>("name", 1);
      Assert.Equal(1, subject.Get<int>("name"));
    }

    [Fact]
    public void GetRequired_Returns_Named_Item()
    {
      var subject = new PropertyBag();
      subject.Set("name", 1);
      Assert.Equal(1, subject.GetRequired<int>("name"));
    }

    [Fact]
    public void GetRequired_Throws_When_Missing()
    {
      var subject = new PropertyBag();
      Assert.Throws<InvalidOperationException>(() => subject.GetRequired<int>());
    }

    [Fact]
    public void GetRequired_ByName_Throws_When_Missing_Or_NullName()
    {
      var subject = new PropertyBag();
      Assert.Throws<ArgumentNullException>(() => subject.GetRequired<int>(null!));
      Assert.Throws<InvalidOperationException>(() => subject.GetRequired<int>("missing"));
    }

    [Fact]
    public void TryGet_Returns_False_When_Missing()
    {
      var subject = new PropertyBag();
      Assert.False(subject.TryGet<int>(out var v));
      Assert.Equal(default(int), v);
    }

    [Fact]
    public void TryGet_Named_Throws_On_NullName()
    {
      var subject = new PropertyBag();
      Assert.Throws<ArgumentNullException>(() => subject.TryGet<int>(null!, out _));
    }

    [Fact]
    public void Unset_Removes_Type_And_Named_Property()
    {
      var subject = new PropertyBag();
      subject.Set<int>(1);
      subject.Unset<int>();
      Assert.False(subject.Contains<int>());

      subject.Set<string>("n", "v");
      subject.Unset<string>("n");
      Assert.False(subject.Contains<string>("n"));
    }

    [Fact]
    public void Clone_Copies_Items_And_Is_Independent()
    {
      var subject = new PropertyBag();
      subject.Set<int>(1);
      subject.Set<string>("n", "v");

      var clone = subject.Clone();
      Assert.Equal(1, clone.Get<int>());
      Assert.Equal("v", clone.Get<string>("n"));

      subject.Set<int>(2);
      Assert.Equal(1, clone.Get<int>()); // clone is independent
    }

    [Fact]
    public void Clone_With_Exclude_Removes_Types()
    {
      var subject = new PropertyBag();
      subject.Set<int>(1);
      subject.Set<long>(1);
      subject.Set<string>("n", "v");

      var clone = subject.Clone(new Type[] { typeof(int) });
      Assert.False(clone.Contains<int>());
      Assert.True(clone.Contains<long>());
      Assert.True(clone.Contains<string>("n"));
    }

    [Fact]
    public void ReadonlyPropertyBag_Prevents_Overwrite()
    {
      var subject = new ReadonlyPropertyBag();
      subject.Set<int>(1);
      // second set should throw because ReadonlyPropertyBag uses Dictionary.Add
      Assert.Throws<ArgumentException>(() => subject.Set<int>(2));

      subject = new ReadonlyPropertyBag();
      subject.Set<string>("n", "v");
      Assert.Throws<ArgumentException>(() => subject.Set<string>("n", "x"));
    }

    [Fact]
    public void Null_Values_Are_Supported_For_Reference_And_Nullable()
    {
      var subject = new PropertyBag();
      subject.Set<string>(null);
      Assert.True(subject.Contains<string>());
      Assert.Null(subject.Get<string>());

      subject.Set<int?>(null);
      Assert.True(subject.Contains<int?>());
      Assert.Null(subject.Get<int?>());
    }
  }
}
