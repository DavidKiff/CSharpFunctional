using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpFunctional.Options
{
    public static class OptionExtensions
    {
        public static IEnumerable<Option<TResult>> Map<T, TResult>(this IEnumerable<Option<T>> enumerable, Func<T, TResult> map)
        {
            return enumerable.Select(option => option.Map(map));
        }

        public static IEnumerable<Option<T>> Map<T>(this IEnumerable<Option<T>> enumerable)
        {
            return enumerable.Select(option => option.Map(o => o));
        }

        public static Option<TResult> Map<T, TResult>(this Option<T> option, Func<T, TResult> map)
        {
            return option.Match(value => (Option<TResult>)map(value),
                                () => None<TResult>.Value);
        }
        
        public static IEnumerable<Option<TResult>> FlatMap<T, TResult>(this IEnumerable<Option<T>> options, Func<T, Option<TResult>> map)
        {
            return options.OfType<Some<T>>().Select(some => map(some.Content));
        }

        public static IEnumerable<Option<T>> FlatMap<T>(this IEnumerable<Option<T>> options)
        {
            return options.OfType<Some<T>>();
        }

        public static IEnumerable<Option<T>> Filter<T>(this IEnumerable<Option<T>> enumerable, Func<T, bool> predicate)
        {
            return enumerable.Select(value => value is Some<T> some && predicate(some) ? some : (Option<T>)None<T>.Value);
        }

        public static IEnumerable<Option<T>> Filter<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            return enumerable.Select(value => value.Filter(predicate));
        }

        public static Option<T> Filter<T>(this T value, Func<T, bool> predicate)
        {
            return predicate(value) ? (Option<T>) value : None.Value;
        }
        
        public static Option<T> Filter<T>(this Option<T> value, Func<T, bool> predicate)
        {
            return value is Some<T> some && predicate(some.Content) ? value : None.Value;
        }

        public static T GetOrElse<T>(this Option<T> option, T valueWhenNone = default(T))
        {
            return option is Some<T> some ? (T)some : valueWhenNone;
        }

        public static T GetOrElse<T>(this Option<T> option, Func<T> valueWhenNoneFactory)
        {
            return option is Some<T> some ? (T)some : valueWhenNoneFactory();
        }
        
        public static T GetOrThrow<T>(this Option<T> option, Exception exception)
        {
            return option is Some<T> some ? (T)some : throw exception;
        }

        public static T GetOrThrow<T>(this Option<T> option, Func<Exception> exceptionFactory)
        {
            return option is Some<T> some ? (T)some : throw exceptionFactory();
        }

        public static Option<T> FirstOrNone<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            return enumerable.Where(predicate).Select<T, Option<T>>(t => t).DefaultIfEmpty(None.Value).First();
        }

        public static void ForEach<T>(this Option<T> option, Action<T> action)
        {
            if (option is Some<T> some) action(some.Content);
        }

        public static void ForEach<T>(this IEnumerable<Option<T>> enumerable, Action<T> action)
        {
            foreach (var item in enumerable.OfType<Some<T>>())
            {
                action(item);
            }
        }
        
        public static List<T> ToList<T>(this Option<T> option)
        {
            if (option is Some<T> some) return new List<T>(1) { some.Content };

            return new List<T>();
        }

        public static List<T> ToList<T>(this IEnumerable<Option<T>> enumerable)
        {
            return enumerable.ToEnumerable().ToList();
        }
        
        public static IEnumerable<T> ToEnumerable<T>(this Option<T> option)
        {
            if (option is Some<T> some) yield return some.Content;
        }

        public static IEnumerable<T> ToEnumerable<T>(this IEnumerable<Option<T>> enumerable)
        {
            return enumerable.OfType<Some<T>>().Select(some => some.Content);
        }
        
        public static TResult Match<T, TResult>(this Option<T> option, Func<T, TResult> some, Func<TResult> none)
        {
            if (option is Some<T> s) return some(s);

            return none();
        }

        public static bool Contains<T>(this Option<T> option, Func<T, bool> predicate)
        {
            return option is Some<T> some && predicate(some.Content);
        }

        public static bool Contains<T>(this IEnumerable<Option<T>> enumerable, Func<T, bool> predicate)
        {
            return enumerable.ToEnumerable().Any(predicate);
        }
    }
}
