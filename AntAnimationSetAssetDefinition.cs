using AssetBankPlugin.Ant;
using AssetBankPlugin.Export;
using Frosty.Core;
using FrostySdk.IO;
using FrostySdk.Managers.Entries;
using FrostySdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using AssetBankPlugin.GenericData;

namespace AssetBankPlugin
{
    public class AntAnimationSetAssetDefinition : AssetDefinition
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
            // Get the Ebx.
            EbxAsset asset = App.AssetManager.GetEbx(entry);
            dynamic antSetAsset = (dynamic)asset.RootObject;
            // Get the Chunk.
            Stream s;
            if (antSetAsset.AssetBankResource != (ulong)0)
            {
                ResAssetEntry res = App.AssetManager.GetResEntry(antSetAsset.AssetBankResource);
                s = App.AssetManager.GetRes(res);
            }
            else
            {
                App.Logger.Log("Asset does not reference a bank");
                return false;
            }

            using (var r = new NativeReader(s))
            {
                var bank = new Bank(r);

                var opt = new AnimationOptions();
                opt.Load();

                EbxAssetEntry skelEntry = App.AssetManager.GetEbxEntry(antSetAsset.SkeletonAsset.External.FileGuid);
                var skelEbx = App.AssetManager.GetEbx(skelEntry);
                dynamic skel = (dynamic)skelEbx.RootObject;

                var skeleton = SkeletonAsset.ConvertToInternal(skel);
                foreach (var dat in bank.Data)
                {
                    if (dat.Value is AnimationAsset anim)
                    {
                        anim.Channels = anim.GetChannels(anim.ChannelToDofAsset);
                        var intern = anim.ConvertToInternal();
                        new AnimationExporterSEANIM().Export(intern, skeleton, Path.GetDirectoryName(path));
                    }
                }
            }

            MessageBox.Show($"Exported {entry.Name} for {ProfilesLibrary.ProfileName}", "Test");

            return true;
        }
    }
}
