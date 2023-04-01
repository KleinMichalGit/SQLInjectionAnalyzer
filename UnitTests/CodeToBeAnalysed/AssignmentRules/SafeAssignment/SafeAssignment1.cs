namespace UnitTests.CodeToBeAnalysed.AssignmentRules.SafeAssignment
{
    public class SafeAssignment1
    {
        public void ThisIsSafeMethod(string arg)
        {
            string vulnerableString1 = "";
            string vulnerableString2;
            string vulnerableString3;
            string vulnerableString4;

            vulnerableString2 = vulnerableString1;
            vulnerableString3 = vulnerableString2;
            vulnerableString4 = vulnerableString3;

            SinkMethodOne(vulnerableString4);
        }

        public void SinkMethodOne(string arg1)
        {

        }
    }
}
