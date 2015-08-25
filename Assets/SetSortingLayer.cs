﻿using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SetSortingLayer : MonoBehaviour {
	public Renderer MyRenderer;
	public string MySortingLayer;
	public int MySortingOrderInLayer;
	
	// Use this for initialization
	void Start () {
		if (MyRenderer == null)
			MyRenderer = this.GetComponent<Renderer>();
	}
	
	// Update is called once per frame
	void Update () {
		if (MyRenderer == null)
			MyRenderer = this.GetComponent<Renderer>();
		MyRenderer.sortingLayerName = MySortingLayer;
		MyRenderer.sortingOrder = MySortingOrderInLayer;
		
		//Debug.Log(MyRenderer.sortingLayerName + " " + MyRenderer.sortingOrder);
	}   
}