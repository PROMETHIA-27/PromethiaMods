using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace PromethiAPI
{
    public static class NetworkUtils
    {
        static NetworkUtils()
        {
            RoR2.Networking.GameNetworkManager.onStartGlobal += RegisterAllPrefabs;
        }

        public static void RegisterPrefab(GameObject prefab)
        {
            MD5 hash = MD5.Create();
            byte[] raw = hash.ComputeHash(Encoding.UTF8.GetBytes(Assembly.GetCallingAssembly().GetName().Name + prefab.name));
            hash.Dispose();

            var sb = new StringBuilder();
            foreach (var i in raw)
                sb.Append(i.ToString("x2"));

            HashesNeeded.Add((prefab, NetworkHash128.Parse(sb.ToString())));
        }

        private static List<(GameObject, NetworkHash128)> HashesNeeded;
        private static void RegisterAllPrefabs()
        {
            foreach (var hashNeeded in HashesNeeded)
            {
                ClientScene.RegisterPrefab(hashNeeded.Item1, hashNeeded.Item2);
            }
        }
    }
}

