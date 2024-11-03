using ShadowsFallForward.Input;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShadowsFallForward.Camera
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField, Required] private GameInputReader input;

        private float currentXAngle;
        private float currentYAngle;

        [SerializeField][Range(0f, 90f)] private float upperVerticalLimit = 35f;
        [SerializeField][Range(0f, 90f)] private float lowerVerticalLimit = 35f;

        private float cameraSpeed = 50f;
        private bool smoothCameraRotation;
        [SerializeField][Range(1f, 50f)] private float cameraSmoothingFactor = 25f;

        private Transform tr;
        private UnityEngine.Camera cam;

        private void Awake()
        {
            // Get components
            tr = transform;
            cam = GetComponentInChildren<UnityEngine.Camera>();

            // Set the current angles
            currentXAngle = tr.localRotation.eulerAngles.x;
            currentYAngle = tr.localRotation.eulerAngles.y;
        }

        private void Update()
        {
            RotateCamera(input.LookDirection.x, input.LookDirection.y);
        }

        private void RotateCamera(float horizontalInput, float verticalInput)
        {

        }

        /// <summary>
        /// Get the Camera's up direction
        /// </summary>
        public Vector3 GetUpDirection() => tr.up;

        /// <summary>
        /// Get the Camera's facing direction
        /// </summary>
        public Vector3 GetFacingDirection() => tr.forward;
    }
}
