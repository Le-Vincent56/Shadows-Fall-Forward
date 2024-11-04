using UnityEngine;

namespace ShadowsFallForward.Player.Control
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class PlayerMover : MonoBehaviour
    {
        [Header("Collider Settings")]
        [SerializeField][Range(0f, 1f)] private float stepHeightRatio = 0.1f;
        [SerializeField] private float colliderHeight = 2f;
        [SerializeField] private float colliderThickness = 1f;
        [SerializeField] private Vector3 colliderOffset = Vector3.zero;

        private Rigidbody rb;
        private Transform tr;
        private CapsuleCollider col;
        private RaycastSensor sensor;

        [SerializeField] private bool isGrounded;
        [SerializeField] private float baseSensorRange;
        private Vector3 currentGroundAdjustmentVelocity; // Velocity to adjust player position to maintain ground contact
        private int currentLayer;

        [Header("Sensor Settings")]
        [SerializeField] private bool isInDebugMode;
        private bool isUsingExtendedSensorRange = true; // Use extended range for smoother ground transitions

        private void Awake()
        {
            Setup();
            RecalculateColliderDimensions();
        }

        private void OnValidate()
        {
            if (gameObject.activeInHierarchy)
                RecalculateColliderDimensions();
        }

        /// <summary>
        /// Set up components and initial variables 
        /// </summary>
        private void Setup()
        {
            // Get components
            tr = transform;
            rb = GetComponent<Rigidbody>();
            col = GetComponent<CapsuleCollider>();

            // Turn off rotations and gravity
            rb.freezeRotation = true;
            rb.useGravity = false;
        }

        /// <summary>
        /// Calculate collider dimensions
        /// </summary>
        private void RecalculateColliderDimensions()
        {
            // If no Collider is set, run the Setup
            if (col == null)
                Setup();

            col.height = colliderHeight * (1f - stepHeightRatio);
            col.radius = colliderThickness / 2f;
            col.center = colliderOffset * colliderHeight + new Vector3(0f, stepHeightRatio * col.height / 2f, 0f);

            // Check if the collider height divided by two is less than
            // the collider radius
            if(col.height / 2f < col.radius)
            {
                // If so, set the collider radius to be equal to the collider height
                // divided by two
                col.radius = col.height / 2f;
            }

            // Recalibrate the RaycastSensor
            RecalibrateSensor();
        }

        /// <summary>
        /// Calibrate the RaycastSensor
        /// </summary>
        private void RecalibrateSensor()
        {
            // If a RaycastSensor is not already set, create one and set it
            sensor ??= new RaycastSensor(tr);

            // Set the cast origin and direction
            sensor.SetCastOrigin(col.bounds.center);
            sensor.SetCastDirection(RaycastSensor.CastDirection.Down);

            // Recalculate the RaycastSensor mask
            RecalculateSensorLayerMask();

            // Set a factor to prevent clipping issues when the RaycastSensor range is calculated
            const float safetyDistanceFactor = 0.001f;

            // Calculate the length of the ray
            float length = colliderHeight * (1f - stepHeightRatio) * 0.5f + colliderHeight * stepHeightRatio;
            baseSensorRange = length * (1f + safetyDistanceFactor) * tr.localScale.x;
            sensor.castLength = length * tr.localScale.x;
        }

        /// <summary>
        /// Calculate the RaycastSensor's LayerMask
        /// </summary>
        private void RecalculateSensorLayerMask()
        {
            // Get the current layer
            int objectLayer = gameObject.layer;
            int layerMask = Physics.AllLayers;

            // Iterate through each Layer
            for(int i = 0; i < 32; i++)
            {
                // Check if the object Layer is set to ignore the Layer i
                if(Physics.GetIgnoreLayerCollision(objectLayer, i))
                {
                    // If so, use bit-shifting to turn the Layer off in the LayerMask
                    layerMask &= ~(1 << i);
                }
            }

            // Clear the "Ignore Raycast" Layer
            int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
            layerMask &= ~(1 << ignoreRaycastLayer);

            // Set the RaycastSensor's LayerMask and the current Layer
            sensor.layerMask = layerMask;
            currentLayer = objectLayer;
        }

        /// <summary>
        /// Check if the Player is on the ground and set their ground adjustment vector
        /// </summary>
        public void CheckForGround()
        {
            // Check if the current Layer is the same as the GameObject's Layer
            if (currentLayer != gameObject.layer)
                // If not, recalculate the RaycastSensor's LayerMask
                RecalculateSensorLayerMask();

            // Set the ground adjustment velocity to zero
            currentGroundAdjustmentVelocity = Vector3.zero;

            // Set the RaycastSensor's cast length
            sensor.castLength = isUsingExtendedSensorRange
                ? baseSensorRange + colliderHeight * tr.localScale.x * stepHeightRatio
                : baseSensorRange;

            // Cast the RaycastSensor
            sensor.Cast();

            // Check if grounded by seeing if the RaycastSensor has detected a hit
            isGrounded = sensor.HasDetectedHit();

            // Exit case - not grounded
            if (!isGrounded) return;

            // Get the distance from the RaycastSensor to the ground
            float distance = sensor.GetDistance();

            // Get the upper bound of where the Player should ideally be positioned
            float upperLimit = colliderHeight * tr.localScale.x * (1f - stepHeightRatio) * 0.5f;

            // Get the middle point: where the player's feet should be relative to the ground
            float middle = upperLimit + colliderHeight * tr.localScale.x * stepHeightRatio;

            // Get the distance to go
            float distanceToGo = middle - distance;

            // Calculate the velocity the player needs to adjust to the ground
            currentGroundAdjustmentVelocity = tr.up * (distanceToGo / Time.fixedDeltaTime);
        }

        /// <summary>
        /// Set the Player's velocity
        /// </summary>
        public void SetVelocity(Vector3 velocity) => rb.velocity = velocity + currentGroundAdjustmentVelocity;

        /// <summary>
        /// Get whether or not the Player is grounded
        /// </summary>
        public bool IsGrounded() => isGrounded;

        /// <summary>
        /// Get the normal from the Player to the Ground
        /// </summary>
        public Vector3 GetGroundNormal() => sensor.GetNormal();

        /// <summary>
        /// Set whether or not to use the extended RaycastSensor range
        /// </summary>
        public void SetExtendSensorRange(bool isExtended) => isUsingExtendedSensorRange = isExtended;
    }
}
