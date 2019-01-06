using System;

namespace CSharpFunctional.Extensions
{
    public static class FunctionalExtensions
    {
        public static TReturn Map<TSource, TReturn>(this TSource source, Func<TSource, TReturn> map)
        {
            return map(source);
        }

        public static TSource Do<TSource>(this TSource source, Action<TSource> action)
        {
            action(source);
            return source;
        }
    }
}
