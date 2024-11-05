using UnityEngine;

namespace ShadowsFallForward.Player.Control
{
    [RequireComponent(typeof(PlayerController))]
    public class AnimatorController : MonoBehaviour
    {
        private PlayerController controller;
        private Animator animator;

        private readonly int HorizontalHash = Animator.StringToHash("HorizontalDirection");
        private readonly int VerticalHash = Animator.StringToHash("VerticalDirection");

        private float currentHorizontal;
        private float currentVertical;
        [SerializeField] private float smoothTime = 0.1f;

        private void Start()
        {
            // Get components
            controller = GetComponent<PlayerController>();
            animator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            // Get the target movement direction
            Vector3 targetDirection = controller.GetMovementDirection();
            float targetHorizontal = targetDirection.x;
            float targetVertical = targetDirection.z;

            // Interpolate the current values toward the target values
            currentHorizontal = Mathf.Lerp(currentHorizontal, targetHorizontal, Time.deltaTime / smoothTime);
            currentVertical = Mathf.Lerp(currentVertical, targetVertical, Time.deltaTime / smoothTime);

            // Set the interpolated values in the animator
            animator.SetFloat(HorizontalHash, currentHorizontal);
            animator.SetFloat(VerticalHash, currentVertical);
        }
    }
}
