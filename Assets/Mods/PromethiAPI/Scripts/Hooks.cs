using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace PromethiAPI.Hooks
{
    public static partial class CharacterBody
    {
        public static class RecalculateStats
        {
            private static System.Reflection.MethodBase _method;
            private static System.Reflection.MethodBase method => _method ?? (_method = 
                                                                    typeof(RoR2.CharacterBody).GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
                                                                    .Where(m => m.GetParameters().Length == 1)
                                                                    .Where(m => m.ReturnParameter.ParameterType.FullName == typeof(void).FullName)
                                                                    .Where(m => m.GetParameters()[0].ParameterType.FullName == typeof(CharacterBody).FullName)
                                                                    .Where(m => m.Name == nameof(RoR2.CharacterBody.RecalculateStats))
                                                                    .FirstOrDefault() 
                                                                    ?? throw new System.MissingMethodException());
            public delegate void Orig(RoR2.CharacterBody self);
            public delegate void Hook(Orig orig, RoR2.CharacterBody self);
            public static event Hook On
            {
                add => MonoMod.RuntimeDetour.HookGen.HookEndpointManager.Add<Hook>(method, value);
                remove => MonoMod.RuntimeDetour.HookGen.HookEndpointManager.Remove<Hook>(method, value);
            }
            public static event MonoMod.Cil.ILContext.Manipulator IL
            {
                add => MonoMod.RuntimeDetour.HookGen.HookEndpointManager.Modify<Hook>(method, value);
                remove => MonoMod.RuntimeDetour.HookGen.HookEndpointManager.Unmodify<Hook>(method, value);
            }
        }
    }
}
