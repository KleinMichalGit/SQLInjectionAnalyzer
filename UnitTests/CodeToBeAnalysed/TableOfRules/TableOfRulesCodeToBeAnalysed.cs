using System;

namespace UnitTests.CodeToBeAnalysed.TableOfRules
{
    public class TableOfRulesCodeToBeAnalysed
    {
        private void A()
        {
            MyClass a = new MyClass();
            string myString = "my string" + " another " + "string"; 
            int i = -1;

            MyClass b = new MyClass("a");
            MyClass c = new MyClass("a", 0);

            a = new MyClass();
            b = new MyClass("a");
            c = new MyClass("a", 0);
        }

        private string B(string a)
        {
            return "";
        }

        private void C(string a, string b)
        {

        }

        private void D(string a, int b, bool c)
        {

        }

        private void E()
        {
            A();
            B("");
            C("", "");
            D("", 0, true);

            string myString;

            myString = 1 < 2 ? "True" : "";
            myString = 1 > 2 ? "" : "False";
            myString = B("").Length > 9 ? "True" : "False";
        }

        private void F(string a)
        {
            string b = "";
            string c = b;
            b = "new string";

            C(c, b);
            string vulnerableString = a;
        }

        public class MyClass {
            public MyClass()
            {

            }

            public MyClass(string a)
            {

            }

            public MyClass(string a, int b)
            {

            }
        }
    }
}
