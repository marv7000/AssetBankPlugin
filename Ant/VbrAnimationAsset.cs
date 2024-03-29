﻿using AssetBankPlugin.Export;
using System.Collections.Generic;

namespace AssetBankPlugin.Ant
{
    public class VbrAnimationAsset : AnimationAsset
    {
        public VbrAnimationAsset() { }

        public override void SetData(Dictionary<string, object> baseData)
        {
            base.SetData(baseData);
        }

        public override InternalAnimation ConvertToInternal()
        {
            return new InternalAnimation();
        }
    }
}
