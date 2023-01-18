using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnitTests
{
    public class NodeFactory
    {
        public T FindSyntaxNode<T>(string filePath, int index)
        {
            string file = File.ReadAllText(filePath);
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(file);
            return syntaxTree.GetRoot().DescendantNodes().OfType<T>().ElementAt(index);
        }
    }
}
