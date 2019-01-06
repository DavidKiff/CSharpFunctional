namespace CSharpFunctional.Options
{
    public sealed class Some<T> : Option<T>
    {
        public Some(T content)
        {
            Content = content;
        }
        
        public T Content { get; }

        public static implicit operator T(Some<T> value) => value.Content;
    }
}