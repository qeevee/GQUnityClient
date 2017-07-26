﻿using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace GQ.Client.Model
{
	public class ActionList : I_GQML {

		#region Structure 

		/// <summary>
		/// The contained actions.
		/// </summary>
		public List<IAction> containedActions = new List<IAction> ();

		public I_GQML Parent { get; set; }

		public Quest Quest {
			get {
				return Parent.Quest;
			}
		}

		#endregion


		#region Functions

		public void Apply ()
		{
			foreach (var action in containedActions) {
				action.Execute ();
			}
		}

		#endregion



	}
}