using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static ShadowsFallForward.Input.PlayerInputActions;

namespace ShadowsFallForward.Input
{
    [CreateAssetMenu(fileName = "Game Input Reader", menuName = "Input/Game Input Reader")]
    public class GameInputReader : ScriptableObject, IPlayerActions
    {
        public event UnityAction<Vector2> Move = delegate { };
        public event UnityAction<Vector2, bool> Look = delegate { };
        public event UnityAction EnableMouseControlCamera = delegate { };
        public event UnityAction DisableMouseControlCamera = delegate { };
        public event UnityAction<bool> Sprint = delegate { };
        public event UnityAction Attack = delegate { };

        PlayerInputActions inputActions;

        public Vector3 Direction => inputActions.Player.Move.ReadValue<Vector2>();
        public Vector3 LookDirection => inputActions.Player.Look.ReadValue<Vector2>();

        public void Enable()
        {
            // Check if no input actions were set
            if (inputActions == null)
            {
                // Set Player Input Actions
                inputActions = new PlayerInputActions();
                inputActions.Player.SetCallbacks(this);
            }

            // Enable the input actions
            inputActions.Enable();
        }
        public void Disable() => inputActions.Disable();

        /// <summary>
        /// Check if the Mouse is being used
        /// </summary>
        private bool IsDeviceMouse(InputAction.CallbackContext context) => context.control.device.name == "Mouse";

        /// <summary>
        /// Handle Player movement
        /// </summary>
        public void OnMove(InputAction.CallbackContext context)
        {
            Move.Invoke(context.ReadValue<Vector2>());
        }

        /// <summary>
        /// Handle Player looking
        /// </summary>
        public void OnLook(InputAction.CallbackContext context)
        {
            Look.Invoke(context.ReadValue<Vector2>(), IsDeviceMouse(context));
        }

        /// <summary>
        /// Handle Player firing
        /// </summary>
        public void OnFire(InputAction.CallbackContext context)
        {
            // Exit case - if not in the Started phase
            if (context.phase != InputActionPhase.Started) return;

            Attack.Invoke();
        }

        /// <summary>
        /// Handle Player mouse camera controls
        /// </summary>
        public void OnMouseControlCamera(InputAction.CallbackContext context)
        {
            switch(context.phase)
            {
                case InputActionPhase.Started:
                    EnableMouseControlCamera.Invoke();
                    break;

                case InputActionPhase.Canceled:
                    DisableMouseControlCamera.Invoke();
                    break;
            }
        }

        /// <summary>
        /// Handle Player running
        /// </summary>
        public void OnRun(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    Sprint.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Sprint.Invoke(false);
                    break;
            }
        }
    }
}
