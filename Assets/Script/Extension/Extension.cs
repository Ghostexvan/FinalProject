using System;
using System.Collections.Generic;
using System.Threading;


static class Extension{
    public static void Shuffle<T>(this IList<T> list)
    {
      int n = list.Count;
      while (n > 1)
      {
        n--;
        int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
        T value = list[k];
        list[k] = list[n];
        list[n] = value;
      }
    }
}

  public static class ThreadSafeRandom
  {
      [ThreadStatic] private static System.Random Local;

      public static System.Random ThisThreadsRandom
      {
          get { return Local ?? (Local = new System.Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
      }
  }