namespace SimpleObjectComparer
{
    public class Delta
    {
        public Delta() { }
        public Delta(Type type)
        {
            TypeName = type.Name;
        }
        public string? TypeName { get; set; }
        public bool IsAdded { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public bool IsModified { get; set; } = false;

        public Dictionary<string, object?> Keys { get; set; } = [];

        // Simple types
        public Dictionary<string, OldNewPair> SimpleTypes { get; set; } = [];
        public Dictionary<string, OldNewPair> SimpleListTypes { get; set; } = [];

        // ComplexTypes
        public Dictionary<string, Delta> ComplexTypes { get; set; } = [];
        public Dictionary<string, List<Delta>> ComplexListTypes { get; set; } = [];

        public List<string> IgnoredFields { get; set; } = [];
        public List<string> UnsupportedFields { get; set; } = [];

    }
}
