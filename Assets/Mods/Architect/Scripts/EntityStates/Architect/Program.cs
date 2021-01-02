﻿using EntityStates;

namespace Architect.EntityStates.Architect
{
    class Program : BaseSkillState
    {
        public override void OnEnter()
        {
            base.OnEnter();


        }

        public override void OnExit()
        {


            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
