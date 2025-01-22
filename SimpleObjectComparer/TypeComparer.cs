namespace SimpleObjectComparer
{
    public abstract class BaseTypeComparer(Type type)
    {
        private readonly Type _type = type;

        public Type ObjectType { get { return _type; } }

        public abstract bool IsEqual<T>(T objA, T objB);

    }
}
