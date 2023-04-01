namespace UnitTests.CodeToBeAnalysed.CleaningRules.AllArgumentsAreCleaned
{
    public class ArgumentsAreCleaned2
    {
        public void ThisIsSafeMethod(string args)
        {
            SinkMethodOne("this is const");
        }

        public void SinkMethodOne(string args)
        {
        }
    }
}