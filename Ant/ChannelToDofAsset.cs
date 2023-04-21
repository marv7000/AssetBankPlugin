using AssetBankPlugin.Enums;
using System;
using System.Collections.Generic;

namespace AssetBankPlugin.Ant
{
    public class ChannelToDofAsset : AntAsset
    {
        public override string Name { get; set; }
        public override Guid ID { get; set; }

        public StorageType StorageType { get; set; }
        public byte[] IndexData { get; set; }

        public override void SetData(Dictionary<string, object> data)
        {
            Name = Convert.ToString(data["__name"]);
            ID = (Guid)data["__guid"];

            StorageType = (StorageType)Convert.ToInt32(data["StorageType"]);
            IndexData = data["IndexData"] as byte[];
        }
    }
}
