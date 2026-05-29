using System.IO;
using System.Windows.Forms;
using GFDLibrary;
using GFDLibrary.Conversion.AssimpNet;
using GFDLibrary.Conversion.FbxSdk;

namespace GFDStudio.FormatModules
{
    public static class ModelPackExportHelper
    {
        public static void ExportFile( ModelPack modelPack, string path )
        {
            var ext = Path.GetExtension( path );
            if ( ext.Equals( ".fbx", System.StringComparison.OrdinalIgnoreCase ) )
            {
                var config = new FbxSdkModelPackExporterConfig();

                if ( modelPack.AnimationPack != null && modelPack.AnimationPack.Animations.Count > 0 )
                {
                    var result = MessageBox.Show(
                        "Model contains embedded animations. Export model with first animation applied?",
                        "Export animation",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question );

                    config.ExportAnimation = result == DialogResult.Yes;
                }

                FbxSdkModelPackExporter.ExportFile( modelPack, path, config );
            }
            else
            {
                AssimpNetModelPackExporter.ExportFile( modelPack, path );
            }
        }
    }
}
