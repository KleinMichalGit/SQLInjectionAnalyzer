namespace UnitTests.CodeToBeAnalysed.OneVulnerableMethod
{
    public class SimpleClass
    {
        public void ThisIsVulnerableMethod(string args)
        {
            SinkMethodOne(args);
        }

        public void SinkMethodOne(string args)
        {
        }
    }
}