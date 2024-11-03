using ShadowsFallForward.Patterns.StateMachine;

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
