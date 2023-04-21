using AssetBankPlugin.Enums;
using FrostySdk;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AssetBankPlugin.Ant
{
    public class ChannelToDofAsset : AntAsset
    {
        public override string Name { get; set; }
        public override Guid ID { get; set; }

        public StorageType StorageType { get; set; }
        public uint[] IndexData { get; set; }

        public override void SetData(Dictionary<string, object> data)
        {
            Name = Convert.ToString(data["__name"]);
            ID = (Guid)data["__guid"];

            if(ProfilesLibrary.IsLoaded(ProfileVersion.PlantsVsZombiesGardenWarfare2))
            {
                ushort[] dofIds = (ushort[])data["DofIds"];
                IndexData = Array.ConvertAll(dofIds, val => checked((uint)val));
            }
            else
            {
                StorageType = (StorageType)Convert.ToInt32(data["StorageType"]);
                IndexData = (data["IndexData"] as Array).Cast<uint>().ToArray();
            }
        }
    }
}
