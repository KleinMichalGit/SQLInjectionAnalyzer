namespace UnitTests.CodeToBeAnalysed.CreationRules.SafeCreationRules
{
    public class SafeCreationClass
    {
        public void ThisIsSafeMethod(string arg)
        {
            string myString = "my String";
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