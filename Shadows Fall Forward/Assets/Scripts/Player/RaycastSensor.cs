using UnityEngine;

namespace ShadowsFallForward.Player
{
    public class RaycastSensor
    {
        public enum CastDirection { Forward, Right, Up, Backward, Left, Down }

        public float castLength = 0.1f;
        public LayerMask layerMask = 255;

        Vector3 origin = Vector3.zero;
        Transform tr;

        CastDirection castDirection;
        RaycastHit hitInfo;

        public RaycastSensor(Transform playerTransform)
        {
            tr = playerTransform;
        }

        public void Cast()
        {
            Vector3 worldOrigin = tr.TransformPoint(origin);
            Vector3 worldDirection = GetCastDirection();

            // Cast the ray
            Physics.Raycast(
                worldOrigin, 
                worldDirection, 
                out hitInfo, 
                castLength, 
                layerMask, 
                QueryTriggerInteraction.Ignore
            );
        }

        /// <summary>
        /// Set the direction in which to cast rays
        /// </summary>
        public void SetCastDirection(CastDirection direction) => castDirection = direction;

        /// <summary>
        /// Set the origin of the raycast
        /// </summary>
        public void SetCastOrigin(Vector3 pos) => origin = tr.InverseTransformPoint(pos);

        /// <summary>
        /// Get the raycast direction based on the CastDirection enum
        /// </summary>
        Vector3 GetCastDirection()
        {
            return castDirection switch
            {
                CastDirection.Forward => tr.forward,
                CastDirection.Right => tr.right,
                CastDirection.Up => tr.up,
                CastDirection.Backward => -tr.forward,
                CastDirection.Left => -tr.right,
                CastDirection.Down => -tr.up,
                _ => Vector3.one
            };
        }

        /// <summary>
        /// Get whether or not a hit was detected
        /// </summary>
        public bool HasDetectedHit() => hitInfo.collider != null;

        /// <summary>
        /// Get the distance of the hit from the ray's origin
        /// </summary>
        public float GetDistance() => hitInfo.distance;

        /// <summary>
        /// Get the normal of the hit
        /// </summary>
        public Vector3 GetNormal() => hitInfo.normal;

        /// <summary>
        /// Get the point that the raycast hit
        /// </summary>
        public Vector3 GetPosition() => hitInfo.point;

        /// <summary>
        /// Get the Collider of the hit object
        /// </summary>
        public Collider GetCollider() => hitInfo.collider;

        /// <summary>
        /// Get the Transform of the hit object
        /// </summary>
        public Transform GetTransform() => hitInfo.transform;
    }
}
