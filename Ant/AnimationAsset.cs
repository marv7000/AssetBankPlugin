using System;
using System.Collections.Generic;
using System.Linq;
using AssetBankPlugin.Enums;
using AssetBankPlugin.Export;
using AssetBankPlugin.GenericData;
using Frosty.Core;
using FrostySdk.IO;

namespace AssetBankPlugin.Ant
{
    public class AnimationAsset : AntAsset
    {
        public override string Name { get; set; }
        public override Guid ID { get; set; }

        public int CodecType;
        public int AnimId;
        public float TrimOffset;
        public ushort EndFrame;
        public bool Additive;
        public Guid ChannelToDofAsset;
        public Dictionary<string, BoneChannelType> Channels;
        public float FPS;

        public StorageType StorageType;


        public AnimationAsset() { }

        public override void SetData(Dictionary<string, object> data)
        {
            Name = (string)data["__name"];
            ID = (Guid)data["__guid"];
            CodecType = Convert.ToInt32(data["CodecType"]);
            AnimId = Convert.ToInt32(data["AnimId"]);
            TrimOffset = (float)data["TrimOffset"];
            EndFrame = Convert.ToUInt16(data["EndFrame"]);
            // Newer titles don't store the additive flag, not sure why.
            if (data.TryGetValue("Additive", out object additive))
            {
                Additive = (bool)additive;
            }
            ChannelToDofAsset = (Guid)data["ChannelToDofAsset"];
            Channels = GetChannels(ChannelToDofAsset);
        }

        public Dictionary<string, BoneChannelType> GetChannels(Guid channelToDofAsset)
        {
            // Get bone mapping table.
            var opt = new AnimationOptions();
            opt.Load();
            var entry = App.AssetManager.GetResEntry(opt.AntStateAsset);
            var s = App.AssetManager.GetRes(entry);
            var r = new NativeReader(s);
            Bank basic = new Bank(r);

            // Get the ChannelToDofAsset referenced by the Animation.
            var dof = (ChannelToDofAsset)AntRefTable.Get(channelToDofAsset);
            StorageType = dof.StorageType;

            // Find the ClipControllerAsset which references the Animation.
            ClipControllerData clip = null;
            foreach (var c in AntRefTable.Refs)
            {
                if (c.Value is ClipControllerData cl && cl.Target == channelToDofAsset)
                {
                    clip = cl;
                }
            }

            FPS = clip.FPS;

            // Get the LayoutHierarchyAsset Guid from the ClipController.
            LayoutHierarchyAsset hierarchy = (LayoutHierarchyAsset)AntRefTable.Get(clip.Target);

            // Loop through all LayoutAssets and append them.
            var channelNames = new Dictionary<string, BoneChannelType>();
            for (int i = 0; i < hierarchy.LayoutAssets.Length; i++)
            {
                var layoutAsset = (LayoutAsset)AntRefTable.Get(hierarchy.LayoutAssets[i]);
            
                for (int x = 0; x < layoutAsset.Slots.Count; x++)
                {
                    channelNames.Add(layoutAsset.Slots[x].Name, layoutAsset.Slots[x].Type);
                }
            }
            
            byte[] data = dof.IndexData;
            var channels = new List<string>();
            
            switch (StorageType)
            {
                // If we overwrite the channels, then just remap the orders.
                case StorageType.Overwrite:
                    {
                        for (int i = 0; i < data.Length; i++)
                        {
                            channels.Add("");
                        }
            
                        for (int i = 0; i < data.Length; i++)
                        {
                            int channelId = data[i];
                            channels[i] = channelNames.ElementAt(channelId).Key;
                        }
                    }
                    break;
                // If we append the channels, the first byte indicates the taget, then second byte the value.
                case StorageType.Append:
                    {
                        var offsets = new Dictionary<int, int>();
                        int offset = 0;
                        for (int i = 0; i < data.Length; i += 2)
                        {
                            int appendTo = data[i];
                            int channelId = data[i + 1];
            
                            offsets[appendTo] = offset;
                            offset++;
            
                            channels.Insert(offsets[appendTo], channelNames.ElementAt(channelId).Key);
                        }
                    }
                    break;
            }
            
            // Reorder
            var output = new Dictionary<string, BoneChannelType>();
            for (int i = 0; i < channels.Count; i++)
            {
                output[channels[i]] =  channelNames[channels[i]];
            }
            
            return output;
        }

        public virtual InternalAnimation ConvertToInternal() { return null; }
    }

    public enum BoneChannelType
    {
        None = 0,
        Rotation = 14,
        Position = 2049856663,
        Scale = 2049856454,
    }
}