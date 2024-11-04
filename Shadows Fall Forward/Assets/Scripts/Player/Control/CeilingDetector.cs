using UnityEngine;

namespace ShadowsFallForward.Player.Control
{
    public class CeilingDetector : MonoBehaviour
    {
        private float ceilingAngleLimit = 10f;
        private bool isInDebugMode;

        private readonly float debugDrawDuration = 2.0f;
        private bool ceilingWasHit;

        public float CeilingAngleLimit { get => ceilingAngleLimit; set => ceilingAngleLimit = value; }
        public bool IsInDebugMode { get => isInDebugMode; set => isInDebugMode = value; }

        private void OnCollisionEnter(Collision collision) => CheckForContact(collision);
        private void OnCollisionStay(Collision collision) => CheckForContact(collision);

        /// <summary>
        /// Check for ceiling contact
        /// </summary>
        private void CheckForContact(Collision collision)
        {
            // Exit case - there are no Collision contacts
            if (collision.contacts.Length == 0) return;

            // Get the angle between the downward vector and the first collision contact normal
            float angle = Vector3.Angle(-transform.up, collision.contacts[0].normal);

            // If the angle is less than the ceiling angle limit, then
            // a ceiling was hit
            if(angle < ceilingAngleLimit)
                ceilingWasHit = true;

            // Debug if necessary
            if (isInDebugMode)
                Debug.DrawRay(collision.contacts[0].point, collision.contacts[0].normal, Color.red, debugDrawDuration);
        }

        public bool GetHitCeiling() => ceilingWasHit;
        public void Reset() => ceilingWasHit = false;
    }
}
