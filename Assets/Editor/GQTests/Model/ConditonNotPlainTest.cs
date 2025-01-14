﻿using Code.GQClient.Err;
using Code.GQClient.Model.conditions;
using NUnit.Framework;

namespace GQTests.Model
{

	public class ConditonNotPlainTest : GQMLTest
	{

		[SetUp]
		public void Init ()
		{
			XmlRoot = "not";
		}

		[Test]
		public void Empty ()
		{
			// Act:
			ConditionNot condition = parseXML<ConditionNot> 
				(@"	<not>
					</not>");

			// Assert:
			Assert.IsNotNull (condition);
			Assert.IsFalse (condition.IsFulfilled ());
			Assert.AreEqual (ConditionNot.NOT_CONDITION_PROBLEM_EMPTY, Log.GetLastProblem ().Message);
		}

		[Test]
		public void Single_True ()
		{
			// Act:
			ConditionNot condition = parseXML<ConditionNot> 
				(@"	<not>
						<eq>
							<bool>true</bool>
							<bool>false</bool>
						</eq>
					</not>");

			// Assert:
			Assert.IsNotNull (condition);
			Assert.IsTrue (condition.IsFulfilled ());
		}

		[Test]
		public void Single_False ()
		{
			// Act:
			ConditionNot condition = parseXML<ConditionNot> 
				(@"	<not>
						<lt>
							<num>10</num>
							<num>10.8</num>
						</lt>
					</not>");

			// Assert:
			Assert.IsNotNull (condition);
			Assert.IsFalse (condition.IsFulfilled ());
		}

		[Test]
		public void TwoAtomicCond_NotAllowed ()
		{
			// Act:
			ConditionNot condition = parseXML<ConditionNot> 
				(@"	<not>
						<eq>
							<bool>true</bool>
							<bool>true</bool>
						</eq>
						<eq>
							<bool>true</bool>
							<bool>false</bool>
						</eq>
					</not>");

			// Assert:
			Assert.IsNotNull (condition);
			Assert.IsFalse (condition.IsFulfilled ());
			Assert.AreEqual (ConditionNot.NOT_CONDITION_PROBLEM_TOO_MANY_ATOMIC_CONIDITIONS, Log.GetLastProblem ().Message);
		}

		[Test]
		public void FourAtomicCond_NotAllowed ()
		{
			// Act:
			ConditionNot condition = parseXML<ConditionNot> 
				(@"	<not>
						<lt>
							<num>110</num>
							<num>10.8</num>
						</lt>
						<lt>
							<num>110</num>
							<num>10.8</num>
						</lt>
						<eq>
							<bool>false</bool>
							<bool>true</bool>
						</eq>
						<eq>
							<bool>true</bool>
							<bool>false</bool>
						</eq>
					</not>");

			// Assert:
			Assert.IsNotNull (condition);
			Assert.IsFalse (condition.IsFulfilled ());
			Assert.AreEqual (ConditionNot.NOT_CONDITION_PROBLEM_TOO_MANY_ATOMIC_CONIDITIONS, Log.GetLastProblem ().Message);
		}

	}
}
