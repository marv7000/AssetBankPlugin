using AssetBankPlugin.Ant;
using AssetBankPlugin.Export;
using AssetBankPlugin.GenericData;
using Frosty.Core;
using FrostySdk;
using FrostySdk.IO;
using FrostySdk.Managers.Entries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace AssetBankPlugin
{
    public class AntStateAssetDefinition : AssetDefinition
    {
        public override void GetSupportedExportTypes(List<AssetExportType> exportTypes)
        {
            exportTypes.Add(new AssetExportType("gltf", "GL Transfer format"));
            exportTypes.Add(new AssetExportType("xml", "XML Animation Keyframe Dump"));
            exportTypes.Add(new AssetExportType("smd", "Source StudioMdl Data"));

            base.GetSupportedExportTypes(exportTypes);
        }

        public override bool Export(EbxAssetEntry entry, string path, string filterType)
        {
            ExportInternal(entry);

            return true;
        }

        public void ExportInternal(EbxAssetEntry entry)
        {
            // Get the Ebx.
            EbxAsset asset = App.AssetManager.GetEbx(entry); 
            dynamic antStateAsset = (dynamic)asset.RootObject;
            // Get the Chunk.
            Stream s;
            if (antStateAsset.StreamingGuid == Guid.Empty)
            {
                ResAssetEntry res = App.AssetManager.GetResEntry(entry.Name);
                s = App.AssetManager.GetRes(res);
            }
            else
            {
                ChunkAssetEntry chunk = App.AssetManager.GetChunkEntry(antStateAsset.StreamingGuid);
                s = App.AssetManager.GetChunk(chunk);
            }

            using (var r = new NativeReader(s))
            {
                var bank = new Bank(r);

                var opt = new AnimationOptions();
                opt.Load();

                var skelEbx = App.AssetManager.GetEbx(opt.ExportSkeletonAsset);
                dynamic skel = (dynamic)skelEbx.RootObject;

                var skeleton = SkeletonAsset.ConvertToInternal(skel);
                //foreach (var anim in bank.Data)
                //{
                //    var intern = anim.Value.ConvertToInternal();
                //    new AnimationExporterSEANIM().Export(intern, skeleton, @"D:\");
                //}
            }

            Console.WriteLine(AntRefTable.Refs);

            MessageBox.Show($"Exporting {entry.Name} for {ProfilesLibrary.ProfileName} AntStateAsset", "Test");
        }
    }
}
