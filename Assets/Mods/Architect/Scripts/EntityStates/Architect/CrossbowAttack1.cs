using RoR2;
using UnityEngine;
using EntityStates;
using R2API;
using UnityEngine.Networking;

namespace Architect.EntityStates.Architect
{
    class CrossbowAttack1 : BaseState
    {
        public float baseDuration = 0.5f;
        float duration;

        private GameObject hitEffectPrefab = Resources.Load<GameObject>("prefabs/effects/impacteffects/Hitspark");
        private GameObject tracerEffectPrefab = Resources.Load<GameObject>("prefabs/effects/tracers/TracerHuntressSnipe");

        private ArchitectData arData;

        public override void OnEnter()
        {
            base.OnEnter();

            arData = gameObject.GetComponent<ArchitectData>();

            duration = baseDuration / attackSpeedStat;
            Ray aimRay = GetAimRay();
            StartAimMode(aimRay, 2f, false);
            if (isAuthority)
            {
                var force = 3f;
                var damage = characterBody.damage;
                var damageType = DamageType.Generic;
                switch (arData.element)
                {
                    case NodeElement.Mass:
                        force = 10f;
                        damage *= 1.5f;
                        break;
                    case NodeElement.Design:
                        damage *= 2.5f;
                        break;
                    case NodeElement.Blood:
                        damage *= 1.5f;
                        damageType = DamageType.IgniteOnHit;
                        break;
                }

                new BulletAttack
                {
                    owner = gameObject,
                    weapon = gameObject,
                    origin = aimRay.origin,
                    aimVector = aimRay.direction,
                    minSpread = 0f,
                    maxSpread = characterBody.spreadBloomAngle,
                    bulletCount = 1U,
                    procCoefficient = 1f,
                    damage = characterBody.damage * 1.5f,
                    force = force,
                    falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                    tracerEffectPrefab = tracerEffectPrefab,
                    hitEffectPrefab = hitEffectPrefab,
                    isCrit = RollCrit(),
                    HitEffectNormal = false,
                    stopperMask = LayerIndex.world.mask | LayerIndex.defaultLayer.mask,
                    smartCollision = true,
                    maxDistance = 300f,
                    damageType = damageType
                }.Fire();
            }
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
            return InterruptPriority.Skill;
        }
    }
}
