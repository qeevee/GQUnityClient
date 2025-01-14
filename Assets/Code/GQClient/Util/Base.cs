﻿// #define DEBUG_LOG

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Code.GQClient.Conf;
using Code.GQClient.Err;
using Code.GQClient.UI;
using Code.GQClient.UI.author;
using Code.GQClient.UI.Dialogs;
using Code.GQClient.UI.map;
using Code.GQClient.UI.Progress;
using Code.GQClient.Util.http;
using Code.GQClient.Util.tasks;
using Code.QM.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.GQClient.Util
{
    public class Base : MonoBehaviour
    {
        #region Inspector Global Values

        public GameObject ListCanvas;
        public GameObject TopicTreeCanvas;
        public GameObject MapCanvas;
        public OnlineMaps Map;
        public GameObject MenuCanvas;
        public GameObject DialogCanvas;
        public GameObject ProgressCanvas;

        public Transform canvasRootT;

        private GameObject _partnersCanvas;

        public GameObject partnersCanvas
        {
            get
            {
                if (_partnersCanvas == null)
                {
                    var obj = Resources.Load("ImportedPackage/prefabs/PartnersCanvas");
                    if (obj == null)
                        return null;
                    _partnersCanvas = (GameObject) Instantiate(obj, canvasRootT);
                }

                return _partnersCanvas;
            }
        }

        public Canvas imprintCanvas;
        public Canvas feedbackCanvas;
        public Canvas privacyCanvas;
        public Canvas authorCanvas;

        public GameObject MenuTopLeftContent;
        public Canvas canvas4TopLeftMenu;
        public GameObject MenuTopRightContent;
        public Canvas canvas4TopRightMenu;

        public GameObject InteractionBlocker;

        #endregion


        #region Singleton

        public const string BASE = "Base";

        private static Base _instance = null;

        public static Base Instance
        {
            get
            {
                if (_instance == null)
                {
                    var baseGo = GameObject.Find(BASE);

                    if (baseGo == null)
                    {
                        baseGo = new GameObject(BASE);
                        Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
                    }

                    if (baseGo.GetComponent(typeof(Base)) == null)
                        baseGo.AddComponent(typeof(Base));

                    // initialize the instance:
                    _instance = (Base) baseGo.GetComponent(typeof(Base));
                    if (_instance.ProgressCanvas != null)
                    {
                        _instance.ProgressCanvas.SetActive(true);
                        _instance.ProgressCanvas.GetComponent<Canvas>().enabled = false;
                    }
                }

                return _instance;
            }
        }

        #endregion


        #region Foyer

        public const string FOYER_SCENE_NAME = "Foyer";

        private bool _listShown;
        private bool _mapShown;
        private bool _menuShown;
        private bool _imprintShown;

        private Dictionary<string, bool> _canvasStates;

        /// <summary>
        /// Called when we leave the foyer towards a page.
        /// </summary>
        public void HideFoyerCanvases()
        {
            StartCoroutine(HideFoyerCanvasesInCoroutine());
        }
        
        private IEnumerator HideFoyerCanvasesInCoroutine()
        {
            yield return new WaitForEndOfFrame();
            // store current show state and hide:
            var rootGOs = SceneManager.GetSceneByName(FOYER_SCENE_NAME)
                .GetRootGameObjects();
            foreach (var rootGo in rootGOs)
            {
                var canvas = rootGo.GetComponent<Canvas>();
                if (canvas != null)
                {
                    _canvasStates[canvas.name] = canvas.isActiveAndEnabled;
                    canvas.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Called when we return to the foyer from a page.
        /// </summary>
        public void ShowFoyerCanvases()
        {
            // show again according to stored state:
            var rootGOs = SceneManager.GetSceneByName(FOYER_SCENE_NAME)
                .GetRootGameObjects();
            foreach (var rootGo in rootGOs)
            {
                var canvas = rootGo.GetComponent<Canvas>();
                if (canvas != null)
                {
                    if (_canvasStates.TryGetValue(canvas.name, out _))
                    {
                        canvas.gameObject.SetActive(_canvasStates[canvas.name]);
                    }
                }
            }

            if (MapCanvas.activeInHierarchy)
            {
                FoyerMapController fmapCtrl = MapCanvas.GetComponent<FoyerMapController>();
                if (fmapCtrl != null)
                {
                    fmapCtrl.UpdateView();
                }
            }
        }

        #endregion


        #region LifeCycle

        private void Awake()
        {
            ConfigurationManager.Initialize();
            
            // hide all canvases at first, we show the needed ones in initViews()
            var rootGOs = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var rootGo in rootGOs)
            {
                var canvas = rootGo.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.gameObject.SetActive("DialogCanvas".Equals(canvas.name));
                }
            }

            DontDestroyOnLoad(Instance);
            SceneManager.sceneLoaded += SceneAdapter.OnSceneLoaded;
            _canvasStates = new Dictionary<string, bool>();
        }

        private void Start()
        {
            if (partnersCanvas != null)
                partnersCanvas.gameObject.SetActive(Config.Current.showPartnersInfoAtStart);
        }

        private void Update()
        {
            //#if UNITY_EDITOR || UNITY_STANDALONE
            if (Author.LoggedIn)
            {
                Device.updateMockedLocation();
            }

            //#endif
        }

        #endregion


        #region Global Functions

        public void BlockInteractions(bool block)
        {
            if (block)
            {
                InteractionBlocker.SetActive(true);
            }
            else
            {
                CoroutineStarter.Run(UnblockInCoroutine());
            }
        }

        private IEnumerator UnblockInCoroutine()
        {
            yield return new WaitForEndOfFrame();
            InteractionBlocker.SetActive(false);
        }

        public DownloadBehaviour GetDownloadBehaviour(AbstractDownloader downloader, string title)
        {
            switch (Config.Current.taskUI)
            {
                case TaskUIMode.Dialog:
                    return new DownloadDialogBehaviour(downloader, title);
                case TaskUIMode.ProgressAtBottom:
                    return new DownloadProgressBehaviour(downloader, title);
                default:
                    Log.SignalErrorToDeveloper("Downloader TaskUI mode {0} is unknown, using default dialog instead.",
                        Config.Current.taskUI);
                    return new DownloadDialogBehaviour(downloader, title);
            }
        }

        public SimpleBehaviour GetSimpleBehaviour(Task task, string title, string details)
        {
            switch (Config.Current.taskUI)
            {
                case TaskUIMode.Dialog:
                    return new SimpleDialogBehaviour(task, title, details);
                case TaskUIMode.ProgressAtBottom:
                    return new SimpleProgressBehaviour(task, title, details);
                default:
                    Log.SignalErrorToDeveloper("Downloader TaskUI mode {0} is unknown, using default dialog instead.",
                        Config.Current.taskUI);
                    return new SimpleDialogBehaviour(task, title, details);
            }
        }
        #endregion
    }
}