using System;
using System.Collections.Generic;
using System.Linq;
using AssetBankPlugin.Enums;
using AssetBankPlugin.Export;
using AssetBankPlugin.GenericData;
using Frosty.Core;
using FrostySdk;
using FrostySdk.IO;
using FrostySdk.Managers.Entries;

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
            //Channels = GetChannels(ChannelToDofAsset);
        }

        public Dictionary<string, BoneChannelType> GetChannels(Guid channelToDofAsset)
        {
            // Get bone mapping table.
            var opt = new AnimationOptions();
            opt.Load();
            ResAssetEntry antStateEntry = App.AssetManager.GetResEntry(opt.AntStateAsset);
            Bank antStateBank;
            if (antStateEntry != null)
            {
                var antBank = App.AssetManager.GetRes(antStateEntry);
                var r = new NativeReader(antBank);
                antStateBank = new Bank(r);
            }

            // Get the ChannelToDofAsset referenced by the Animation.
            ChannelToDofAsset dof;
            if(ProfilesLibrary.IsLoaded(ProfileVersion.PlantsVsZombiesGardenWarfare2))
            {
                dof = (ChannelToDofAsset)Bank.Data[ChannelToDofAsset];
            }
            else
            {
                dof = (ChannelToDofAsset)AntRefTable.Get(channelToDofAsset);
                StorageType = dof.StorageType;
            }

            // Find the ClipControllerAsset which references the Animation.
            dynamic clip = null;

            switch((ProfileVersion)ProfilesLibrary.DataVersion)
            {
                case ProfileVersion.PlantsVsZombiesGardenWarfare2:
                    foreach (var c in AntRefTable.Refs)
                    {
                        if (c.Value is ClipControllerAsset cl)
                        {
                            if(cl.Anims.Contains(ID) || AntRefTable.TryGet(ID, out AntAsset clAsset))
                            {
                                clip = cl;
                                break;
                            }
                        }
                    }
                    FPS = clip.FPS;
                    break;
                default:
                    foreach (var c in AntRefTable.Refs)
                    {
                        if (c.Value is ClipControllerData cl)
                        {
                            if (cl.Anim == ID || cl.Anim == AntRefTable.InternalRefs[ID])
                            {
                                clip = cl;
                                break;
                            }
                        }
                    }
                    FPS = clip.FPS;
                    break;
            }

            // Get the LayoutHierarchyAsset Guid from the ClipController.
            LayoutHierarchyAsset hierarchy = (LayoutHierarchyAsset)AntRefTable.Get(clip.Target);

            // Loop through all LayoutAssets and append them.
            var channelNames = new Dictionary<string, BoneChannelType>();
            for (int i = 0; i < hierarchy.LayoutAssets.Length; i++)
            {
                Object layoutAsset = AntRefTable.Get(hierarchy.LayoutAssets[i]);

                if(layoutAsset is LayoutAsset la)
                {
                    for (int x = 0; x < la.Slots.Count; x++)
                    {
                        channelNames.Add(la.Slots[x].Name, la.Slots[x].Type);
                    }
                }
                else if(layoutAsset is DeltaTrajLayoutAsset)
                {

                }
            }
            
            uint[] data = dof.IndexData;
            var channels = new List<string>();

            if (ProfilesLibrary.IsLoaded(ProfileVersion.PlantsVsZombiesGardenWarfare2))
            {
                // TODO: crashes when exporting Worlds/Hub/Props/Hub_Prop_Sewer_Rat_Animations
                for (int i = 0; i < data.Length; i++)
                {
                    channels.Add("");
                }

                for (int i = 0; i < data.Length; i++)
                {
                    int channelId = (int)data[i];
                    channels[i] = channelNames.ElementAt(channelId).Key;
                }
            }
            else
            {
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
                                int channelId = (int)data[i];
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
                                int appendTo = (int)data[i];
                                int channelId = (int)data[i + 1];

                                offsets[appendTo] = offset;
                                offset++;

                                channels.Insert(offsets[appendTo], channelNames.ElementAt(channelId).Key);
                            }
                        }
                        break;
                }
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