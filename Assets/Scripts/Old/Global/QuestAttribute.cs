using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System;
using System.Linq;
using System.Text;
using GQ.Geo;
using GQ.Util;
using UnitySlippyMap;

[System.Serializable]
public class QuestAttribute {

	public string key;
	public string value;

	public QuestAttribute () {

	}

	public QuestAttribute (string k, string v) {

		key = k;
		value = v;
	}
	
}