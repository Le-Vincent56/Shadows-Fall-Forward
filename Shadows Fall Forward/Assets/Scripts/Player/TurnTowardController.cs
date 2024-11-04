using ShadowsFallForward.Camera;
using ShadowsFallForward.Utilities.Vectors;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ShadowsFallForward.Player
{
    public class TurnTowardController : MonoBehaviour
    {
        [SerializeField, Required] private CameraController controller;
        [SerializeField] private float turnSpeed = 50f;

        private Transform tr;
        private float currentYRotation;
        private const float fallOffAngle = 90f;

        private void Start()
        {
            // Get components
            tr = transform;

            // Get the current y rotation
            currentYRotation = tr.localEulerAngles.y;
        }

        private void LateUpdate()
        {
            // Get the camera's forward direction and flatten it on the horizontal plane
            Vector3 cameraForward = controller.GetFacingDirection();

            // Remove the vertical component to avoid tilting the character
            cameraForward.y = 0;

            // Exit case - if the magnitude is too small
            if (cameraForward.sqrMagnitude < 0.001f) return;

            // Calculate the angle difference between the current forward direction and the velocity's forward direction
            float angleDifference = VectorMathUtils.GetAngle(tr.forward, cameraForward.normalized, tr.parent.up);

            // Determine the step size for rotation
            // - Determine the direction to rotate in
            // - Then inverse Lerp to gradually reduce the angle speed as it approaches the falloff angle
            float step = Mathf.Sign(angleDifference) 
                * Mathf.InverseLerp(0f, fallOffAngle, Mathf.Abs(angleDifference)) 
                * Time.deltaTime * turnSpeed;

            // Check if the step is larger than the angle difference,
            // if so, then rotate by the angle difference,
            // otherwise, rotate by the step
            currentYRotation += (Mathf.Abs(step) > Mathf.Abs(angleDifference)) ? angleDifference : step;

            // Apply the rotation to the transform
            tr.localRotation = Quaternion.Euler(0f, currentYRotation, 0f);
        }
    }
}
