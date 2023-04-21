using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace AssetBankPlugin.Ant
{
    public partial class DctAnimationAsset : AnimationAsset
    {
        public static float[,] DctCoeffs = new float[8, 8] {
            { 0.250000f, 0.490393f, 0.461940f, 0.415735f, 0.353553f, 0.277785f, 0.191342f, 0.097545f,  },
            { 0.250000f, 0.415735f, 0.191342f, -0.097545f, -0.353553f, -0.490393f, -0.461940f, -0.277785f,  },
            { 0.250000f, 0.277785f, -0.191342f, -0.490393f, -0.353553f, 0.097545f, 0.461940f, 0.415735f,  },
            { 0.250000f, 0.097545f, -0.461940f, -0.277785f, 0.353553f, 0.415735f, -0.191342f, -0.490393f,  },
            { 0.250000f, -0.097545f, -0.461940f, 0.277785f, 0.353553f, -0.415735f, -0.191342f, 0.490393f,  },
            { 0.250000f, -0.277785f, -0.191342f, 0.490393f, -0.353553f, -0.097545f, 0.461940f, -0.415735f,  },
            { 0.250000f, -0.415735f, 0.191342f, 0.097545f, -0.353553f, 0.490393f, -0.461940f, 0.277785f,  },
            { 0.250000f, -0.490393f, 0.461940f, -0.415735f, 0.353554f, -0.277785f, 0.191342f, -0.097545f,  },
        };

        public float[] GenerateCoeffs(ushort frame)
        {
            float[] coeffs = new float[8];

            var coeffIdx = frame % 8;

            for (var i = 0; i < 8; i++)
            {
                var coeff = DctCoeffs[coeffIdx, i];
                var mult = ((float)QuantizeMultSubblock * 0.1f * (float)i + 1.0f) / (float)QuantizeMultBlock;
                var value = coeff * mult;

                coeffs[i] = value;
            }
            return coeffs;
        }

        public Vector4 UnpackVec(List<short> values, ushort frame)
        {
            var result = new Vector4(0.0f);

            var coeffs = GenerateCoeffs(frame);

            for (var i = 0; i < 8; i++)
            {
                var vec = new Vector4((float)values[i * 4 + 0], (float)values[i * 4 + 1], (float)values[i * 4 + 2], (float)values[i * 4 + 3]);

                result += Vector4.Multiply(vec, coeffs[i]);
            }

            return result;
        }

        public List<Vector4> Decompress()
        {
            var s_DofCount = NumVec3 + NumQuats + NumFloatVec;

            var dofTable = new DofTable[s_DofCount];

            var s_SubBlockTotal = 0;
            for (var i = 0; i < s_DofCount; i++)
            {
                // 4 bits is unused.
                var s_SubBlocksCount = (byte)(DofTableDescBytes[i] >> 4 & 0xF);

                var s_DofData = new DofTable(s_SubBlocksCount);
                s_DofData.DeltaBase = new short[4]
                {
                DeltaBaseX[i],
                DeltaBaseY[i],
                DeltaBaseZ[i],
                DeltaBaseW[i],
                };

                s_DofData.BitsPerSubBlock = new DofTable.BitsPerComponent[s_DofData.SubBlockCount];
                for (var j = 0; j < s_DofData.SubBlockCount; j++)
                    s_DofData.BitsPerSubBlock[j] = new DofTable.BitsPerComponent(BitsPerSubblock[s_SubBlockTotal + j]);

                dofTable[i] = s_DofData;

                s_SubBlockTotal += s_SubBlocksCount;

            }

            var r = new BitReader(new MemoryStream(Data), 64, FrostySdk.IO.Endian.Big);


            var blocks = new List<List<short>>();
            for (var blockFrame = 0; blockFrame < (NumKeys + 7) / 8; blockFrame++)
            {
                for (var dofIdx = 0; dofIdx < dofTable.Length; dofIdx++)
                {
                    var block = new List<short>();

                    var subBlock = dofTable[dofIdx];

                    var s_Components = subBlock.BitsPerSubBlock;

                    if (blockFrame == 0)
                    {
                        block.Add(0);
                        block.Add(0);
                        block.Add(0);
                        block.Add(0);

                        s_Components = s_Components.Skip(1).ToArray();
                    }

                    foreach (var s_Component in s_Components)
                    {
                        var x = r.ReadIntHigh(s_Component.SafeBitsX(CatchAllBitCount));
                        var y = r.ReadIntHigh(s_Component.SafeBitsY(CatchAllBitCount));
                        var z = r.ReadIntHigh(s_Component.SafeBitsZ(CatchAllBitCount));
                        var w = r.ReadIntHigh(s_Component.SafeBitsW(CatchAllBitCount));

                        block.Add((short)x);
                        block.Add((short)y);
                        block.Add((short)z);
                        block.Add((short)w);
                    }

                    if (s_Components.Length < 8)
                    {
                        for (var i = 0; i < 8 - s_Components.Length; i++)
                        {
                            block.Add(0);
                            block.Add(0);
                            block.Add(0);
                            block.Add(0);
                        }
                    }

                    block[0] += subBlock.DeltaBase[0];
                    block[1] += subBlock.DeltaBase[1];
                    block[2] += subBlock.DeltaBase[2];
                    block[3] += subBlock.DeltaBase[3];

                    blocks.Add(block);
                }
            }

            var result = new List<Vector4>();

            for (var frame = 0; frame < NumKeys; frame++)
            {
                var blockIdx = frame / 8;

                for (var dofIdx = 0; dofIdx < dofTable.Length; dofIdx++)
                {
                    var dataIdx = blockIdx * dofTable.Length + dofIdx;

                    if (dataIdx >= blocks.Count)
                        break;

                    var block = blocks.ElementAt(dataIdx);

                    result.Add(UnpackVec(block, (ushort)frame));
                }
            }

            return result;
        }
    }
}