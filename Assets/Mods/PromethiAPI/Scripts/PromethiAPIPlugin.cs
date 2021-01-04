using BepInEx;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PromethiAPI
{
    [BepInDependency("com.PassivePicasso.RainOfStages", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("com.Promethia.PromethiAPI", "PromethiAPI", "1.0.0")]
    public class PromethiAPIPlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            RoR2.RoR2Application.isModded = true;

            ModhelperFix<BuffDef>.Fix();
            ModhelperFix<ItemDef>.Fix();
        }
    }
}
