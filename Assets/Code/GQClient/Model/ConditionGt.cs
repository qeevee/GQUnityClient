﻿using UnityEngine;
using System.Collections;

namespace GQ.Client.Model
{

	public class ConditionGt : ComparingCondition
	{

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

			return firstVal.IsGreaterThan (secondVal);
		}
	
	}
}
