using AssetBankPlugin.Export;
using System;
using System.Collections.Generic;
using System.Numerics;


namespace AssetBankPlugin.Ant
{
    public class FrameAnimationAsset : AnimationAsset
    {
        public int FloatCount = 0;
        public int Vec3Count = 0;
        public int QuatCount = 0;
        public float[] Data = new float[0];

        public FrameAnimationAsset() { }

        public override void SetData(Dictionary<string, object> data)
        {
            Name = (string)data["__name"];
            ID = (Guid)data["__guid"];
            Data = (float[])data["Data"];
            FloatCount = Convert.ToInt32(data["FloatCount"]);
            Vec3Count = Convert.ToInt32(data["Vec3Count"]);
            QuatCount = Convert.ToInt32(data["QuatCount"]);

            base.SetData(data);
        }

        public override InternalAnimation ConvertToInternal()
        {
            InternalAnimation ret = new InternalAnimation();
            List<Vector3> positions = new List<Vector3>();
            List<Quaternion> rotations = new List<Quaternion>();

            List<string> posChannels = new List<string>();
            List<string> rotChannels = new List<string>();

            int dataIndex = 0;
            // Get all names.
            foreach (var channel in Channels)
            {
                if (channel.Value == BoneChannelType.Rotation)
                    rotChannels.Add(channel.Key.Replace(".q", ""));
                else if (channel.Value == BoneChannelType.Position)
                    posChannels.Add(channel.Key.Replace(".t", ""));
            }
            foreach (var channel in Channels)
            {
                if (channel.Value == BoneChannelType.Rotation)
                {
                    rotations.Add(new Quaternion(Data[dataIndex++], Data[dataIndex++], Data[dataIndex++], Data[dataIndex++]));
                }
                else if (channel.Value == BoneChannelType.Position)
                {
                    positions.Add(new Vector3(Data[dataIndex++], Data[dataIndex++], Data[dataIndex++]));
                    dataIndex++;
                }
            }

            // FrameAnimations are like RawAnimations but with one fixed frame.
            ret.Name = Name;
            var frame = new Frame();
            frame.FrameIndex = 0;
            frame.Positions = positions;
            frame.Rotations = rotations;
            ret.Frames.Add(frame);
            ret.PositionChannels = posChannels;
            ret.RotationChannels = rotChannels;
            ret.Additive = Additive;

            return ret;
        }
    }
}