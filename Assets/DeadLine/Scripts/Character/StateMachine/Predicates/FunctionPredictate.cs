using System;

namespace DeadLine
{
    public class FunctionPredicate : IPredicate
    {
        readonly Func<bool> function;
        readonly string description;

        public FunctionPredicate(Func<bool> function, string description = null)
        {
            this.function = function;
            this.description = description;
        }

        public bool Evaluate()
        {
            return function.Invoke();
        }

        public override string ToString()
        {
            return description;
        }
    }
}
