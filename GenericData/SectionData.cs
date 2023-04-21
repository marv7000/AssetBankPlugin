using AssetBankPlugin.Extensions;
using FrostySdk.IO;
using System.Collections.Generic;
using System.Text;

namespace AssetBankPlugin.GenericData
{
    public class SectionData : Section
    {
        public const string Identifier = "GD.DATA";
        public override Endian Endianness { get; set; }
        public override uint DataSize { get; set; }
        public override uint DataOffset { get; set; }
        public uint IndicesOffset { get; set; }

        public SectionData(NativeReader r, Endian endian)
        {
            Endianness = endian;

            _ = r.ReadSizedString(8);
            DataSize = r.ReadUInt(Endianness);
            IndicesOffset = r.ReadUInt(Endianness);
            DataOffset = (uint)r.BaseStream.Position;
            IndicesOffset += DataOffset;

            r.BaseStream.Position += DataSize - 16;
        }

        public Dictionary<string, object> ReadValues(NativeReader r, Dictionary<uint, GenericClass> classes, uint baseOffset, uint type)
        {
            var data = new Dictionary<string, object>();

            GenericClass cl = classes[type];
            for (int x = 0; x < cl.Elements.Count; x++)
            {
                GenericField field = cl.Elements[x];
                object fieldData = null;

                // Go to the offset of the current field.
                r.BaseStream.Position = baseOffset + field.Offset;
                switch (field.Type)
                {
                    case "Bool":
                        if (!field.IsArray)
                        {
                            fieldData = r.ReadBoolean();
                        }
                        break;
                    case "UInt8":
                        if (!field.IsArray)
                        {
                            fieldData = r.ReadByte();
                        }
                        else
                        {
                            uint size = r.ReadUInt(Endianness);
                            uint capacity = r.ReadUInt(Endianness);
                            long offset = r.ReadLong(Endianness);
                            r.BaseStream.Position = DataOffset + offset;
                            fieldData = r.ReadByteArray((int)size);
                        }
                        break;
                    case "Int8":
                        if (!field.IsArray)
                        {
                            fieldData = r.ReadSByte();
                        }
                        else
                        {
                            uint size = r.ReadUInt(Endianness);
                            uint capacity = r.ReadUInt(Endianness);
                            long offset = r.ReadLong(Endianness);
                            r.BaseStream.Position = DataOffset + offset;
                            fieldData = r.ReadSByteArray((int)size);
                        }
                        break;
                    case "Int16":
                        if (!field.IsArray)
                        {
                            fieldData = r.ReadShort(Endianness);
                        }
                        else
                        {
                            uint size = r.ReadUInt(Endianness);
                            uint capacity = r.ReadUInt(Endianness);
                            long offset = r.ReadLong(Endianness);
                            r.BaseStream.Position = DataOffset + offset;
                            fieldData = r.ReadShortArray((int)size, Endianness);
                        }
                        break;
                    case "Int32":
                        if (!field.IsArray)
                        {
                            fieldData = r.ReadInt(Endianness);
                        }
                        else
                        {
                            uint size = r.ReadUInt(Endianness);
                            uint capacity = r.ReadUInt(Endianness);
                            long offset = r.ReadLong(Endianness);
                            r.BaseStream.Position = DataOffset + offset;
                            fieldData = r.ReadIntArray((int)size, Endianness);
                        }
                        break;
                    case "Int64":
                        if (!field.IsArray)
                        {
                            fieldData = r.ReadLong(Endianness);
                        }
                        else
                        {
                            uint size = r.ReadUInt(Endianness);
                            uint capacity = r.ReadUInt(Endianness);
                            long offset = r.ReadLong(Endianness);
                            r.BaseStream.Position = DataOffset + offset;
                            fieldData = r.ReadLongArray((int)size, Endianness);
                        }
                        break;
                    case "UInt16":
                        if (!field.IsArray)
                        {
                            fieldData = r.ReadUShort(Endianness);
                        }
                        else
                        {
                            uint size = r.ReadUInt(Endianness);
                            uint capacity = r.ReadUInt(Endianness);
                            long offset = r.ReadLong(Endianness);
                            r.BaseStream.Position = DataOffset + offset;
                            fieldData = r.ReadUShortArray((int)size, Endianness);
                        }
                        break;
                    case "UInt32":
                        if (!field.IsArray)
                        {
                            fieldData = r.ReadInt(Endianness);
                        }
                        else
                        {
                            uint size = r.ReadUInt(Endianness);
                            uint capacity = r.ReadUInt(Endianness);
                            long offset = r.ReadLong(Endianness);
                            r.BaseStream.Position = DataOffset + offset;
                            fieldData = r.ReadUIntArray((int)size, Endianness);
                        }
                        break;
                    case "UInt64":
                        if (!field.IsArray)
                        {
                            fieldData = r.ReadLong(Endianness);
                        }
                        else
                        {
                            uint size = r.ReadUInt(Endianness);
                            uint capacity = r.ReadUInt(Endianness);
                            long offset = r.ReadLong(Endianness);
                            r.BaseStream.Position = DataOffset + offset;
                            fieldData = r.ReadULongArray((int)size, Endianness);
                        }
                        break;
                    case "Float":
                        if (!field.IsArray)
                        {
                            fieldData = r.ReadFloat(Endianness);
                        }
                        else
                        {
                            uint size = r.ReadUInt(Endianness);
                            uint capacity = r.ReadUInt(Endianness);
                            long offset = r.ReadLong(Endianness);
                            r.BaseStream.Position = DataOffset + offset;
                            fieldData = r.ReadSingleArray((int)size, Endianness);
                        }
                        break;
                    case "Double":
                        if (!field.IsArray)
                        {
                            fieldData = r.ReadDouble(Endianness);
                        }
                        else
                        {
                            uint size = r.ReadUInt(Endianness);
                            uint capacity = r.ReadUInt(Endianness);
                            long offset = r.ReadLong(Endianness);
                            r.BaseStream.Position = DataOffset + offset;
                            fieldData = r.ReadDoubleArray((int)size, Endianness);
                        }
                        break;
                    case "DataRef":
                        if (!field.IsArray)
                        {
                            fieldData = r.ReadLong(Endianness);
                        }
                        else
                        {
                            uint size = r.ReadUInt(Endianness);
                            uint capacity = r.ReadUInt(Endianness);
                            long offset = r.ReadLong(Endianness);
                            r.BaseStream.Position = DataOffset + offset;
                            fieldData = r.ReadGuidArray((int)size, Endianness);
                        }
                        break;
                    case "String":
                        if (!field.IsArray)
                        {
                            uint size = r.ReadUInt(Endianness);
                            uint capacity = r.ReadUInt(Endianness);
                            long offset = r.ReadLong(Endianness);
                            r.BaseStream.Position = DataOffset + offset;
                            fieldData = r.ReadSizedString((int)size);
                        }
                        break;
                    case "Guid":
                        if (!field.IsArray)
                        {
                            fieldData = r.ReadGuid(Endianness);
                        }
                        else
                        {
                            uint size = r.ReadUInt(Endianness);
                            uint capacity = r.ReadUInt(Endianness);
                            long offset = r.ReadLong(Endianness);
                            r.BaseStream.Position = DataOffset + offset;
                            fieldData = r.ReadGuidArray((int)size, Endianness);
                        }
                        break;
                    default:
                        if (!field.IsArray)
                        {
                            fieldData = ReadValues(r, classes, (uint)(field.Offset + baseOffset), field.TypeHash);
                        }
                        else
                        {
                            uint size = r.ReadUInt(Endianness);
                            uint capacity = r.ReadUInt(Endianness);
                            long offset = r.ReadLong(Endianness);
                            fieldData = new Dictionary<string, object>[size];
                            for (uint i = 0; i < size; i++)
                            {
                                var alignedSize = GetAlignedSize(cl.Elements[x].Size, cl.Elements[x].Alignment);
                                (fieldData as Dictionary<string, object>[])[i] = 
                                    ReadValues(r, classes, (uint)(DataOffset + offset + alignedSize * i), field.TypeHash);
                            }
                        }
                        break;
                }
                data.Add(field.Name, fieldData);
            }
            return data;
        }

        /// <summary>
        /// Gets the total amount of bytes a block would need if it were to be aligned by <paramref name="alignBy"/>.
        /// </summary>
        private static uint GetAlignedSize(uint size, uint alignBy)
        {
            if (size % alignBy != 0)
                size += alignBy - (size % alignBy);
            return size;
        }

        public override string ToString()
        {
            return $"GD.DATA [{Endianness}-Endian, Offset {DataOffset}, DataSize {DataSize}]";
        }
    }


}
