namespace UnitTests.CodeToBeAnalysed.Complex
{
    public class ComplexAnalysis1
    {
        public void ThisIsVulnerableMethod(string arg)
        {
            string myString = arg;
            int arg3;

            string arg1 = CreateStringValue(1 < 2 ? myString : string.Empty);
            string arg2 = CreateStringValue("");
            arg3 = 0;
            arg3 = 1;

            SinkMethodOne(arg1, arg2, arg3, new MyClass(string.Empty));
        }

        private void SinkMethodOne(string arg1, string arg2, int arg3, MyClass myClass)
        {

        }

        private string CreateStringValue(string arg)
        {
            return arg;
        }

        public class MyClass
        {
            public MyClass(string s)
            {

            }
        }
    }
}
