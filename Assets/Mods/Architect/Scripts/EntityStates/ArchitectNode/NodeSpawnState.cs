using EntityStates;
using RoR2;

namespace Architect.EntityStates.ArchitectNode
{
    class NodeSpawnState : GenericCharacterSpawnState
    {
		const float totalDuration = 1f;

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

			if (fixedAge >= totalDuration && isAuthority)
			{
				outer.SetNextStateToMain();
				return;
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}

		public NodeSpawnState() : base() { }
	}
}
