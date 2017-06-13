﻿using UnityEngine;
using System.Collections;
using System;
using System.Xml.Serialization;
using GQ.Client.Err;

namespace GQ.Client.Model
{

	public class NumberExpression : SimpleExpression
	{
		#region Structure

		protected override void setValue (string valueAsString)
		{
			int valueAsInt;
			if (Int32.TryParse (valueAsString, out valueAsInt)) {
				value = new Value (valueAsString, Value.Type.Integer);
				return;
			}

			double valueAsDouble;
			if (Double.TryParse (valueAsString, out valueAsDouble)) {
				value = new Value (valueAsString, Value.Type.Float);
			} else {
				value = new Value ("0", Value.Type.Integer);
				Log.WarnAuthor ("Tried to store {0} to a num typed value, but that does not work. We store 0 as Integer instead.", valueAsString);
			}
		}

		#endregion

	}
}