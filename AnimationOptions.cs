using Frosty.Core.Controls.Editors;
using Frosty.Core;
using FrostySdk.Attributes;
using FrostySdk.IO;
using MeshSetPlugin.Editors;
using Frosty.Core.Controls;

namespace AssetBankPlugin
{
    [DisplayName("Animation Options")]
    public class AnimationOptions : OptionsExtension
    {
        [Category("Export/Import")]
        [DisplayName("Export Skeleton")]
        [Description("Determines the default skeleton selected when exporting animations.")]
        [Editor(typeof(FrostySkeletonEditor))]
        public string ExportSkeletonAsset { get; set; }
        
        [Category("Export/Import")]
        [DisplayName("Default AntStateAsset")]
        [Description("The AntStateAsset that stores the bone indices. Usually found in \"levels/frontend/\".")]
        [Editor(typeof(FrostyStringEditor))]
        public string AntStateAsset { get; set; }

        public override void Load()
        {
            ExportSkeletonAsset = Config.Get<string>("AnimationExportSkeleton", "", ConfigScope.Game);
            AntStateAsset = Config.Get<string>("AntStateAsset", "", ConfigScope.Game);
        }

        public override void Save()
        {
            Config.Add("AnimationExportSkeleton", ExportSkeletonAsset, ConfigScope.Game);
            Config.Add("AntStateAsset", AntStateAsset, ConfigScope.Game);
            Config.Save();
        }

        public override bool Validate() { return true; }
    }
}
