namespace UnitTests.CodeToBeAnalysed.CleaningRules.NotAllArgumentsAreCleaned
{
    public class NotAllArgumentsCleanedClass
    {
        public void ThisIsVulnerableMethod(string arg)
        {
            const string thisIsConst = "this is const";

            SinkMethodOne(thisIsConst, arg);
        }

        public void SinkMethodOne(string arg1, string arg2)
        {

        }
    }
}
