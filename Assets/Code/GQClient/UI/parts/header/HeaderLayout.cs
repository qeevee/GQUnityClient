﻿using System;
using System.Collections;
using System.Collections.Generic;
using GQ.Client.Conf;
using GQ.Client.Err;
using UnityEngine;
using UnityEngine.UI;

namespace GQ.Client.UI
{
    public class HeaderLayout : LayoutConfig
    {
        public GameObject Header;
        public GameObject MiddleButton; 

        public override void layout()
        {
            setHeader();
        }

        protected virtual void setHeader()
        {
            if (Header == null)
            {
                Log.SignalErrorToDeveloper("Header is null.");
                return;
            }

            // set background color:
            Image image = Header.GetComponent<Image>();
            if (image != null)
            {
                image.color = ConfigurationManager.Current.headerBgColor;
            }

            setMiddleButton();

            LayoutElement layElem = Header.GetComponent<LayoutElement>();
            if (layElem == null)
            {
                Log.SignalErrorToDeveloper("LayoutElement for Header is null.");
                return;
            }

            float height = Units2Pixels(HeaderHeightUnits);
            Debug.Log(("Setting the header height to: " + height).Green());
            SetLayoutElementHeight(layElem, height);
        }

        protected virtual void setMiddleButton()
        {
            if (MiddleButton == null)
                return;

            // show top logo and load image:
            Transform middleTopLogo = MiddleButton.transform.Find("TopLogo");

            if (middleTopLogo != null)
            {
                middleTopLogo.gameObject.SetActive(true);
                Image mtlImage = middleTopLogo.GetComponent<Image>();
                if (mtlImage != null)
                {
                    Config cf = ConfigurationManager.Current;
                    mtlImage.sprite = Resources.Load<Sprite>(cf.topLogo.path);
                }
            }
        }

    }
}
