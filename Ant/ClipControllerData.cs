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
        public float FPS { get; set; }

        public override void SetData(Dictionary<string, object> data)
        {
            Name = Convert.ToString(data["__name"]);
            ID = (Guid)data["__guid"];

            Anim = (Guid)data["Anim"];
            Target = (Guid)data["Target"];
            FPS = Convert.ToSingle(data["FPS"]);
        }
    }
}
