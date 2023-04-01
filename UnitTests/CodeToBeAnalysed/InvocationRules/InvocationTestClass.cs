namespace UnitTests.CodeToBeAnalysed.InvocationRules
{
    public class InvocationTestClass
    {
        public void ThisIsSaveMethod(string arg)
        {
            const string stringConst = "thisIsConst";

            string arg1 = CreateStringValue(stringConst);
            string arg2 = CreateStringValue(string.Empty);
            int arg3 = 0;

            SinkMethodOne(arg1, arg2, arg3);
        }

        private void SinkMethodOne(string arg1, string arg2, int arg3)
        {

        }

        private string CreateStringValue(string arg)
        {
            return arg;
        }
    }
}
