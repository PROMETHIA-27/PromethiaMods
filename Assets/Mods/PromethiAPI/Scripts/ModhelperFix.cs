using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PromethiAPI
{
    public class ModhelperFix<T>
    {
        public void Fix()
        {
            if (!BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.bepis.r2api"))
                Hooks.CatalogModHelper<T>.CollectAndRegisterAdditionalEntries.IL += FixIL;
        }

        private void FixIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_1);
            c.Emit<ModhelperFix<T>>(OpCodes.Callvirt, "FixedModHelperCollect");
            c.Emit(OpCodes.Ret);
        }

        public static void FixedModHelperCollect(CatalogModHelper<T> helper, ref T[] entries)
        {
            int vanillaEntryCount = entries.Length;
            List<T> list = new List<T>();

            typeof(CatalogModHelper<T>).GetEvent("getAdditionalEntries").GetRaiseMethod().Invoke(helper, new object[] { list });

            Array.Resize(ref entries, entries.Length + list.Count);

            var regDelegate = helper.GetType().GetField("registrationDelegate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var del = (Action<int, T>)regDelegate.GetValue(helper);
            for (int i = vanillaEntryCount; i < vanillaEntryCount + list.Count; i++)
            {
                del(i, list[i - vanillaEntryCount]);
            }
        }
    }
}
