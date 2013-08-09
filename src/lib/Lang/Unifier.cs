using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace lib.Lang
{
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

            // Creating new cotext
            var replacesCopy = new Dictionary<string, string>(replaces);

            replaces[x.ItemName] = NextId();
            replaces[x.AccName] = NextId();
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
            if (realType == typeof (Binary))
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
            else if (realType == typeof (Unary))
            {
                Unify((Unary) e);
            }
            else if (realType == typeof (Var))
            {
                Unify((Var) e);
            }
            else
            {
                throw new Exception("Unknown operation in unifying");                
            }
        }


        [Test]
        [TestCase("(fold x_27005 0 (lambda (x_27005 x_27006) (shr1 (if0 x_27005 (if0 (plus (shl1 (shr1 0)) x_27005) x_27005 x_27006) x_27005))))",
            "(fold x_27005 0 (lambda (x_27005 x_27006) (shr1 (if0 x_27005 (if0 (plus (shl1 (shr1 0)) x_27005) x_27005 x_27006) x_27005))))")]
        public void Unify_Test(string expected, string got)
        {
            Assert.AreEqual(Expr.ParseExpr(expected).ToSExpr(), Expr.ParseExpr(got).GetUnified().ToSExpr());            
        }

    }
}
