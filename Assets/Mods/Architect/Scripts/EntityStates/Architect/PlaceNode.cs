using EntityStates;
using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Architect.EntityStates.Architect
{
	public class PlaceNode : BaseState //Code yoinked from engi's turret placement, then fixed
	{
		public override void OnEnter()
		{
			base.OnEnter();
			if (isAuthority)
			{
				currentPlacementInfo = GetPlacementInfo(); // Get placement info for placing turret, then build a blueprint at that position
				blueprints = Object.Instantiate(blueprintPrefab, currentPlacementInfo.position, currentPlacementInfo.rotation).GetComponent<BlueprintController>();
			}
			//PlayAnimation("Gesture", "PrepTurret");
			entryCountdown = entryDelay;
			exitCountdown = exitDelay;
			exitPending = false;
			//if (modelLocator) //Create a wristdisplayobject, not sure what this is
			//{
			//	ChildLocator component = modelLocator.modelTransform.GetComponent<ChildLocator>();
			//	if (component)
			//	{
			//		Transform transform = component.FindChild("WristDisplay");
			//		if (transform)
			//		{
			//			wristDisplayObject = Object.Instantiate(wristDisplayPrefab, transform);
			//		}
			//	}
			//}
		}

		static PlaceNode()
        {
			var comp = blueprintPrefab.GetComponent<BlueprintController>();
			comp.okMaterial = Resources.Load<Material>("materials/matBlueprintsOk");
			comp.invalidMaterial = Resources.Load<Material>("materials/matBlueprintsInvalid");
        }
		
		//Return where turret would be, its rotation, and whether we can place
		private PlacementInfo GetPlacementInfo()
		{
			Ray aimRay = GetAimRay();
			if (!Physics.Raycast(aimRay, out RaycastHit hit, maxPlacementRayDistance, LayerIndex.world.mask))
				return new PlacementInfo { position = new Vector3(0, -99999, 0) };
			
			Vector3 hitNormal = hit.normal;
			Vector3 aimPoint = hit.point;
			PlacementInfo info = default(PlacementInfo);
			info.ok = false;
			Vector3 rotDir = aimRay.direction;
			rotDir.y = 0;
			rotDir.Normalize();
			info.rotation = Util.QuaternionSafeLookRotation(Vector3.ProjectOnPlane(-rotDir, hitNormal), hitNormal);
			Ray downRay = new Ray(aimPoint + hitNormal * placementMaxUp, -hitNormal);
			float totalHeightDist = placementMaxUp + placementMaxDown;
			float heightDist = totalHeightDist;
			if (Physics.SphereCast(downRay, turretRadius, out hit, totalHeightDist, LayerIndex.world.mask))
			{
				heightDist = hit.distance;
				info.ok = true;
            }
			Vector3 groundPoint = downRay.GetPoint(heightDist + turretRadius);
			info.position = groundPoint;
			if (info.ok)
            {
				float height = Mathf.Max(turretHeight, turretCenter);
				if (Physics.CheckCapsule(info.position + hitNormal * (height - turretRadius), info.position + hitNormal * turretRadius, turretRadius - 0.05f, LayerIndex.world.mask | LayerIndex.defaultLayer.mask))
                {
					info.ok = false;
                }
            }
			return info;
		}

		private void DestroyBlueprints()
		{
			if (blueprints) //Clear blueprints
			{
				Destroy(blueprints.gameObject);
				blueprints = null;
			}
		}

        public override void OnExit()
        {
            base.OnExit();
            //PlayAnimation("Gesture", "PlaceTurret"); //Animate and destroy prefab instances when done
            if (wristDisplayObject)
            {
                Destroy(wristDisplayObject);
            }
            DestroyBlueprints();
        }

		public override void Update()
		{
			base.Update();
			currentPlacementInfo = GetPlacementInfo();
			if (blueprints) //Get placement info every frame and update the state of the blueprint 
			{
				blueprints.PushState(currentPlacementInfo.position, currentPlacementInfo.rotation, currentPlacementInfo.ok);
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (isAuthority) //All fixed code is only run on the authority
			{
				entryCountdown -= Time.fixedDeltaTime; //Deduct from entrycountdown
				if (exitPending)
				{
					exitCountdown -= Time.fixedDeltaTime;
					if (exitCountdown <= 0f) //Do a countdown to exit when ending begins pending
					{
						outer.SetNextStateToMain();
						return;
					}
				}
				else if (inputBank && entryCountdown <= 0f) //If we're not exiting and we've gotten past the entry countdown
				{
					if (inputBank.skill1.down && currentPlacementInfo.ok) //If we're pressing primary and we can place
					{
						if (characterBody) //If we exist...guess that's necessary
						{ //So i guess the characterbody itself controls turret placement, cringe
						  //characterBody.SendConstructTurret(characterBody, currentPlacementInfo.position, currentPlacementInfo.rotation, MasterCatalog.FindMasterIndex(turretMasterPrefab));
							var master = new MasterSummon()
							{
								masterPrefab = turretMasterPrefab,
								position = currentPlacementInfo.position,
								rotation = currentPlacementInfo.rotation,
								//summonerBodyObject = gameObject,
								teamIndexOverride = teamComponent.teamIndex,
								ignoreTeamMemberLimit = true
							}.Perform();

                            if (skillLocator)
							{
								GenericSkill skill = skillLocator.GetSkill(SkillSlot.Secondary);
								if (skill)
								{
									skill.DeductStock(1);
								} //Get the skill and make sure stock is properly removed
							}
						}
						//Util.PlaySound(placeSoundString, gameObject);
						DestroyBlueprints(); //Finish up and get ready to skedaddle from this state
						exitPending = true;
					}
					if (inputBank.skill2.justPressed)
					{
						DestroyBlueprints(); //Cancel by pressing secondary
						exitPending = true;
					}
				}
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}

		[SerializeField]
		public GameObject wristDisplayPrefab;

		[SerializeField]
		public string placeSoundString;

		[SerializeField]
		public static GameObject blueprintPrefab = Resources.Load<GameObject>("@Architect:Assets/Prefabs/ArchitectNodeBlueprint.prefab");

		[SerializeField]
		public static GameObject turretMasterPrefab = Resources.Load<GameObject>("@Architect:Assets/Prefabs/ArchitectNodeMaster.prefab");

		private const float placementMaxUp = 1f;

		private const float placementMaxDown = 3f;

		private const float placementForwardDistance = 2f;

		private const float entryDelay = 0.1f;

		private const float exitDelay = 0.25f;

		private const float turretRadius = 0.5f;

		private const float turretHeight = 1.82f;

		private const float turretCenter = 0f;

		private const float turretModelYOffset = -0.75f;

		private const float maxPlacementRayDistance = 30f;

		private GameObject wristDisplayObject;

		private BlueprintController blueprints;

		private float exitCountdown;

		private bool exitPending;

		private float entryCountdown;

		private PlacementInfo currentPlacementInfo;

		private struct PlacementInfo
		{
			public bool ok;

			public Vector3 position;

			public Quaternion rotation;
		}
	}
}