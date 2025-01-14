﻿using Code.GQClient.Err;
using Code.GQClient.Model.actions;
using Code.GQClient.Model.expressions;
using Code.GQClient.Util;
using NUnit.Framework;

namespace GQTests.Model
{

	public class ActionSetVariableTest : GQMLTest
	{

		[SetUp]
		public void Init ()
		{
			XmlRoot = "action";
		}

		[Test]
		public void SetNewVariable ()
		{
			// Arrange:
			ActionSetVariable action = parseXML<ActionSetVariable> 
				(@"	<action type=""SetVariable"" var=""x"">
						<value>
							<num>20.23</num>
						</value>
					</action>");
			
			Variables.Clear ();
			Assert.AreEqual (Value.Null, Variables.GetValue ("x")); 

			// Act:
			action.Execute ();

			// Assert:
			Assert.AreNotEqual (Value.Null, Variables.GetValue ("x"));
			Assert.That (Values.NearlyEqual (20.23d, Variables.GetValue ("x").AsDouble ()));
		}

		[Test]
		public void OverwriteExistingVariable ()
		{
			// Arrange:
			ActionSetVariable action1 = parseXML<ActionSetVariable> 
				(@"	<action type=""SetVariable"" var=""x"">
						<value>
							<bool>true</bool>
						</value>
					</action>");
			
			ActionSetVariable action2 = parseXML<ActionSetVariable> 
				(@"	<action type=""SetVariable"" var=""x"">
						<value>
							<string>Hallo</string>
						</value>
					</action>");
			
			Variables.Clear ();
			action1.Execute ();
			Assert.AreEqual (Value.Type.Bool, Variables.GetValue ("x").ValType);
			Assert.AreEqual (true, Variables.GetValue ("x").AsBool ());

			// Act:
			action2.Execute ();

			// Assert:
			Assert.AreEqual (Value.Type.Text, Variables.GetValue ("x").ValType);
			Assert.AreEqual ("Hallo", Variables.GetValue ("x").AsString ());
		}

		[Test]
		public void NameMayNotStartWithDollar ()
		{
			// Arrange:
			// $ may not be used as start of a varibale name:
			ActionSetVariable action1 = parseXML<ActionSetVariable> 
				(@"	<action type=""SetVariable"" var=""$x"">
						<value>
							<bool>true</bool>
						</value>
					</action>");

			Variables.Clear ();
			Assert.AreEqual (Value.Null, Variables.GetValue ("$x")); 

			// Act:
			action1.Execute ();

			// Assert:
			Assert.AreEqual ("Variable Name may not start with '$' Symbol, so you may not use $x as you did in a SetVariable action.", Log.GetLastProblem ().Message);
		}

		[Test]
		public void SetVariableWithEmptyString ()
		{
			// Arrange:
			ActionSetVariable action = parseXML<ActionSetVariable> 
				(@"	<action type=""SetVariable"" var=""x"">
						<value>
							<string></string>
						</value>
					</action>");

			Variables.Clear ();
			Assert.AreEqual (Value.Null, Variables.GetValue ("x")); 

			// Act:
			action.Execute ();

			// Assert:
			Assert.AreNotEqual (Value.Null, Variables.GetValue ("x"));
			Assert.AreEqual ("", Variables.GetValue ("x").AsString ());
		}

		[Test]
		public void SetVariableWithEmptyStringTag ()
		{
			// Arrange:
			ActionSetVariable action = parseXML<ActionSetVariable> 
				(@"	<action type=""SetVariable"" var=""x"">
						<value>
							<string/>
						</value>
					</action>");

			Variables.Clear ();
			Assert.AreEqual (Value.Null, Variables.GetValue ("x")); 

			// Act:
			action.Execute ();

			// Assert:
			Assert.AreNotEqual (Value.Null, Variables.GetValue ("x"));
			Assert.AreEqual ("", Variables.GetValue ("x").AsString ());
		}

	}
}
