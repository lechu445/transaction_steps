using System;
using System.Diagnostics.CodeAnalysis;

namespace Anixe.TransactionSteps.Helpers;

internal static class ThrowHelper
{
#if !NETSTANDARD2_0
  [DoesNotReturn]
#endif
  public static void ThrowArgumentNullException(string paramName)
  {
    throw new ArgumentNullException(paramName);
  }
}
