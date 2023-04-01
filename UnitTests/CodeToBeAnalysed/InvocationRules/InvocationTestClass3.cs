namespace UnitTests.CodeToBeAnalysed.InvocationRules
{
    public class InvocationTestClass3
    {
        public void ThisIsVulnerableMethod(string arg)
        {
            string arg1 = CreateStringValue(arg);
            string arg2 = CreateStringValue(string.Empty);

            string arg3 = CreateStringValue(arg1);
            string arg4 = CreateStringValue(arg2);

            SinkMethodOne(arg3, arg4);
        }

        private string CreateStringValue(string arg)
        {
            return arg;
        }

        private void SinkMethodOne(string arg1, string arg2)
        {
        }
    }
}