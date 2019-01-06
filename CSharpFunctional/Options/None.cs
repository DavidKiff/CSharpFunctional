namespace CSharpFunctional.Options
{
    public sealed class None<T> : Option<T>
    {
        private None() { }

        public static None<T> Value { get; } = new None<T>();
    }

    public sealed class None
    {
        private None() { }

        public static None Value { get; } = new None();
    }
}