using EntityStates;
using RoR2;

namespace Architect.EntityStates.Architect
{
    class ScopeShift : BaseSkillState
    {
        const float duration = 0.1f;

        private ArchitectData arData;

        public override void OnEnter()
        {
            base.OnEnter();

            arData = gameObject.GetComponent<ArchitectData>();
        }

        public override void OnExit()
        {
            base.OnExit();

            switch (arData.element)
            {
                case NodeElement.Mass:
                    arData.element = NodeElement.Design;
                    break;
                case NodeElement.Design:
                    arData.element = NodeElement.Blood;
                    break;
                case NodeElement.Blood:
                    arData.element = NodeElement.Mass;
                    break;
            }
            Chat.AddMessage(arData.element.ToString());
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= duration && isAuthority)
                outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
