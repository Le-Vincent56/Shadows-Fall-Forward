using UnityEngine;

namespace ShadowsFallForward.Player.Control
{
    [RequireComponent(typeof(PlayerController))]
    public class AnimatorController : MonoBehaviour
    {
        private PlayerController controller;
        private Animator animator;

        private readonly int SpeedHash = Animator.StringToHash("Speed");

        private void Start()
        {
            // Get components
            controller = GetComponent<PlayerController>();
            animator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            animator.SetFloat(SpeedHash, controller.GetMovementVelocity().magnitude);
        }
    }
}
