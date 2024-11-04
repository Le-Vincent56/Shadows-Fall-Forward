using Sirenix.OdinInspector;
using UnityEngine;

namespace ShadowsFallForward
{
    public class CameraDistanceRaycaster : MonoBehaviour
    {
        [SerializeField, Required] private Transform cameraTransform;
        [SerializeField, Required] private Transform cameraTargetTransform;

        private LayerMask layerMask = Physics.AllLayers; // Initialize with all layers
        [SerializeField] private float minimumDistanceFromObstacles = 0.1f;
        [SerializeField] private float smoothingFactor = 25f;

        private Transform tr;
        private float currentDistance;

        private void Awake()
        {
            // Get components
            tr = transform;

            // Exluce the "Ignore Raycast" layer from the LayerMask
            layerMask &= ~(1 << LayerMask.NameToLayer("Ignore Raycast"));

            // Set an initial curent distance
            currentDistance = (cameraTargetTransform.position - tr.position).magnitude;
        }

        private void LateUpdate()
        {
            // Get the direction from the camera's position to the target's position
            Vector3 castDirection = cameraTargetTransform.position - tr.position;

            // Get the distance
            float distance = GetCameraDistance(castDirection);

            // Interpolate the current distance to the calculated distance
            currentDistance = Mathf.Lerp(currentDistance, distance, Time.deltaTime * smoothingFactor);

            // Set the camera's position based on the new current distance and the direction to the target
            cameraTransform.position = tr.position + castDirection.normalized * currentDistance;
        }

        /// <summary>
        /// Calculate the distance from the Camera to the Camera Target
        /// </summary>
        private float GetCameraDistance(Vector3 castDirection)
        {
            // Calculate the full distance from the camera's position to
            // the target's position
            float distance = castDirection.magnitude + minimumDistanceFromObstacles;

            float sphereRadius = 0.5f;
            if (Physics.SphereCast(
                new Ray(tr.position, castDirection), 
                sphereRadius, 
                out RaycastHit hit, 
                distance, 
                layerMask, 
                QueryTriggerInteraction.Ignore))
            {
                // Calculate the distance to the obstacle subtracted by the minimum distance buffer,
                // use Mathf.Max() to prevent negative values
                return Mathf.Max(0f, hit.distance - minimumDistanceFromObstacles);
            }

            // If nothing was hit, return the full distance to the target
            return castDirection.magnitude;
        }
    }
}
