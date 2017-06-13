﻿using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using GQ.Client.Model;
using GQ.Client.Util;
using GQ.Client.Err;

namespace GQTests.Model
{

	public class ActionDecrementVariableTest : XMLTest
	{

		[SetUp]
		public void Init ()
		{
			XmlRoot = "action";
		}

		[Test]
		public void DecExistingIntegerVar ()
		{
			// Arrange:
			SetVariableAction actSetVar = parseXML<SetVariableAction> 
				(@"	<action type=""SetVariable"" var=""x"">
						<value>
							<num>10</num>
						</value>
					</action>");
			
			Variables.ClearAll ();
			actSetVar.Execute ();
			Assert.AreEqual (10, Variables.GetValue ("x").AsInt ()); 

			// Act:
			DecrementVariableAction actDecVar = parseXML<DecrementVariableAction> 
				(@"	<action type=""DecrementVariable"" var=""x""/>");
			actDecVar.Execute ();

			// Assert:
			Assert.AreEqual (Value.Type.Integer, Variables.GetValue ("x").GetType ());
			Assert.AreEqual (9, Variables.GetValue ("x").AsInt ()); 
		}

		[Test]
		public void UndefinedVar ()
		{
			// Arrange:
			Variables.ClearAll ();
			Assert.AreEqual (Value.Null, Variables.GetValue ("x")); 

			// Act:
			DecrementVariableAction actDecVar = parseXML<DecrementVariableAction> 
				(@"	<action type=""DecrementVariable"" var=""x""/>");
			actDecVar.Execute ();

			// Assert:
			Assert.AreEqual (Value.Type.Integer, Variables.GetValue ("x").GetType ());
			Assert.AreEqual (-1, Variables.GetValue ("x").AsInt ()); 
		}

		[Test]
		public void DecBoolVar ()
		{
			// Arrange:
			SetVariableAction actSetVarF = parseXML<SetVariableAction> 
				(@"	<action type=""SetVariable"" var=""f"">
						<value>
							<bool>false</bool>
						</value>
					</action>");
			SetVariableAction actSetVarT = parseXML<SetVariableAction> 
				(@"	<action type=""SetVariable"" var=""t"">
						<value>
							<bool>true</bool>
						</value>
					</action>");
			
			Variables.ClearAll ();
			actSetVarF.Execute ();
			actSetVarT.Execute ();
			Assert.IsFalse (Variables.GetValue ("f").AsBool ()); 
			Assert.IsTrue (Variables.GetValue ("t").AsBool ()); 

			// Act:
			DecrementVariableAction actDecVarF = parseXML<DecrementVariableAction> 
				(@"	<action type=""DecrementVariable"" var=""f""/>");
			DecrementVariableAction actDecVarT = parseXML<DecrementVariableAction> 
				(@"	<action type=""DecrementVariable"" var=""t""/>");
			actDecVarF.Execute ();
			actDecVarT.Execute ();

			// Assert:
			Assert.AreEqual (Value.Type.Bool, Variables.GetValue ("f").GetType ());
			Assert.IsFalse (Variables.GetValue ("f").AsBool (), "Decrementing a bool var 'false' should keep the value 'false'"); 
			Assert.AreEqual (Value.Type.Bool, Variables.GetValue ("t").GetType ());
			Assert.IsFalse (Variables.GetValue ("t").AsBool (), "Decrementing a bool var 'true' should change value to 'false'"); 
		}

		[Test]
		public void DecDoubleVar ()
		{
			// Arrange:
			SetVariableAction actSetVar = parseXML<SetVariableAction> 
				(@"	<action type=""SetVariable"" var=""x"">
						<value>
							<num>10.05</num>
						</value>
					</action>");

			Variables.ClearAll ();
			actSetVar.Execute ();
			Assert.That (Values.NearlyEqual (10.05d, Variables.GetValue ("x").AsDouble ())); 

			// Act:
			DecrementVariableAction actDecVar = parseXML<DecrementVariableAction> 
				(@"	<action type=""DecrementVariable"" var=""x""/>");
			actDecVar.Execute ();

			// Assert:
			Assert.AreEqual (Value.Type.Float, Variables.GetValue ("x").GetType ());
			Assert.That (Values.NearlyEqual (9.05, Variables.GetValue ("x").AsDouble ())); 
		}


		[Test]
		public void DecStringVar ()
		{
			// Arrange:
			SetVariableAction actSetVar = parseXML<SetVariableAction> 
				(@"	<action type=""SetVariable"" var=""x"">
						<value>
							<string>Hallo</string>
						</value>
					</action>");

			Variables.ClearAll ();
			actSetVar.Execute ();
			Assert.AreEqual (Value.Type.Text, Variables.GetValue ("x").GetType ());
			Assert.AreEqual ("Hallo", Variables.GetValue ("x").AsString ()); 

			// Act:
			DecrementVariableAction actDecVar = parseXML<DecrementVariableAction> 
				(@"	<action type=""DecrementVariable"" var=""x""/>");
			actDecVar.Execute ();

			// Assert:
			Assert.AreEqual (Value.Type.Text, Variables.GetValue ("x").GetType ());
			Assert.AreEqual ("Halln", Variables.GetValue ("x").AsString ()); 
		}

		[Test]
		public void EmbeddedInRule ()
		{
			// Arrange:
			XmlRoot = "rule";
			Rule rule = parseXML<Rule> 
				(@"	<rule>
						<action type=""SetVariable"" var=""x"">
							<value>
								<num>10</num>
							</value>
						</action>
						<action type=""DecrementVariable"" var=""x""/>
					</rule>");
			Variables.ClearAll ();
			Assert.AreEqual (Value.Null, Variables.GetValue ("x")); 

			// Act:
			rule.Apply ();

			// Assert:
			Assert.AreEqual (Value.Type.Integer, Variables.GetValue ("x").GetType ());
			Assert.AreEqual (9, Variables.GetValue ("x").AsInt ());
		}


	}
}