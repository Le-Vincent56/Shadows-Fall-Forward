using ShadowsFallForward.Patterns.StateMachine;
using ShadowsFallForward.Player.Control;

namespace ShadowsFallForward.Player.States
{
    public class PlayerState : IState
    {
        protected readonly PlayerController controller;

        public PlayerState(PlayerController controller)
        {
            this.controller = controller;
        }

        public virtual void OnEnter() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void OnExit() { }
    }
}
