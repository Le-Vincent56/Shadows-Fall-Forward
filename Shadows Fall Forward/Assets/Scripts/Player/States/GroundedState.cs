using ShadowsFallForward.Player.Control;

namespace ShadowsFallForward.Player.States
{
    public class GroundedState : PlayerState
    {
        public GroundedState(PlayerController controller) : base(controller)
        {
        }

        public override void OnEnter()
        {
            controller.Land();
        }
    }
}
