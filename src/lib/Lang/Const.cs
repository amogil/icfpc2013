using System;
using System.Collections.Generic;

namespace lib.Lang
{
	public class Const : Expr
	{
		public readonly UInt64 value;

		public Const(UInt64 value)
		{
			this.value = value;
		}

		public override UInt64 Eval(Vars vars)
		{
			return value;
		}

	    public override object Clone()
	    {
	        return new Const(value);
	    }

	    public override IEnumerable<byte> ToBinExp()
	    {
            if (value == 0 || value == 1)
            { 
	            yield return Convert.ToByte(value);
            }
            else
            {
                throw new FormatException("Unknown constant: " + value.ToString());                
            }

	    }

	    public override string ToSExpr()
		{
			return value.ToString();
		}
	}
}