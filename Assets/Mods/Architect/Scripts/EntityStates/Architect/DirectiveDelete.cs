using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace Architect.EntityStates.Architect
{
    class DirectiveDelete : BaseSkillState
    {
        const float duration = 0.1f;

        public override void OnEnter()
        {
            base.OnEnter();

            //Projectile
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
