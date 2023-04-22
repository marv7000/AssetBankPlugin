using Frosty.Core;
using Frosty.Core.Controls;
using System;
using System.Collections.Generic;

namespace AssetBankPlugin.Ant
{
    public class AntRefTable
    {
        public static Dictionary<Guid, Guid> InternalRefs = new Dictionary<Guid, Guid>();
        public static Dictionary<Guid, AntAsset> Refs = new Dictionary<Guid, AntAsset>();

        public static void Add(AntAsset asset)
        {
            Refs[asset.ID] = asset;
        }

        public static AntAsset Get(Guid refId, bool recurse = false)
        {
            if (Refs.TryGetValue(refId, out var guid))
            {
                return guid;
            }
            else if (InternalRefs.ContainsKey(refId))
            {
                if (Refs.TryGetValue(InternalRefs[refId], out var internalGuid))
                {
                    return internalGuid;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (recurse)
                {
                    // If we still land here after loading the cached bundle, then we couldn't find the requested AntRef
                    // and something has gone terribly wrong.
                    return null;
                }
                else
                {
                    int bundleId;
                    if (Cache.AntStateBundleIndices.ContainsKey(refId))
                    {
                        bundleId = Cache.AntStateBundleIndices[refId];
                    }
                    else if (Cache.AntRefMap.ContainsKey(refId) && Cache.AntStateBundleIndices.ContainsKey(Cache.AntRefMap[refId]))
                    {
                        bundleId = Cache.AntStateBundleIndices[Cache.AntRefMap[refId]];
                    }
                    else
                    {
                        FrostyExceptionBox.Show(new KeyNotFoundException($"Could not find AntRef {refId}, rebuilding the AntRef cache might fix this."), "Animation Export Error"); 
                        return null;
                    }
                    var bundle = App.AssetManager.GetBundleEntry(bundleId);
                    AntStateAssetDefinition.LoadAntStateFromBundle(bundle);

                    return Get(refId, true);
                }
            }
        }
    }
}
