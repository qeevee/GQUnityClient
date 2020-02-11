﻿using UnityEngine;

namespace Code.UnitySlippyMap.Helpers
{
    public class CameraFacingBillboard : MonoBehaviour
    {
        private Transform myTransform;
        public Vector3 Axis = Vector3.back;

        void Start()
        {
            myTransform = this.transform;
        }

        void Update()
        {
            Transform cameraTransform = Camera.main.transform;
            myTransform.LookAt(myTransform.position + cameraTransform.rotation * Axis,
                cameraTransform.rotation * Vector3.up);
        }
    }
}