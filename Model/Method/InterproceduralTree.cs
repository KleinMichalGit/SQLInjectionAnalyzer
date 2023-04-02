using System.Collections.Generic;

namespace Model.Method
{
    public class InterproceduralTree
    {
        public int Id { get; set; }
        public string MethodName { get; set; }
        public List<InterproceduralTree> Callers { get; set; }
    }
}