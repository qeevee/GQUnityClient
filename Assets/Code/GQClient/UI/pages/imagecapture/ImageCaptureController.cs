﻿//#define DEBUG_LOG

using System;
using System.Collections;
using System.IO;
using Code.GQClient.Conf;
using Code.GQClient.Err;
using Code.GQClient.FileIO;
using Code.GQClient.Model.expressions;
using Code.GQClient.Model.gqml;
using Code.GQClient.Model.mgmt.quests;
using Code.GQClient.Model.pages;
using Code.GQClient.Util;
using GQClient.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.GQClient.UI.pages.imagecapture
{
    public class ImageCaptureController : PageController
    {
        #region Inspector Features

        public TextMeshProUGUI text;
        public Image textbg;

        public Button button;

        bool camIsRotated;
        WebCamTexture cameraTexture;
        public RawImage camRawImage;

        #endregion


        #region Runtime API

        protected PageImageCapture myPage;

        /// <summary>
        /// Is called during Start() of the base class, which is a MonoBehaviour.
        /// </summary>
        public override void InitPage_TypeSpecific()
        {
            myPage = (PageImageCapture) page;

            // show the task and button:
            if (myPage.Task != null && myPage.Task != "")
            {
                text.text = myPage.Task.Decode4TMP(false);

                /*  Adjust the text size after, because it is rotated by 
                    -90 degrees relative to its parent, but it should be 
                    same position: */
                Rect textRect = text.rectTransform.rect;
                Vector2 textSizeDelta = text.rectTransform.sizeDelta;
                text.rectTransform.sizeDelta = new Vector2(
                    textRect.height - textRect.width,
                    textRect.width - textRect.height
                );
            }
            else
            {
                text.gameObject.SetActive(false);
            }

            // init web cam;
            Base.Instance.StartCoroutine(initWebCam());
        }

        IEnumerator initWebCam()
        {
            string deviceName = null;

#if DEBUG_LOG
            Debug.Log("ImageCapture: ## 1");
#endif
            // look for a cam in the preferred direction:
            foreach (WebCamDevice wcd in WebCamTexture.devices)
            {
                if (wcd.isFrontFacing == myPage.PreferFrontCam)
                {
                    deviceName = wcd.name;
                    break;
                }
            }
#if DEBUG_LOG
            Debug.Log("ImageCapture: ## 2");
#endif
            // if we did not find a right cam, we use the first cam available:
            if (deviceName == null && WebCamTexture.devices.Length > 0)
            {
                deviceName = WebCamTexture.devices[0].name;
                Log.SignalErrorToUser(
                    "Your device does not offer a {0} camera, os we can only use what we get: the default camera.",
                    myPage.PreferFrontCam ? "front" : "rear"
                );
            }

            cameraTexture = new WebCamTexture(deviceName, 3000, 2000);

#if DEBUG_LOG
            Debug.Log("ImageCapture: ## 3");
#endif

            if (cameraTexture == null) Debug.Log("Cam Texture is null");

            cameraTexture.Play();

#if DEBUG_LOG
            Debug.Log("ImageCapture: ## 4");
#endif

            // wait for web cam to be ready which is guaranteed after first image update:
            while (!cameraTexture.didUpdateThisFrame)
            {
                // if we did not have permission, the user had now the chance to give permission and we can proceed.
                if (!cameraTexture.isPlaying)
                {
#if DEBUG_LOG
                    Debug.Log("ImageCapture: ## 5: not playing");
#endif
                    cameraTexture = new WebCamTexture(deviceName, 3000, 2000);
                    cameraTexture.Play();
                }
                else
                {
                    // the user did obviously not give us permission, hence we need to skip this page or even quest TODO
                    Log.SignalErrorToUser(
                        "Without permission to access the camera we can not proceed correctly with the current quest.");
                }

                yield return null;
            }

#if DEBUG_LOG
            Debug.Log("ImageCapture: ## 6: has updated and seems to be running");
#endif

            // rotate if needed:
            camRawImage.transform.rotation *= Quaternion.AngleAxis(cameraTexture.videoRotationAngle, Vector3.back);

            camIsRotated = Math.Abs(cameraTexture.videoRotationAngle) == 90 ||
                           Math.Abs(cameraTexture.videoRotationAngle) == 270;

            float camHeight = (camIsRotated ? cameraTexture.width : cameraTexture.height);
            float camWidth = (camIsRotated ? cameraTexture.height : cameraTexture.width);

            float panelHeight = camRawImage.rectTransform.rect.height;
            float panelWidth = camRawImage.rectTransform.rect.width;

            float heightScale = panelHeight / camHeight;
            float widthScale = panelWidth / camWidth;
            float fitScale = Math.Min(heightScale, widthScale);

            float goalHeight = cameraTexture.height * fitScale;
            float goalWidth = cameraTexture.width * fitScale;

            heightScale = goalHeight / panelHeight;
            widthScale = goalWidth / panelWidth;

            float mirrorAdjustment = cameraTexture.videoVerticallyMirrored ? -1F : 1F;
            // TODO adjust mirror also correct if cam is not rotated:

            camRawImage.transform.localScale = new Vector3(widthScale, heightScale * mirrorAdjustment, 1F);

            camRawImage.texture =
                cameraTexture; // TODO evtl. auf zwischen speicherung verzichten und direkt in camRawImage.texture anlegen?
        }

        public void TakeSnapshot()
        {
            Texture2D photo;

            // we add 360 degrees to avoid any negative values:
            var rotatedClockwiseQuarters = 360 - cameraTexture.videoRotationAngle;

            switch (Input.deviceOrientation)
            {
                case DeviceOrientation.LandscapeLeft:
                    rotatedClockwiseQuarters += 90;
                    break;
                case DeviceOrientation.PortraitUpsideDown:
                    rotatedClockwiseQuarters += 180;
                    break;
                case DeviceOrientation.LandscapeRight:
                    rotatedClockwiseQuarters += 270;
                    break;
                case DeviceOrientation.Unknown:
                case DeviceOrientation.Portrait:
                case DeviceOrientation.FaceUp:
                case DeviceOrientation.FaceDown:
                default:
                    break;
            }

            rotatedClockwiseQuarters /= 90; // going from degrees to quarters
            rotatedClockwiseQuarters %= 4; // reducing to 0, 1 ,2 or 3 quarters

            cameraTexture.Pause();
            var pixels = cameraTexture.GetPixels();
            cameraTexture.Stop();

            switch (rotatedClockwiseQuarters)
            {
                case 1:
                    photo = new Texture2D(cameraTexture.height, cameraTexture.width);
                    photo.SetPixels(pixels.Rotate90(cameraTexture.height, cameraTexture.width));
                    break;
                case 2:
                    photo = new Texture2D(cameraTexture.width, cameraTexture.height);
                    photo.SetPixels(pixels.Rotate180(cameraTexture.width, cameraTexture.height));
                    break;
                case 3:
                    photo = new Texture2D(cameraTexture.height, cameraTexture.width);
                    photo.SetPixels(pixels.Rotate270(cameraTexture.height, cameraTexture.width));
                    break;
                case 0:
                default:
                    photo = new Texture2D(cameraTexture.width, cameraTexture.height);
                    photo.SetPixels(pixels);
                    break;
            }

            photo.Apply();

            SaveTextureToCamera(photo);

            OnForward();
        }

        void SaveTextureToCamera(Texture2D texture)
        {
            var filename = Quest.GetRuntimeMediaFileName(".jpg");
            var filepath = Files.CombinePath(QuestManager.GetRuntimeMediaPath(myPage.Quest.Id), filename);

            var bytes = texture.EncodeToJPG();

            File.WriteAllBytes(filepath, bytes);
            Variables.SetVariableValue(myPage.File, new Value(filename));

            // save media info for local file under the pseudo variable (e.g. @_imagecapture):
            string relDir = Files.CombinePath(QuestInfoManager.QuestsRelativeBasePath, myPage.Quest.Id.ToString(), "runtime");
            myPage.Quest.MediaStore[GQML.PREFIX_RUNTIME_MEDIA + myPage.File] =
                new MediaInfo(
                    myPage.Quest.Id,
                    GQML.PREFIX_RUNTIME_MEDIA + myPage.File,
                    relDir, 
                    filename
                );
 
            // TODO save to mediainfos.json again
            
            NativeGallery.Permission permission = NativeGallery.RequestPermission(NativeGallery.PermissionType.Write);
            if (permission == NativeGallery.Permission.Denied)
            {
                if (NativeGallery.CanOpenSettings())
                {
                    NativeGallery.OpenSettings();
                }
            }

            permission = NativeGallery.SaveImageToGallery(texture, Config.Current.name, filename);
            Destroy(texture); // avoid memory leaks
        }

        #endregion
    }
}