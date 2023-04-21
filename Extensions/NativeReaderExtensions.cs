using AssetBankPlugin.Enums;
using AssetBankPlugin.GenericData;
using FrostySdk.IO;
using System;

namespace AssetBankPlugin.Extensions
{
    public static class NativeReaderExtensions
    {
        public static ReflLayout ReadReflLayoutEntry(this NativeReader r, Endian bigEndian, out int fieldSize)
        {
            var layout = new ReflLayout();

            layout.mMinSlot = r.ReadInt(bigEndian);
            layout.mMaxSlot = r.ReadInt(bigEndian);

            fieldSize = (layout.mMinSlot * -1) + layout.mMaxSlot + 1;

            layout.mDataSize = r.ReadUInt(bigEndian);
            layout.mAlignment = r.ReadUInt(bigEndian);
            layout.mStringTableOffset = r.ReadUInt(bigEndian);
            layout.mStringTableLength = r.ReadUInt(bigEndian);
            layout.mReordered = r.ReadBoolean();
            layout.mNative = r.ReadBoolean();
            _ = r.ReadBytes(2); // Pad
            layout.mHash = r.ReadUInt(bigEndian);

            // Fill data entries.
            layout.mEntries = new ReflEntry[fieldSize];
            for (int j = 0; j < fieldSize; j++)
            {
            LABEL_1:
                var entry = new ReflEntry();
                entry.mLayoutHash = r.ReadUInt(bigEndian);
                entry.mElementSize = r.ReadUInt(bigEndian);
                entry.mOffset = r.ReadUInt(bigEndian);
                entry.mName = r.ReadUInt(bigEndian);
                entry.mCount = r.ReadUShort(bigEndian);
                entry.mFlags = (EFlags)r.ReadUShort(bigEndian);
                entry.mElementAlign = r.ReadUShort(bigEndian);
                entry.mRLE = r.ReadShort(bigEndian);
                entry.mLayout = r.ReadLong(bigEndian);

                // Sometimes there seems to be a missing entry?
                // In that case we let everything finish, but then we fix up the alignments.
                if (entry.mElementSize != 0 && entry.mCount != 0)
                {
                    layout.mEntries[j] = entry;
                }
                else
                {
                    fieldSize -= 1;
                    goto LABEL_1;
                }
            }

            r.ReadBytes(1); // Pad
            layout.mName = r.ReadNullTerminatedString();

            // Fill field names.
            layout.mFieldNames = new string[fieldSize];
            for (int j = 0; j < fieldSize; j++)
            {
                layout.mFieldNames[j] = r.ReadNullTerminatedString();
            }

            return layout;
        }

        public static void ReadDataHeader(this NativeReader r, Endian bigEndian, out uint hash, out uint type, out uint offset)
        {
            hash = (uint)r.ReadULong(bigEndian);
            r.ReadBytes(8);
            type = (uint)r.ReadULong(bigEndian);
            r.ReadBytes(4);
            offset = r.ReadUShort(bigEndian);
            r.ReadBytes(2);
        }

        public static sbyte[] ReadSByteArray(this NativeReader r, int count)
        {
            sbyte[] array = new sbyte[count];

            for (int i = 0; i < count; i++)
            {
                array[i] = r.ReadSByte();
            }

            return array;
        }
        public static byte[] ReadByteArray(this NativeReader r, int count)
        {
            byte[] array = new byte[count];

            for (int i = 0; i < count; i++)
            {
                array[i] = r.ReadByte();
            }

            return array;
        }
        public static short[] ReadShortArray(this NativeReader r, int count, Endian bigEndian)
        {
            short[] array = new short[count];

            for (int i = 0; i < count; i++)
            {
                array[i] = r.ReadShort(bigEndian);
            }

            return array;
        }
        public static int[] ReadIntArray(this NativeReader r, int count, Endian bigEndian)
        {
            int[] array = new int[count];

            for (int i = 0; i < count; i++)
            {
                array[i] = r.ReadInt(bigEndian);
            }

            return array;
        }
        public static long[] ReadLongArray(this NativeReader r, int count, Endian bigEndian)
        {
            long[] array = new long[count];

            for (int i = 0; i < count; i++)
            {
                array[i] = r.ReadLong(bigEndian);
            }

            return array;
        }
        public static ushort[] ReadUShortArray(this NativeReader r, int count, Endian bigEndian)
        {
            ushort[] array = new ushort[count];

            for (int i = 0; i < count; i++)
            {
                array[i] = r.ReadUShort(bigEndian);
            }

            return array;
        }
        public static uint[] ReadUIntArray(this NativeReader r, int count, Endian bigEndian)
        {
            uint[] array = new uint[count];

            for (int i = 0; i < count; i++)
            {
                array[i] = r.ReadUInt(bigEndian);
            }

            return array;
        }
        public static ulong[] ReadULongArray(this NativeReader r, int count, Endian bigEndian)
        {
            ulong[] array = new ulong[count];

            for (int i = 0; i < count; i++)
            {
                array[i] = r.ReadULong(bigEndian);
            }

            return array;
        }
        public static float[] ReadSingleArray(this NativeReader r, int count, Endian bigEndian)
        {
            float[] array = new float[count];

            for (int i = 0; i < count; i++)
            {
                array[i] = r.ReadFloat(bigEndian);
            }

            return array;
        }
        public static double[] ReadDoubleArray(this NativeReader r, int count, Endian bigEndian)
        {
            double[] array = new double[count];

            for (int i = 0; i < count; i++)
            {
                array[i] = r.ReadDouble(bigEndian);
            }

            return array;
        }
        public static Guid[] ReadGuidArray(this NativeReader r, int count, Endian bigEndian)
        {
            Guid[] array = new Guid[count];

            for (int i = 0; i < count; i++)
            {
                array[i] = r.ReadGuid(bigEndian);
            }

            return array;
        }
    }
}
