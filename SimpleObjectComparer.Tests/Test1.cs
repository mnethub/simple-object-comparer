using static SimpleObjectComparer.Tests.Models.NestedModel;

namespace SimpleObjectComparer.Tests
{
    [TestClass]
    public sealed class Test1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var model = Helpers.Helper.GetModel<Person>("json1");

        }
    }
}
