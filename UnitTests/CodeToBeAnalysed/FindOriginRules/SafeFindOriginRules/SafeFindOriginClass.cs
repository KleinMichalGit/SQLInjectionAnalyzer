using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.CodeToBeAnalysed.FindOriginRules.SafeFindOriginRules
{
    public class SafeFindOriginClass
    {
        public void ThisIsSafeMethod(string arg)
        {
            string myString = "my String";
            MyClass myClass = new MyClass(myString);

            SinkMethodOne(myClass); //find origin should look above this line. Not bellow!

            myClass = new MyClass(arg);
            myClass = new MyClass(myString);
            
            myString = arg;
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
