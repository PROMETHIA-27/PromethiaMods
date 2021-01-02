using EntityStates;
using RoR2;
using UnityEngine;
using System.Linq;
using R2API;
using UnityEngine.Networking;

namespace Architect.EntityStates.Architect
{
    class Overlay : BaseState
    {
  //      private const float maxTrackingDistance = 50f;
  //      private const float maxTrackingAngle = 20f;
  //      private HurtBox trackingTarget;
		//private Indicator indicator;
		//private HurtBox selectedNode = null;
		//private static int nodeBodyIndex = BodyCatalog.FindBodyIndex("ArchitectNodeBody");
		//private static GameObject huntressIndicator = Resources.Load<GameObject>("Prefabs/HuntressTrackingIndicator");
		//private static GameObject connectionEffect = Resources.Load<GameObject>("@Architect:Assets/Prefabs/ConnectionEdge.prefab");

  //      public override void OnEnter()
  //      {
		//	base.OnEnter();
			
		//	if (isAuthority)
		//		indicator = new Indicator(gameObject, huntressIndicator);
  //      }

  //      public override void OnExit()
  //      {
		//	if (isAuthority)
		//		indicator.active = false;

  //          base.OnExit();
  //      }

  //      public override void FixedUpdate()
  //      {
  //          base.FixedUpdate();

		//	if (isAuthority)
  //          {
		//		if (selectedNode)
		//		{
		//			trackingTarget = SearchForTarget();
		//			if (inputBank.skill3.justReleased)
		//			{
		//				ConnectTargets(selectedNode, gameObject.GetComponent<ModelLocator>().modelTransform.GetComponent<HurtBoxGroup>().mainHurtBox, false);

		//				skillLocator.GetSkill(SkillSlot.Utility).DeductStock(1);

		//				outer.SetNextStateToMain();

		//				return;
		//			}
		//			if (trackingTarget)
		//			{
		//				if (!indicator.active)
		//					indicator.active = true;
		//				indicator.targetTransform = trackingTarget.transform;
		//				if (inputBank.skill1.justPressed)
		//				{
		//					bool isHostile = trackingTarget.healthComponent.GetComponent<TeamComponent>().teamIndex == TeamIndex.Monster;

		//					ConnectTargets(selectedNode, trackingTarget, isHostile);

		//					skillLocator.GetSkill(SkillSlot.Utility).DeductStock(1);

		//					outer.SetNextStateToMain();

		//					return;
		//				}
		//			}
		//			else if (indicator.active)
		//			{
		//				indicator.active = false;
		//			}
		//		}
		//		else
		//		{
		//			trackingTarget = SearchForNodeTarget();
		//			if (trackingTarget)
		//			{
		//				if (!indicator.active)
		//				{
		//					indicator.active = true;
		//				}
		//				indicator.targetTransform = trackingTarget.transform;
		//				if (inputBank.skill1.justPressed)
		//				{
		//					selectedNode = trackingTarget;
		//				}
		//			}
		//			else if (indicator.active)
		//			{
		//				indicator.active = false;
		//			}
		//		}

		//		if (inputBank.skill2.justReleased)
		//		{
		//			outer.SetNextStateToMain();
		//		}
		//	}
  //      }

		//private HurtBox SearchForNodeTarget()
  //      {
		//	var extraDist = 0f;
		//	Ray ray = CameraRigController.ModifyAimRayIfApplicable(GetAimRay(), gameObject, out extraDist);
		//	BullseyeSearch bullseyeSearch = new BullseyeSearch();
		//	bullseyeSearch.searchOrigin = ray.origin;
		//	bullseyeSearch.searchDirection = ray.direction;
		//	bullseyeSearch.maxDistanceFilter = maxTrackingDistance + extraDist;
		//	bullseyeSearch.maxAngleFilter = maxTrackingAngle;
		//	bullseyeSearch.teamMaskFilter = TeamMask.none;
		//	bullseyeSearch.teamMaskFilter.AddTeam(TeamIndex.Player);
		//	bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
		//	bullseyeSearch.RefreshCandidates();
		//	bullseyeSearch.FilterOutGameObject(gameObject);
		//	var result = bullseyeSearch.GetResults().FirstOrDefault();
		//	if (result && result.healthComponent.GetComponent<CharacterBody>().bodyIndex != nodeBodyIndex)
		//		result = null;
		//	return result;
		//}

		//private HurtBox SearchForTarget()
		//{
		//	var extraDist = 0f;
		//	Ray ray = CameraRigController.ModifyAimRayIfApplicable(GetAimRay(), gameObject, out extraDist);
		//	BullseyeSearch bullseyeSearch = new BullseyeSearch();
		//	bullseyeSearch.searchOrigin = ray.origin;
		//	bullseyeSearch.searchDirection = ray.direction;
		//	bullseyeSearch.maxDistanceFilter = maxTrackingDistance + extraDist;
		//	bullseyeSearch.maxAngleFilter = maxTrackingAngle;
		//	bullseyeSearch.teamMaskFilter = TeamMask.none;
		//	bullseyeSearch.teamMaskFilter.AddTeam(TeamIndex.Player);
		//	bullseyeSearch.teamMaskFilter.AddTeam(TeamIndex.Monster);
		//	bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
		//	bullseyeSearch.RefreshCandidates();
		//	bullseyeSearch.FilterOutGameObject(gameObject);
		//	bullseyeSearch.FilterOutGameObject(selectedNode.healthComponent.gameObject);
		//	return bullseyeSearch.GetResults().FirstOrDefault();
		//}

		//private void ConnectTargets(HurtBox sourceNode, HurtBox target, bool targetIsEnemy = false)
		//{
		//	var line = Object.Instantiate(connectionEffect).GetComponent<LineBetweenTransforms>();

		//	NetworkServer.Spawn(line.gameObject);

		//	line.transformNodes = new Transform[] { sourceNode.transform, target.transform };

		//	var srcBod = sourceNode.healthComponent.GetComponent<CharacterBody>();
		//	var tarBod = target.healthComponent.GetComponent<CharacterBody>();

		//	var tarIsNode = tarBod.bodyIndex == nodeBodyIndex;

		//	var connection = line.GetComponent<ConnectionEdge>();
		//	connection.Negative = targetIsEnemy;
		//	connection.Init(
		//		new ConnectionEdge.NodeInfo()
		//		{
		//			body = srcBod,
		//			health = sourceNode.healthComponent
		//		},
		//		new ConnectionEdge.NodeInfo()
		//		{
		//			body = tarBod,
		//			health = target.healthComponent
		//		}
		//	);

		//	if (ConnectionEdge.ConnectionCount(srcBod, ConnectionEdge.Direction.Outgoing) > 1)
		//		ConnectionEdge.RemoveFirstConnection(srcBod, ConnectionEdge.Direction.Outgoing);
		//	if (ConnectionEdge.ConnectionCount(tarBod, ConnectionEdge.Direction.Incoming) > (tarIsNode ? 3 : 1))
		//		ConnectionEdge.RemoveFirstConnection(tarBod, ConnectionEdge.Direction.Incoming);

		//	if (ConnectionEdge.ConnectionExists(tarBod, srcBod))
		//		ConnectionEdge.RemoveConnection(tarBod, srcBod);

		//	tarBod.RecalculateStats();
  //      }

  //      public override InterruptPriority GetMinimumInterruptPriority()
  //      {
  //          return InterruptPriority.PrioritySkill;
  //      }
    }
}
