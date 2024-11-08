using ShadowsFallForward.Player.Control;

namespace ShadowsFallForward.Player.States
{
    public class FallingState : PlayerState
    {
        public FallingState(PlayerController controller) : base(controller)
        {
        }

        public override void OnEnter()
        {
            controller.OnFallStart();
        }
    }
}
