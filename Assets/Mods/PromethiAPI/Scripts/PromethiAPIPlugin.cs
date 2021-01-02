using BepInEx;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[BepInPlugin("com.Promethia.PromethiAPI", "PromethiAPI", "1.0.0")]
public class PromethiAPIPlugin : BaseUnityPlugin
{
    public void Awake()
    {
        RoR2.RoR2Application.isModded = true;
    }
}
