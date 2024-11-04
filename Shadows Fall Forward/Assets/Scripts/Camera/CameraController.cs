using ShadowsFallForward.Input;
using Sirenix.OdinInspector;
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

        [SerializeField] private float cameraSpeed = 50f;
        [SerializeField] private bool smoothCameraRotation;
        [SerializeField][Range(1f, 50f)] private float cameraSmoothingFactor = 25f;

        private Transform tr;

        private void Awake()
        {
            // Get components
            tr = transform;

            // Set the current angles
            currentXAngle = tr.localRotation.eulerAngles.x;
            currentYAngle = tr.localRotation.eulerAngles.y;
        }

        private void Update()
        {
            RotateCamera(input.LookDirection.x, -input.LookDirection.y);
        }

        /// <summary>
        /// Handle rotating the camera
        /// </summary>
        private void RotateCamera(float horizontalInput, float verticalInput)
        {
            // Check if to smooth the camera rotation
            if(smoothCameraRotation)
            {
                // Lerp the inputs
                horizontalInput = Mathf.Lerp(0, horizontalInput, Time.deltaTime * cameraSmoothingFactor);
                verticalInput = Mathf.Lerp(0, verticalInput, Time.deltaTime * cameraSmoothingFactor);
            }

            // Update the camera angles
            currentXAngle += verticalInput * cameraSpeed * Time.deltaTime;
            currentYAngle += horizontalInput * cameraSpeed * Time.deltaTime;

            // Clamp the x-angle to ensure it stays within the vertical limits
            currentXAngle = Mathf.Clamp(currentXAngle, -upperVerticalLimit, lowerVerticalLimit);

            // Set the rotation
            tr.localRotation = Quaternion.Euler(currentXAngle, currentYAngle, 0);
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
