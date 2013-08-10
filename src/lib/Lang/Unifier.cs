using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace lib.Lang
{
    /// <summary>
    /// This class unifies expression to canonical form.
    /// In canonical form there is at most 3 variables
    /// Function args has name <code>x</code>
    /// Fold iterator has name <code>i</code>
    /// Fold accumulator has name <code>a</code>
    /// </summary>
    class Unifier
    {
        private int counter = 0;
        private IDictionary<string, string> replaces = new Dictionary<string, string>(); 
        public void Unify(Binary x)
        {
            Unify(x.A);
            Unify(x.B);
        }
        public void Unify(Const c)
        {
            // Dummy
        }

        public void Unify(Fold x)
        {
            Unify(x.Start);
            Unify(x.Collection);

            // Creating new context
            var replacesCopy = new Dictionary<string, string>(replaces);

            replaces[x.ItemName] = "i"; // NextId();
            replaces[x.AccName] ="a";//NextId();
            x.AccName = replaces[x.AccName];
            x.ItemName = replaces[x.ItemName];

            Unify(x.Func);

            replaces = replacesCopy;
        }

        public void Unify(If0 x)
        {
            Unify(x.cond);
            Unify(x.falseExpr);
            Unify(x.trueExpr);
        }

        public void Unify(Unary x)
        {
            Unify(x.Arg);
        }
        public void Unify(Var x)
        {
            if (!replaces.ContainsKey(x.Name))
            {
                replaces.Add(x.Name, NextId());
            }
            x.Name = replaces[x.Name];
        }

        private string NextId()
        {
            var old_c = counter++;
            if (old_c == 0)
            {
                return "x";
            }
            else
            {
                return string.Format("x{0}", old_c); 
            }
        }

        public void Unify(Expr e)
        {
            var realType = e.GetType();
            if (realType.IsSubclassOf(typeof (Binary)))
            {
                Unify((Binary) e);    
            }
            else if (realType == typeof (Const))
            {
                Unify((Const) e);
            }
            else if (realType == typeof (Fold))
            {
                Unify((Fold) e);
            }
            else if (realType == typeof(If0))
            {
                Unify((If0) e);
            }
            else if (realType.IsSubclassOf(typeof (Unary)))
            {
                Unify((Unary) e);
            }
            else if (realType == typeof (Var))
            {
                Unify((Var) e);
            }
            else
            {
                throw new Exception("Unknown operation in unifying:" + realType.ToString());                
            }
        }
    }
}
