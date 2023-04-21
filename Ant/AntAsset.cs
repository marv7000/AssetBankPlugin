using AssetBankPlugin.Extensions;
using AssetBankPlugin.GenericData;
using FrostySdk.IO;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;

namespace AssetBankPlugin.Ant
{
    public abstract class AntAsset
    {
        public abstract string Name { get; set; }
        public abstract Guid ID { get; set; }

        public Bank Bank { get; set; }

        public abstract void SetData(Dictionary<string, object> data);

        public static AntAsset Deserialize(NativeReader r, SectionData section, Dictionary<uint, GenericClass> classes, Bank bank)
        {
            r.BaseStream.Position = section.DataOffset;
            r.ReadDataHeader(section.Endianness, out uint hash, out uint type, out uint offset);

            var values = section.ReadValues(r, classes, section.DataOffset + offset, type);

            // Add the base values if class inherits from another class.
            if ((long)values["__base"] != 0)
            {
                r.BaseStream.Position = section.DataOffset + (long)values["__base"];

                r.ReadDataHeader(section.Endianness, out uint base_hash, out uint base_type, out uint base_offset);

                var baseValues = section.ReadValues(r, 
                    classes, 
                    section.DataOffset + base_offset + Convert.ToUInt32(values["__base"]), 
                    base_type);

                foreach (var value in baseValues)
                {
                    if (!values.ContainsKey(value.Key))
                        values.Add(value.Key, value.Value);
                }
            }

            Type assetType = Type.GetType("AssetBankPlugin.Ant." + classes[type].Name);

            // If assetType is not null, that means we have a supported AntAsset. In that case, parse it and add it to the AntRefTable.
            if (assetType != null)
            {
                AntAsset asset = (AntAsset)Activator.CreateInstance(assetType);
                asset.Bank = bank;
                asset.SetData(values);

                AntRefTable.Add(asset);
                return asset;
            }
            else
            {
                return null;
            }

        }
    }
}
