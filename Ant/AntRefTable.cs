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

        public static AntAsset Get(Guid refId)
        {
            if (Refs.TryGetValue(refId, out var guid))
            {
                return guid;
            }
            else
            {
                return Refs[InternalRefs[refId]];
            }
        }

        public static bool TryGet(Guid refId, out AntAsset result)
        {
            if(Refs.ContainsKey(refId))
            {
                result = Refs[refId];
                return true;
            }
            result = null;
            return false;
        }
    }
}
