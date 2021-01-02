using System;
using UnityEngine;
using R2API;

namespace Architect.Utils
{
	public static class Resources
	{
        public static Texture2D LoadTexture2D(byte[] resourceBytes)
        {
            //Check to make sure that the byte array supplied is not null, and throw an appropriate exception if they are.
            if (resourceBytes == null) throw new ArgumentNullException(nameof(resourceBytes));

            //Create a temporary texture, then load the texture onto it.
            var tempTex = new Texture2D(1, 1);
            tempTex.LoadImage(resourceBytes);

            return tempTex;
        }

        public static AssetBundle LoadAssetBundle(byte[] resourceBytes)
        {
            var bundle = AssetBundle.LoadFromMemory(resourceBytes);
            if (bundle != null)
                return bundle;
            throw new Exception("Failed to load bundle from bytes!");
        }

        public static AssetBundle LoadAssetBundleResourcesProvider(string prefix, AssetBundle bundle)
        {
            //Check to make sure that the byte array supplied is not null, and throw an appropriate exception if they are.
            if (bundle == null) throw new ArgumentNullException(nameof(bundle));
            if (string.IsNullOrEmpty(prefix) || !prefix.StartsWith("@")) throw new ArgumentException("Invalid prefix format", nameof(prefix));

            // Create and register the provider.
            var provider = new AssetBundleResourcesProvider(prefix, bundle);
            ResourcesAPI.AddProvider(provider);

            return bundle;
        }

        public static GameObject RegisterCodePrefab(GameObject prefab, string name)
        {
            var newPrefab = prefab.InstantiateClone(name);

            UnityEngine.Object.Destroy(prefab);

            prefab = newPrefab;

            return prefab;
        }
    }
}