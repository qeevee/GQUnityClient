﻿using System.Xml;
using Code.GQClient.Model.expressions;

namespace Code.GQClient.Model.conditions
{

    public class ConditionGeq : ComparingCondition
	{
        public ConditionGeq(XmlReader reader) : base(reader) { }

        protected override bool isFulfilledEmptyComparison ()
		{
			return true;
		}

		protected override bool isFulfilledCompare (IExpression expression)
		{
			return true;
		}

		protected override bool isFulfilledCompare (IExpression firstExpression, IExpression secondExpression)
		{
			Value firstVal = firstExpression.Evaluate ();
			Value secondVal = secondExpression.Evaluate ();
			return firstVal.IsGreaterOrEqual (secondVal);
		}
	}
}
