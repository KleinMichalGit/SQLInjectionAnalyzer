namespace UnitTests.CodeToBeAnalysed.CreationRules.VulnerableCreationRules
{
    public class VulnerableCreationClass
    {
        public void ThisIsVulnerableMethod(string arg)
        {
            string myString = arg;
            MyClass myClass = new MyClass(myString);

            SinkMethodOne(myClass);
        }

        public void SinkMethodOne(MyClass myClass)
        {

        }

        public class MyClass
        {
            public MyClass(string s)
            {

            }
        }
    }
}
