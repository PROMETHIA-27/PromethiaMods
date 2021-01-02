using EntityStates;
using RoR2;

namespace Architect.EntityStates.ArchitectNode
{
    class NodePrimaryWeapon : BaseSkillState
    {
        float baseDuration = 1f;
        float duration;

        protected NodeData data;

        public override void OnEnter()
        {
            base.OnEnter();

            duration = baseDuration / attackSpeedStat;

            data = GetComponent<NodeData>();
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
            return InterruptPriority.Skill;
        }

        protected float GetOrbRange()
        {
            return 30 + (data.GetMatchingInputCount(OutputColor.Blue, NodeElement.Design, 1) * 10f);
        }

        protected float GetOrbDamage(HurtBox target)
        {
            var multiplier = data.GetMatchingInputCount(OutputColor.Green, NodeElement.Design, 1);
            const float damageMod = 0.25f;
            if (!target.healthComponent.body.isBoss && !target.healthComponent.body.isElite)
                multiplier = 0;
            return characterBody.damage * (1 + (damageMod * multiplier));
        }
    }
}
