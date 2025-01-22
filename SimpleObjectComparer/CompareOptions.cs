namespace SimpleObjectComparer
{
    public class CompareOptions
    {
        public bool NoCache { get; set; } = false;
        public bool IncludeTypeMappings { get; set; } = false;
        public List<BaseTypeComparer> CustomTypeComparers { get; set; } = [];
    }

}
