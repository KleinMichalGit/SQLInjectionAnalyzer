namespace UnitTests.CodeToBeAnalysed.CleaningRules.AllArgumentsAreCleaned
{
    internal class ArgumentsAreCleaned1
    {
        public void ThisIsSafeMethod(string args)
        {
            SinkMethodOne(string.Empty);
        }

        public void SinkMethodOne(string args)
        {
        }
    }
}