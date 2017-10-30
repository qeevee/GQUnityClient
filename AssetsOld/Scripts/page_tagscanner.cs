﻿using UnityEngine;
using UnityEngine.UI;

using System.Collections;

using System.Threading;

using ZXing;
using UnityEngine.SceneManagement;
using GQ.Client.Model;

public class page_tagscanner : MonoBehaviour
{

	public questdatabase questdb;
	public Quest quest;
	public Page tagscanner;

	public Text text;

	private const string TASKDESCRIPTION_DEFAULT = "QR Code Scannen";
	public Image textbg;
	public Text ergebnis_text;
	public Image ergebnis_textbg;
	public string qrresult = "";
	public bool showresult = false;
	WebCamTexture camTexture;

	public RawImage camQRImage;

	private Thread qrThread;
	private Color32[] c;
	//	private sbyte[] d;
	private int W, H;
	//	private int x, y, z;
	//	Material cameraMat;
	//	public MeshRenderer plane;
	private string qrcontent;
	//	public MessageReceiver receiver;

	IEnumerator Start ()
	{

		if (GameObject.Find ("QuestDatabase") == null) {

			SceneManager.LoadScene ("questlist");
			yield break;
		}

		questdb = GameObject.Find ("QuestDatabase").GetComponent<questdatabase> ();
		quest = QuestManager.Instance.CurrentQuest;
		tagscanner = QuestManager.Instance.CurrentQuest.currentpage;

		if (tagscanner.onStart != null) {

			tagscanner.onStart.Invoke ();
		}

		if (tagscanner.hasAttribute ("taskdescription")) {
			
			text.text = questdb.GetComponent<actions> ().formatString (tagscanner.getAttribute ("taskdescription"));
		} else {
			text.text = TASKDESCRIPTION_DEFAULT;
//			text.enabled = false;
//			textbg.enabled = false;
		}

		showresult = false;
		if (tagscanner.hasAttribute ("showTagContent")) {

			if (tagscanner.getAttribute ("showTagContent") == "true") {

				showresult = true;
			}
		}

		// init web cam;
		if (Application.isWebPlayer) {
			yield return Application.RequestUserAuthorization (UserAuthorization.WebCam);
		}

		string deviceName = null;
		foreach (WebCamDevice wcd in WebCamTexture.devices) {
			if (!wcd.isFrontFacing) {
				deviceName = wcd.name;
				break;
			}
		}

		camTexture = new WebCamTexture (deviceName);
		// request a resolution that is enough to scan qr codes reliably:
		camTexture.requestedHeight = 480;
		camTexture.requestedWidth = 640;

		camTexture.Play ();

		// wait for web cam to be ready which is guaranteed after first image update:
		while (!camTexture.didUpdateThisFrame)
			yield return null;

		// scale height according to camera aspect ratio:
		float xScale = 1F;
		float yScale = ((float)camTexture.height / (float)camTexture.width) * (camTexture.videoVerticallyMirrored ? -1F : 1F);

		// scale to fill:
		float fillScale = 1;
		float minHeight = ((RectTransform)camQRImage.transform.parent).rect.height;
		float minWidth = ((RectTransform)camQRImage.transform.parent).rect.width;
		float isHeight = camQRImage.rectTransform.rect.height * yScale;
		float isWidth = camQRImage.rectTransform.rect.width;
		if (minHeight > isHeight)
			fillScale = Mathf.Max (minHeight / isHeight, fillScale);
		if (minWidth > isWidth)
			fillScale = Mathf.Max (minWidth / isWidth, fillScale);
		xScale *= fillScale;
		yScale *= fillScale;
		
		// correct shown texture according to webcam details:
		camQRImage.transform.rotation *= Quaternion.AngleAxis (camTexture.videoRotationAngle, Vector3.back);
		camQRImage.transform.localScale = new Vector3 (xScale, yScale, 1F);

		Debug.Log (
			"QR Start(): Cam h: " + camTexture.height + ", w: " + camTexture.width +
			", isH: " + isHeight + " isW: " + isWidth +
			", minH: " + minHeight + " minW: " + minWidth +
			", yscale: " + yScale +
			", fillScale: " + fillScale
		);

		camQRImage.texture = camTexture;
		W = camTexture.width;
		H = camTexture.height;

		qrThread = new Thread (DecodeQR);
		qrThread.Start ();
	}

