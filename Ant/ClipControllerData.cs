using System;
using System.Collections.Generic;

namespace AssetBankPlugin.Ant
{
    public class ClipControllerData : AntAsset
    {
        public override string Name { get; set; }
        public override Guid ID { get; set; }

        public Guid Anim { get; set; }
        public Guid Target { get; set; }
        public Guid ChannelToDofAsset { get; set; }
        public float FPS { get; set; }
        public float FPSScale { get; set; }
        public float TrimOffset { get; set; }
        public float NumTicks { get; set; }
        public float TickOffset { get; set; }
        public float Distance { get; set; }
        public int CodecType { get; set; }

        public override void SetData(Dictionary<string, object> data)
        {
            Name = Convert.ToString(data["__name"]);
            ID = (Guid)data["__guid"];

            Anim = (Guid)data["Anim"];
            Target = (Guid)data["Target"];
            ChannelToDofAsset = (Guid)data["ChannelToDofAsset"];
            FPS = Convert.ToSingle(data["FPS"]);
            CodecType = Convert.ToInt32(data["CodecType"]);
            FPSScale = Convert.ToSingle(data["FPSScale"]);
            TickOffset = Convert.ToSingle(data["TickOffset"]);
            NumTicks = Convert.ToSingle(data["NumTicks"]);
            TrimOffset = Convert.ToSingle(data["TrimOffset"]);
            Distance = Convert.ToSingle(data["Distance"]);
        }
    }
}
