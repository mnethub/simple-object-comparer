namespace SimpleObjectComparer
{
    public interface IComparer
    {
        Delta Compare<T>(T oldObject, T newObject);
    }
}