	void Update ()
	{
		if (camTexture != null && camTexture.didUpdateThisFrame) {
			c = camTexture.GetPixels32 ();
		}

		if (qrcontent != null && qrcontent != "" && qrcontent != "!XEMPTY_GEOQUEST_QRCODEX!28913890123891281283012") {
			checkResult (qrcontent);
		}
	}

	void OnDisable ()
	{
		Debug.Log ("OnDisable()");
		if (camTexture != null) {
			camTexture.Pause ();
		}
	}

	void OnDestroy ()
	{
		Debug.Log ("OnDestroy()");

		if (qrThread != null) {
			decoderRunning = false;
		}

		if (camTexture != null) {

			camTexture.Stop ();
		}
	}


	private bool decoderRunning = false;

	void DecodeQR ()
	{  
		// create a reader with a custom luminance source

		var barcodeReader = new BarcodeReader {
			AutoRotate = false,
			TryHarder = false
		};
				
		decoderRunning = true;

		while (decoderRunning) {


			try {
				string result = ""; 

				Debug.Log ("decode thread running");

				// decode the current frame

				if (c != null) {

					result = barcodeReader.Decode (c, W, H).Text; 
					Debug.Log ("THREAD: DecodeQR() ##1 result given: >" + result + "<");
				}        
				if (result != null) {           
					qrcontent = result;   

					Debug.Log ("THREAD: DecodeQR() result given: >" + result + "<");
				}
				// Sleep a little bit and set the signal to get the next frame
				c = null;
				Thread.Sleep (200); 
			} catch {   
				continue;
			}
		}
	}


	//old


	void checkResult (string r)
	{

		if (r != qrresult) {
			qrresult = r;

			if (r.Length > 0) {
				questdb.debug ("QR CODE gescannt:" + r);

				tagscanner.result = r;

				if (showresult) {
					ergebnis_text.text = r;
					ergebnis_text.enabled = true;
					ergebnis_textbg.enabled = true;
				}


				if (tagscanner.contents_expectedcode != null && tagscanner.contents_expectedcode.Count > 0) {

					bool foundCorrectResult = false;

					foreach (QuestContent qc in tagscanner.contents_expectedcode) {

						if (qc.content == r) {

							foundCorrectResult = true;

							text.enabled = false;
							textbg.enabled = false;

							break;
						}
					}

					if (foundCorrectResult) {

						onSuccess ();
					} else {

						onFailure ();
					}
				}
				StartCoroutine (onEnd ());
			}
		}
	}


	void onSuccess ()
	{

		if (tagscanner.onSuccess != null) {

			tagscanner.stateOld = "succeeded";
			tagscanner.onSuccess.Invoke ();
		}  


	}


	void onFailure ()
	{

		if (tagscanner.onFailure != null) {
		
			tagscanner.stateOld = "failed";
			tagscanner.onFailure.Invoke ();
		}  
	}



	IEnumerator  onEnd ()
	{

		yield return new WaitForSeconds (0.2f);

		if (!GQML.STATE_FAILED.Equals (tagscanner.stateOld)) {

			tagscanner.stateOld = GQML.STATE_SUCCEEDED;
		}

		if (tagscanner.onEnd != null) {
			
			tagscanner.onEnd.Invoke ();
		} else if (!tagscanner.onSuccess.hasMissionAction () && !tagscanner.onFailure.hasMissionAction ()) {

			questdb.endQuest ();
			// TODO looks like an ERROR
			Debug.LogWarning ("A QR Scan page did neither have onSucceed nor onFail change page actions, so we end the quest. Shouldn't we simply perform onEnd()?");
		} else {

			if (showresult) {

				ergebnis_text.enabled = false;
				ergebnis_textbg.enabled = false;
			}
		}
	}
}
