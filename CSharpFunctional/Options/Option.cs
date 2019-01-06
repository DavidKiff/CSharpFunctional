
namespace CSharpFunctional.Options
{
    public abstract class Option<T>
    {
        public static implicit operator Option<T>(T value) => ReferenceEquals(value, null) ? (Option<T>) None<T>.Value : new Some<T>(value);

        public static implicit operator Option<T>(None value) => None<T>.Value;

        public override string ToString() => this is Some<T> some ? $"Some({some.Content?.ToString() ?? "Null"})" : "None";
    }
}
