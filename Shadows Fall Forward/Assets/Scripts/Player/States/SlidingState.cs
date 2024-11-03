using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShadowsFallForward.Player.States
{
    public class SlidingState : PlayerState
    {
        public SlidingState(PlayerController controller) : base(controller)
        {
        }

        public override void OnEnter()
        {
            controller.OnGroundContactLost();
        }
    }
}
