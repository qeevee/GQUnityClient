﻿using UnityEngine;
using System.Collections;
using GQ.Client.Model;
using System.Xml.Serialization;
using GQ.Client.Err;
using System.Xml;

namespace GQ.Client.Model
{

	public class ActionStartMission : ActionAbstract
	{
		#region State

		public int Id { get; set; }

		public bool AllowReturn { get; set;	}

		#endregion


		#region Structure

		protected override void ReadAttributes(XmlReader reader) {
			Id = GQML.GetIntAttribute (GQML.PAGE_ID, reader);
			AllowReturn = GQML.GetOptionalBoolAttribute (GQML.ACTION_STARTMISSION_ALLOWRETURN, reader, false);
		}

		#endregion


		#region Functions

		public override void Execute ()
		{
			Quest.GetPageWithID(Id).Start();
		}

		#endregion
	}
}