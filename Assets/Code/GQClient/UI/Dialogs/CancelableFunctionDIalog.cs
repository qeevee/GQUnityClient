﻿using System;
using UnityEngine;

namespace Code.GQClient.UI.Dialogs
{
    public class CancelableFunctionDialog : DialogBehaviour
    {
        private string title { get; set; }
        private string message { get; set; }
        private string doButtonText { get; set; }
        private string cancelText { get; set; }
        private Action doFunction { get; set; }

        /// <summary>
        /// Static convenience method that creates a dialog object and starts it.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="cancelableFunction"></param>
        /// <param name="doButtonText"></param>
        /// <param name="cancelText"></param>
        public static void Show(
            string title,
            string message,
            Action cancelableFunction,
            string doButtonText = "Ok",
            string cancelText = "Abbrechen"
        )
        {
            CancelableFunctionDialog dialog = new CancelableFunctionDialog(
                title,
                message,
                cancelableFunction,
                doButtonText,
                cancelText
            );
            dialog.Start();
        }

        public CancelableFunctionDialog(
            string title,
            string message,
            Action cancelableFunction,
            string doButtonText = "Ok",
            string cancelText = "Abbrechen"
        ) : base(null) // 'null' because we do NOT connect a Task, sice cancel dialogs only rely on user interaction

        {
            this.title = title;
            this.message = message;
            this.doButtonText = doButtonText;
            this.cancelText = cancelText;
            this.doFunction = cancelableFunction;
        }

        public override void Start()
        {
            base.Start();

            Dialog.Title.text = title;
            Dialog.Img.gameObject.SetActive(false);
            Dialog.Details.text = message;
            Dialog.SetYesButton(cancelText, CloseDialog);
            Dialog.SetNoButton(
                doButtonText,
                (GameObject sender, EventArgs e) =>
                    {
                        doFunction();
                        CloseDialog(sender, e);
                    });

            // show the dialog:
            Dialog.Show();
        }
    }
}
