namespace UnitTests.CodeToBeAnalysed.FindOriginRules.VulnerableFindOriginRules
{
    public class VulnerableFindOriginClass
    {
        public void ThisIsVulnerableMethod(string arg)
        {
            SinkMethodOne(arg); //find origin should look above this line. Not bellow!

            arg = "my String";
        }

        public void SinkMethodOne(string myString)
        {
        }
    }
}