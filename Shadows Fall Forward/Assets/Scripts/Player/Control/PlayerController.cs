using ShadowsFallForward.Input;
using ShadowsFallForward.Patterns.StateMachine;
using ShadowsFallForward.Player.States;
using ShadowsFallForward.Utilities.Vectors;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace ShadowsFallForward.Player.Control
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField, Required] GameInputReader input;

        private Transform tr;
        private PlayerMover mover;
        CeilingDetector ceilingDetector;

        [SerializeField] private float movementSpeed = 7f;
        [SerializeField] private float airControlRate;
        [SerializeField] private float groundFriction = 100f;
        [SerializeField] private float airFriction = 0.5f;
        [SerializeField] private float gravity = 30f;
        [SerializeField] private float slideGravity = 5f;
        [SerializeField] private float slopeLimit = 30f;
        [SerializeField] private bool useLocalMomentum;

        private StateMachine stateMachine;

        [SerializeField] private Transform cameraTransform;
        private Vector3 momentum;
        private Vector3 savedVelocity;
        private Vector3 savedMovementVelocity;
        private Vector3 movementDirection;

        public event Action<Vector3> OnLand = delegate { };

        private void Awake()
        {
            // Get components
            tr = transform;
            mover = GetComponent<PlayerMover>();
            ceilingDetector = GetComponent<CeilingDetector>();

            // Set up the Player State Machine
            SetupStateMachine();
        }

        private void Start()
        {
            // Enable Player input
            input.Enable();
        }

        private void Update()
        {
            // Update the State Machine
            stateMachine.Update();
        }

        private void FixedUpdate()
        {
            // Update the State Machine
            stateMachine.FixedUpdate();

            // Check for the ground
            mover.CheckForGround();

            // Handle Player momentum
            CalculateMomentum();

            // Calculate the velocity
            Vector3 velocity = stateMachine.GetState() is GroundedState 
                ? CalculateMovementVelocity()
                : Vector3.zero;

            // Add momentum to the velocity
            velocity += useLocalMomentum ? tr.localToWorldMatrix * momentum : momentum;

            // Extend the Sensor range as the Player will always be grounded
            mover.SetExtendSensorRange(true);
            mover.SetVelocity(velocity);

            // Set the saved velocity
            savedVelocity = velocity;
            savedMovementVelocity = CalculateMovementVelocity();
        }

        /// <summary>
        /// Set up the Playe State Machine
        /// </summary>
        private void SetupStateMachine()
        {
            // Create the State Machine
            stateMachine = new StateMachine();

            // Initialize states
            GroundedState groundedState = new GroundedState(this);
            SlidingState slidingState = new SlidingState(this);
            FallingState fallingState = new FallingState(this);

            // Define transitions
            stateMachine.At(groundedState, slidingState, new FuncPredicate(() => mover.IsGrounded() && IsGroundTooSteep()));
            stateMachine.At(groundedState, fallingState, new FuncPredicate(() => !mover.IsGrounded()));

            stateMachine.At(slidingState, groundedState, new FuncPredicate(() => mover.IsGrounded() && !IsGroundTooSteep()));
            stateMachine.At(slidingState, fallingState, new FuncPredicate(() => !mover.IsGrounded()));

            stateMachine.At(fallingState, groundedState, new FuncPredicate(() => mover.IsGrounded() && !IsGroundTooSteep()));
            stateMachine.At(fallingState, slidingState, new FuncPredicate(() => mover.IsGrounded() && IsGroundTooSteep()));

            // Set an initial state
            stateMachine.SetState(fallingState);
        }

        /// <summary>
        /// Check whether or not the Player is grounded
        /// </summary>
        private bool IsGrounded() => stateMachine.GetState() is GroundedState or SlidingState;

        /// <summary>
        /// Checkk if the ground beneath the Player is too steep
        /// </summary>
        private bool IsGroundTooSteep() => Vector3.Angle(mover.GetGroundNormal(), tr.up) > slopeLimit;

        /// <summary>
        /// Get the momentum of the Player
        /// </summary>
        public Vector3 GetMomentum() => useLocalMomentum ? tr.localToWorldMatrix * momentum : momentum;

        /// <summary>
        /// Get the movement velocity of the Player
        /// </summary>
        public Vector3 GetMovementVelocity() => savedMovementVelocity;

        /// <summary>
        /// Get the direction of movement of the Player
        /// </summary>
        public Vector3 GetMovementDirection() => movementDirection;

        /// <summary>
        /// Calculate Player momentum
        /// </summary>
        private void CalculateMomentum()
        {
            // Check if using local momentum
            if (useLocalMomentum) 
                // If so, translate from world space to local space
                momentum = tr.localToWorldMatrix * momentum;

            // Get the vertical momentum of the Player by extracting the
            // vertical component of the vector
            Vector3 verticalMomentum = VectorMathUtils.ExtractDotVector(momentum, tr.up);

            // Get the horizontal momentum by subtracting the total momentum by the extracted
            // vertical momentum
            Vector3 horizontalMomentum = momentum - verticalMomentum;

            // Apply gravity to the vertical momentum
            verticalMomentum -= tr.up * (gravity * Time.deltaTime);

            // Check if grounded and still applying a negative vertical force to the Player
            if(stateMachine.GetState() is GroundedState && VectorMathUtils.GetDotProduct(verticalMomentum, tr.up) < 0f)
            {
                verticalMomentum = Vector3.zero;
            }

            // Check if not grounded
            if (!IsGrounded())
            {
                // Adjust the horizontal momentum
                AdjustHorizontalMomentum(ref horizontalMomentum, CalculateMovementVelocity());
            }

            // Check if sliding
            if (stateMachine.GetState() is SlidingState)
            {
                // Handle sliding
                HandleSliding(ref horizontalMomentum);
            }

            // Introduce friction
            float friction = stateMachine.GetState() is GroundedState ? groundFriction : airFriction;
            horizontalMomentum = Vector3.MoveTowards(horizontalMomentum, Vector3.zero, friction * Time.deltaTime);

            // Calculate the final momentum vector
            momentum = horizontalMomentum + verticalMomentum;

            // Check if the Player is sliding
            if(stateMachine.GetState() is SlidingState)
            {
                // Project the horizontal momentum ont othe plane described by the ground normal
                momentum = Vector3.ProjectOnPlane(momentum, mover.GetGroundNormal());

                // Check if upward momentum exists that's positive
                if (VectorMathUtils.GetDotProduct(momentum, tr.up) > 0f)
                    // Remove upward momentum
                    VectorMathUtils.RemoveDotVector(momentum, tr.up);

                // Calculate the slide direction by projecting the downward vector onto the ground plane
                Vector3 slideDirection = Vector3.ProjectOnPlane(-tr.up, mover.GetGroundNormal().normalized);

                // Adjust the momentum using gravity
                momentum += slideDirection * (slideGravity * Time.deltaTime);
            }

            // Check if using local momentum
            if (useLocalMomentum)
                // If so, translate from local space to world space
                momentum = tr.worldToLocalMatrix * momentum;
        }

        /// <summary>
        /// Adjust the horizontal momentum for when the Player is not grounded
        /// </summary>
        private void AdjustHorizontalMomentum(ref Vector3 horizontalMomentum, Vector3 movementVelocity)
        {
            // Check if the horizontal momentum's magnitude is greater than the movement speed
            if(horizontalMomentum.magnitude > movementSpeed)
            {
                // Check if the movement velocity has any component towards the current momentum
                if(VectorMathUtils.GetDotProduct(movementVelocity, horizontalMomentum.normalized) > 0f)
                {
                    // If so, remove the overlapping component from the movement velocity
                    movementVelocity = VectorMathUtils.RemoveDotVector(movementVelocity, horizontalMomentum.normalized);
                }

                // Add the movement velocity scaled by the air control air
                horizontalMomentum += movementVelocity * (Time.deltaTime * airControlRate * 0.25f);
            } else
            {
                // Add the movement velocity scaled by the air control air
                horizontalMomentum += movementVelocity * (Time.deltaTime * airControlRate);

                // Clamp the horizontal momentum's magnitude to the movement speed
                horizontalMomentum = Vector3.ClampMagnitude(horizontalMomentum, movementSpeed);
            }
        }

        /// <summary>
        /// Handle horizontal momentum with sliding
        /// </summary>
        private void HandleSliding(ref Vector3 horizontalMomentum)
        {
            // Calculate the direction of the slope's descent
            Vector3 pointDownVector = Vector3.ProjectOnPlane(mover.GetGroundNormal(), tr.up).normalized;

            // Get the Player's movement velocity and remove the point down vector from it
            // to prevent excessive movement due to input
            Vector3 movementVelocity = CalculateMovementVelocity();
            movementVelocity = VectorMathUtils.RemoveDotVector(movementVelocity, pointDownVector);

            // Add the velocity to the horizontal momentum
            horizontalMomentum += movementVelocity * Time.fixedDeltaTime;
        }

        /// <summary>
        /// Calculate the Player's movement velocity
        /// </summary>
        private Vector3 CalculateMovementVelocity() => CalculateMovementDirection() * movementSpeed;

        /// <summary>
        /// Calculate the Player's movement direction
        /// </summary>
        private Vector3 CalculateMovementDirection()
        {
            // Get the direction based on if there's a camera or not
            Vector3 direction = cameraTransform == null
                ? tr.right * input.Direction.x + tr.forward * input.Direction.y
                : Vector3.ProjectOnPlane(cameraTransform.right, tr.up).normalized * input.Direction.x +
                  Vector3.ProjectOnPlane(cameraTransform.forward, tr.up).normalized * input.Direction.y;

            movementDirection = direction.magnitude > 1f ? direction.normalized : direction;

            // Return the direction, normalizing it if its magnitude is greater than 1
            return movementDirection;
        }

        /// <summary>
        /// Handle the Player's velocity when losing ground contact
        /// </summary>
        public void OnGroundContactLost()
        {
            // Check if using local momentum
            if (useLocalMomentum)
                // If so, translate from world space to local space
                momentum = tr.localToWorldMatrix * momentum;

            // Get the movement velocity
            Vector3 velocity = GetMovementVelocity();

            // Check if both the velocity and the momentum are non-zero
            if(velocity.sqrMagnitude >= 0f && momentum.sqrMagnitude > 0f)
            {
                // Project the momentum onto the direction of the velocity to find the component
                // of momentum aligned with the direction of velocity
                Vector3 projectedMomentum = Vector3.Project(momentum, velocity.normalized);

                // Get the dot product of the projected momentum and the velocity
                float dot = VectorMathUtils.GetDotProduct(projectedMomentum.normalized, velocity.normalized);

                // Check if the projected momentum's magnitude and the velocity's magnitude are the same
                // and the dot product is positive
                if (projectedMomentum.sqrMagnitude >= velocity.sqrMagnitude && dot > 0f)
                    // Set the velocity to zero, preventing additional acceleration in the same direction
                    // of the momentum
                    velocity = Vector3.zero;
                // Check if they're not equal, but the dot product is positive
                else if (dot > 0f)
                    // Subtract the projected momentum from the velocity to slow the Player's movement
                    // in that direction
                    velocity -= projectedMomentum;
            }

            // Add the velocity to the momentum
            momentum += velocity;

            // Check if using local momentum
            if(useLocalMomentum)
                // If so, translate from local space to world space
                momentum = tr.worldToLocalMatrix * momentum;
        }

        /// <summary>
        /// Handle when the Player begins to fall
        /// </summary>
        public void OnFallStart()
        {
            // Get the upward momentum
            Vector3 currentUpMomentum = VectorMathUtils.ExtractDotVector(momentum, tr.up);

            // Remove the upward momentum from the current momentum
            momentum = VectorMathUtils.RemoveDotVector(momentum, tr.up);

            // Substract the current up momentum's magnitude from the momentum
            momentum -= tr.up * currentUpMomentum.magnitude;
        }

        /// <summary>
        /// Land the Player
        /// </summary>
        public void Land()
        {
            // Get the collision velocity
            Vector3 collisionVelocity = useLocalMomentum ? tr.localToWorldMatrix * momentum : momentum;

            // Invoke the land event using the collision velocity
            OnLand.Invoke(collisionVelocity);
        }
    }
}
