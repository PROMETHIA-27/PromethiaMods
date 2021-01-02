using EntityStates;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace Architect.EntityStates.ArchitectNode
{
    class NodeMain : GenericCharacterMain
    {
        public static HashSet<GameObject> ActiveNodes = new HashSet<GameObject>();

        const float barrierInterval = 1f;
        float barrierTimer;

        public override void OnEnter()
        {
            base.OnEnter();

            ActiveNodes.Add(gameObject);
        }

        public override void OnExit()
        {
            ActiveNodes.Remove(gameObject);

            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            barrierTimer += Time.fixedDeltaTime;
            if (barrierTimer >= barrierInterval && isAuthority)
            {
                barrierTimer -= barrierInterval;
                healthComponent.AddBarrierAuthority(healthComponent.fullHealth * 0.05f * barrierInterval);
            }
        }
    }
}
