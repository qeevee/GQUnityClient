﻿// 
// KeywordTextDocument.cs
// 
// Copyright (c) 2014, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains a basic KeywordCollection class for use with HyperText.
// This class simply looks for delimited values in a TextAsset.

using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Candlelight
{
	/// <summary>
	/// A text document containing keywords for automatic generation of HyperText links.
	/// </summary>
	public class KeywordsTextDocument : KeywordCollection
	{
		/// <summary>
		/// An enum to specify how keywords are delimited in the text document.
		/// </summary>
		public enum DelimiterValue { Comma, LineBreak }

		#region Backing Fields
		[SerializeField, PropertyBackingField]
		private TextAsset m_TextDocument;
		[SerializeField, PropertyBackingField]
		private DelimiterValue m_Delimiter = DelimiterValue.LineBreak;
		#endregion
		/// <summary>
		/// Gets or sets the delimiter.
		/// </summary>
		/// <value>The delimiter.</value>
		public DelimiterValue Delimiter
		{
			get { return m_Delimiter; }
			set
			{
				if (m_Delimiter != value)
				{
					m_Delimiter = value;
					RebuildKeywords();
				}
			}
		}
		/// <summary>
		/// Gets or sets the text document.
		/// </summary>
		/// <value>The text document.</value>
		public TextAsset TextDocument
		{
			get { return m_TextDocument; }
			set
			{
				if (m_TextDocument != value)
				{
					m_TextDocument = value;
					RebuildKeywords();
				}
			}
		}
		
		/// <summary>
		/// Populates the supplied keyword list.
		/// </summary>
		/// <param name="keywordList">An empty keyword list.</param>
		protected override void PopulateKeywordList(List<string> keywordList)
		{
			string delimiter = m_Delimiter == DelimiterValue.Comma ? "," : "\n";
			if (m_TextDocument != null)
			{
				keywordList.AddRange(
					new List<string>(new System.Text.RegularExpressions.Regex(delimiter).Split(m_TextDocument.text))
				);
			}
		}
	}
}