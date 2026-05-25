using System;
using System.IO;
using GFDLibrary.Animations;
using GFDLibrary.Conversion.FbxSdk;
using GFDLibrary.Models;

namespace GFDStudio.FormatModules
{
    public static class AnimationExportHelper
    {
        public static void ExportFile( Animation animation, Model skeleton, string path )
        {
            ExportFile( animation, skeleton, null, path );
        }

        public static void ExportFile( Animation animation, Model skeleton, string animationName, string path )
        {
            if ( path.EndsWith( ".ascii.fbx", StringComparison.OrdinalIgnoreCase )
                 || Path.GetExtension( path ).Equals( ".fbx", StringComparison.OrdinalIgnoreCase ) )
            {
                if ( skeleton == null )
                    throw new InvalidOperationException( "A skeleton model is required to export an animation to FBX." );

                FbxSdkAnimationExporter.ExportFile( animation, skeleton, animationName, path, new FbxSdkAnimationExporterConfig() );
                return;
            }

            var ext = Path.GetExtension( path );
            if ( ext.Equals( ".dae", StringComparison.OrdinalIgnoreCase )
                 || ext.Equals( ".obj", StringComparison.OrdinalIgnoreCase ) )
            {
                throw new NotSupportedException( $"Exporting animations to {ext} is not supported. Use .fbx instead." );
            }

            animation.Save( path );
        }
    }
}
