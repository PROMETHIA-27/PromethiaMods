using EntityStates;
using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Architect.EntityStates.Architect
{
    class Link : BaseSkillState
    {
		private const float maxTrackingDistance = 50f;
		private const float maxTrackingAngle = 20f;

		private HurtBox trackingTarget;
		private Indicator indicator;
        private HurtBox selectedNode = null;

		private static int nodeBodyIndex = BodyCatalog.FindBodyIndex("ArchitectNodeBody");
		private static GameObject huntressIndicator = Resources.Load<GameObject>("Prefabs/HuntressTrackingIndicator");
		private static GameObject connectionEffect = Resources.Load<GameObject>("@Architect:Assets/Prefabs/ConnectionEdge.prefab");

		private ArchitectData arData;

		public override void OnEnter()
		{
			base.OnEnter();

			arData = gameObject.GetComponent<ArchitectData>();

			if (isAuthority)
				indicator = new Indicator(gameObject, huntressIndicator);
		}

		public override void OnExit()
		{
			if (isAuthority)
				indicator.active = false;

			base.OnExit();
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();

			if (isAuthority)
			{
				if (selectedNode)
				{
					trackingTarget = SearchForTargetNode();
					if (inputBank.skill3.justPressed)
					{
						ConnectTargets(selectedNode, gameObject.GetComponent<ModelLocator>().modelTransform.GetComponent<HurtBoxGroup>().mainHurtBox);

						skillLocator.GetSkill(SkillSlot.Utility).DeductStock(1);

						outer.SetNextStateToMain();

						return;
					}
					if (trackingTarget)
					{
						if (!indicator.active)
							indicator.active = true;
						indicator.targetTransform = trackingTarget.transform;
						if (inputBank.skill1.justPressed)
						{
							ConnectTargets(selectedNode, trackingTarget);

							skillLocator.GetSkill(SkillSlot.Utility).DeductStock(1);

							outer.SetNextStateToMain();

							return;
						}
					}
					else if (indicator.active)
					{
						indicator.active = false;
					}
				}
				else
				{
					trackingTarget = SearchForSourceNode();
					if (trackingTarget)
					{
						if (!indicator.active)
						{
							indicator.active = true;
						}
						indicator.targetTransform = trackingTarget.transform;
						if (inputBank.skill1.justPressed)
						{
							selectedNode = trackingTarget;
						}
					}
					else if (indicator.active)
					{
						indicator.active = false;
					}
				}

				if (inputBank.skill2.justReleased)
				{
					outer.SetNextStateToMain();
				}
			}
		}

		private HurtBox SearchForSourceNode()
		{
			var extraDist = 0f;
			Ray ray = CameraRigController.ModifyAimRayIfApplicable(GetAimRay(), gameObject, out extraDist);
			BullseyeSearch bullseyeSearch = new BullseyeSearch();
			bullseyeSearch.searchOrigin = ray.origin;
			bullseyeSearch.searchDirection = ray.direction;
			bullseyeSearch.maxDistanceFilter = maxTrackingDistance + extraDist;
			bullseyeSearch.maxAngleFilter = maxTrackingAngle;
			bullseyeSearch.teamMaskFilter = TeamMask.none;
			bullseyeSearch.teamMaskFilter.AddTeam(GetTeam());
			bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
			bullseyeSearch.RefreshCandidates();
			bullseyeSearch.FilterOutGameObject(gameObject);
			return bullseyeSearch.GetResults().FirstOrDefault(x => x.healthComponent.body.bodyIndex == nodeBodyIndex);
		}

		private HurtBox SearchForTargetNode()
		{
			var extraDist = 0f;
			Ray ray = CameraRigController.ModifyAimRayIfApplicable(GetAimRay(), gameObject, out extraDist);
			BullseyeSearch bullseyeSearch = new BullseyeSearch();
			bullseyeSearch.searchOrigin = ray.origin;
			bullseyeSearch.searchDirection = ray.direction;
			bullseyeSearch.maxDistanceFilter = maxTrackingDistance + extraDist;
			bullseyeSearch.maxAngleFilter = maxTrackingAngle;
			bullseyeSearch.teamMaskFilter = TeamMask.none;
			bullseyeSearch.teamMaskFilter.AddTeam(GetTeam());
			bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
			bullseyeSearch.RefreshCandidates();
			bullseyeSearch.FilterOutGameObject(gameObject);
			bullseyeSearch.FilterOutGameObject(selectedNode.healthComponent.gameObject);
			return bullseyeSearch.GetResults().FirstOrDefault(x => x.healthComponent.body.bodyIndex == nodeBodyIndex && x.healthComponent.GetComponent<NodeData>().form == NodeForm.Program);
		}

		private void ConnectTargets(HurtBox provider, HurtBox program)
		{
			var provData = provider.GetComponent<NodeData>();
			var progData = program.GetComponent<NodeData>();

			provData.ConnectTo(progData, arData.color);

			program.healthComponent.body.RecalculateStats();
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}
	}
}
