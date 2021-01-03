using EntityStates;
//using R2API;
using RoR2;
using RoR2.Orbs;
using RoR2.Projectile;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Architect.EntityStates.ArchitectNode
{
    class FireTesla : NodePrimaryWeapon
    {
		private List<HealthComponent> previousTeslaTargetList = new List<HealthComponent>();

        public override void OnExit()
		{
            LightningOrb lightningOrb = new LightningOrb
            {
                origin = characterBody.aimOrigin,
                isCrit = false,
                bouncesRemaining = 0,
                teamIndex = teamComponent.teamIndex,
                attacker = gameObject,
                procCoefficient = 0.0f,
                bouncedObjects = previousTeslaTargetList,
                lightningType = LightningOrb.LightningType.Tesla,
                damageColorIndex = DamageColorIndex.Item,
                range = GetOrbRange()
            };
            HurtBox hurtBox = lightningOrb.PickNextTarget(transform.position);
            if (hurtBox)
            {
                lightningOrb.damageValue = GetOrbDamage(hurtBox);
                lightningOrb.target = hurtBox;
                OrbManager.instance.AddOrb(lightningOrb);
            }

            base.OnExit();
		}

        static readonly int MassNodeBodyIndex = BodyCatalog.FindBodyIndex("MassNodeBody");
        static readonly int DesignNodeBodyIndex = BodyCatalog.FindBodyIndex("DesignNodeBody");
        static readonly int BloodNodeBodyIndex = BodyCatalog.FindBodyIndex("BloodNodeBody");

        public static void NodeProcs(DamageReport report)
        {
            if (!NetworkServer.active)
                return;
            if (report.attackerBodyIndex == MassNodeBodyIndex || report.attackerBodyIndex == DesignNodeBodyIndex || report.attackerBodyIndex == BloodNodeBodyIndex)
            {
                var nodeData = report.attacker.GetComponent<NodeData>();

                var MassGreenTier1 = nodeData.GetMatchingInputCount(OutputColor.Green, NodeElement.Mass, 0);
                var MassGreenTier2 = nodeData.GetMatchingInputCount(OutputColor.Green, NodeElement.Mass, 1);
                var MassRedTier1 = nodeData.GetMatchingInputCount(OutputColor.Red, NodeElement.Mass, 0);
                var MassRedTier2 = nodeData.GetMatchingInputCount(OutputColor.Red, NodeElement.Mass, 1);
                var DesignRedTier1 = nodeData.GetMatchingInputCount(OutputColor.Red, NodeElement.Design, 0);
                var BloodRedTier1 = nodeData.GetMatchingInputCount(OutputColor.Red, NodeElement.Blood, 0);
                var BloodRedTier2 = nodeData.GetMatchingInputCount(OutputColor.Red, NodeElement.Blood, 1);

                var blackHoleProc = Util.CheckRoll(MassGreenTier2);
                if (blackHoleProc)
                    ProjectileManager.instance.FireProjectile(Resources.Load<GameObject>("Prefabs/Projectiles/GravSphere"), report.attacker.transform.position, Quaternion.identity, report.attacker, 0f, 0f, false, DamageColorIndex.Default, null, 0f);

                report.attackerBody.healthComponent.AddBarrier(report.attackerBody.maxHealth * 0.1f * MassGreenTier1);

                for (int i = 0; i < BloodRedTier2; i++)
                    DotController.InflictDot(report.victim.gameObject, report.attacker, DotController.DotIndex.Bleed, 3f * report.damageInfo.procCoefficient, 1f);

                for (int i = 0; i < BloodRedTier1; i++)
                    DotController.InflictDot(report.victim.gameObject, report.attacker, DotController.DotIndex.Burn, 4f * report.damageInfo.procCoefficient, 1f);

                if (MassRedTier1 > 0)
                    report.victimBody.AddTimedBuff(ArchitectPlugin.ReduceMoveSpeed25BuffIdx, 1.5f * MassRedTier1);

                if (MassRedTier2 > 0)
                    report.victimBody.AddTimedBuff(ArchitectPlugin.ReduceAttackSpeed5BuffIdx, 0.5f * MassRedTier2);

                if (DesignRedTier1 > 0)
                    report.victimBody.AddTimedBuff(ArchitectPlugin.ReduceArmor30BuffIdx, 1.5f * DesignRedTier1);
            }
            if (report.victimBodyIndex == MassNodeBodyIndex || report.victimBodyIndex == DesignNodeBodyIndex || report.victimBodyIndex == BloodNodeBodyIndex)
            {
                var nodeData = report.victim.GetComponent<NodeData>();

                var DesignGreenTier1 = nodeData.GetMatchingInputCount(OutputColor.Green, NodeElement.Design, 0);

                if (DesignGreenTier1 > 0 && report.damageDealt > report.victimBody.maxHealth / 0.1f)
                {
                    report.victimBody.AddTimedBuff(BuffIndex.Cloak, 5f * DesignGreenTier1);
                }
            }
        }

        static GameObject explodePrefab = Resources.Load<GameObject>("prefabs/effects/WilloWispExplosion");

        public static void NodeOnDeath(DamageReport report)
        {
            if (!NetworkServer.active)
                return;
            if (report.attackerBodyIndex == MassNodeBodyIndex || report.attackerBodyIndex == DesignNodeBodyIndex || report.attackerBodyIndex == BloodNodeBodyIndex)
            {
                var nodeData = report.attacker.GetComponent<NodeData>();
                
                var DesignGreenTier2 = nodeData.GetMatchingInputCount(OutputColor.Green, NodeElement.Design, 1);

                var BloodGreenTier1 = nodeData.GetMatchingInputCount(OutputColor.Green, NodeElement.Blood, 0);
                var BloodGreenTier2 = nodeData.GetMatchingInputCount(OutputColor.Green, NodeElement.Blood, 1);

                if (Util.CheckRoll(30f * BloodGreenTier1))
                {
                    Vector3 corePosition = Util.GetCorePosition(report.victim.gameObject);
                    const float damageCoefficient = 3.5f;
                    float baseDamage = Util.OnKillProcDamage(report.attackerBody.damage, damageCoefficient);
                    GameObject explosionInstance = Object.Instantiate(explodePrefab, corePosition, Quaternion.identity);
                    DelayBlast blast = explosionInstance.GetComponent<DelayBlast>();
                    blast.position = corePosition;
                    blast.baseDamage = baseDamage;
                    blast.baseForce = 2000f;
                    blast.bonusForce = Vector3.up * 1000f;
                    blast.radius = 12f;
                    blast.attacker = report.attacker;
                    blast.inflictor = null;
                    blast.crit = Util.CheckRoll(report.attackerBody.crit, report.attackerMaster);
                    blast.maxTimer = 0.5f;
                    blast.damageColorIndex = DamageColorIndex.Item;
                    blast.falloffModel = BlastAttack.FalloffModel.SweetSpot;
                    explosionInstance.GetComponent<TeamFilter>().teamIndex = report.attackerTeamIndex;
                    NetworkServer.Spawn(explosionInstance);
                }

                if (Util.CheckRoll(10f * BloodGreenTier2))
                {
                    Util.TryToCreateGhost(report.victimBody, report.attackerBody, 30);
                }
            }
        }
	}
}
