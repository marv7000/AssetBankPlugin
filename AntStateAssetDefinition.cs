using AssetBankPlugin.Ant;
using AssetBankPlugin.Export;
using AssetBankPlugin.GenericData;
using Frosty.Controls;
using Frosty.Core;
using FrostySdk;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AssetBankPlugin
{
    public class AntStateAssetDefinition : AssetDefinition
    {
        public static Bank DefaultAntState;

        public override void GetSupportedExportTypes(List<AssetExportType> exportTypes)
        {
            exportTypes.Add(new AssetExportType("gltf", "GL Transfer format"));
            exportTypes.Add(new AssetExportType("xml", "XML Animation Keyframe Dump"));
            exportTypes.Add(new AssetExportType("smd", "Source StudioMdl Data"));

            base.GetSupportedExportTypes(exportTypes);
        }

        public override bool Export(EbxAssetEntry entry, string path, string filterType)
        {
            // Load Options
            var opt = new AnimationOptions();
            opt.Load();

            // Get the Ebx.
            EbxAsset asset = App.AssetManager.GetEbx(entry);
            dynamic antStateAsset = asset.RootObject;
            // Get the Chunk.
            Stream s;
            int bundleId = 0;
            if (antStateAsset.StreamingGuid == Guid.Empty)
            {
                ResAssetEntry res = App.AssetManager.GetResEntry(entry.Name);
                bundleId = res.Bundles[0];
                s = App.AssetManager.GetRes(res);
            }
            else
            {
                ChunkAssetEntry chunk = App.AssetManager.GetChunkEntry(antStateAsset.StreamingGuid);
                bundleId = chunk.Bundles[0];
                s = App.AssetManager.GetChunk(chunk);
            }

            // Load all AntStates into memory.
            IEnumerable<BundleEntry> bundles;
            string cachePath = $"Caches/{ProfilesLibrary.ProfileName}_antstate.cache";
            string internalCachePath = $"Caches/{ProfilesLibrary.ProfileName}_antref.cache";

            // If we're not using the cache or it doesn't exist yet, then get every bundle.
            if (opt.UseCache)
            {
                if (File.Exists(cachePath) && File.Exists(internalCachePath))
                {
                    Cache.ReadState(cachePath);
                    Cache.ReadMap(internalCachePath);
                }
                else
                {
                    // First time setup, read all bundles and store them in the antstatecache.
                    bundles = App.AssetManager.EnumerateBundles();
                    foreach (var bundle in bundles)
                    {
                        LoadAntStateFromBundle(bundle);
                    }
                    Cache.WriteState(cachePath);
                    Cache.WriteMap(internalCachePath);
                }
            }
            else
            {
                bundles = App.AssetManager.EnumerateBundles();
                foreach (var bundle in bundles)
                {
                    LoadAntStateFromBundle(bundle);
                }
            }

            // Read the main AntStateAsset.
            using (var r = new NativeReader(s))
            {
                var bank = new Bank(r, bundleId);

                var skelEbx = App.AssetManager.GetEbx(opt.ExportSkeletonAsset);
                dynamic skel = skelEbx.RootObject;

                var skeleton = SkeletonAsset.ConvertToInternal(skel);
                foreach (var dataName in bank.DataNames)
                {
                    var dat = AntRefTable.Get(dataName.Value);
                    if (dat is AnimationAsset anim)
                    {
                        anim.Name = dataName.Key;
                        anim.Channels = anim.GetChannels(anim.ChannelToDofAsset);
                        var intern = anim.ConvertToInternal();
                        new AnimationExporterSEANIM().Export(intern, skeleton, Path.GetDirectoryName(path));
                    }
                }
            }

            FrostyMessageBox.Show($"Exported {entry.Name} for {ProfilesLibrary.ProfileName}", "Test");

            return true;
        }

        public static void LoadAntStateFromBundle(BundleEntry bundle)
        {
                var resources = App.AssetManager.EnumerateRes(bundle).Where(x => x.Type == "AssetBank");
                foreach (var res in resources)
                {
                    Console.WriteLine(res.DisplayName);
                    var antBank = App.AssetManager.GetRes(res);
                    var antReader = new NativeReader(antBank);
                    _ = new Bank(antReader, App.AssetManager.GetBundleId(bundle));
                    antBank.Dispose();
                    antReader.Dispose();
                }
        }
    }
}
