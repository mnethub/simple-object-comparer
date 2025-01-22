namespace SimpleObjectComparer
{
    public class OldNewPair
    {
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }


        public static OldNewPair Get(object? oldValue, object? newValue)
        {
            return new OldNewPair { OldValue = oldValue, NewValue = newValue };
        }
    }
}
