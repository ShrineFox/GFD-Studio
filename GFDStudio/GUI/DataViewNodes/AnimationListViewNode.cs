using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using GFDLibrary;
using GFDLibrary.Animations;
using GFDLibrary.Models;
using GFDStudio.FormatModules;
using Ookii.Dialogs;
using Ookii.Dialogs.Wpf;

namespace GFDStudio.GUI.DataViewNodes
{
    public class AnimationListViewNode : ListViewNode<Animation>
    {
        public AnimationListViewNode( string text, List<Animation> data, ListItemNameProvider<Animation> nameProvider ) : base( text, data, nameProvider )
        {
        }

        public AnimationListViewNode( string text, List<Animation> data, IList<string> itemNames ) : base( text, data, itemNames )
        {
        }

        protected override void InitializeCore()
        {
            RegisterAddHandler<Animation>( file =>
            {
                var animation = Resource.Load<Animation>( file );
                Data.Add( animation );
            } );
            RegisterCustomHandler( "Export", "All", () =>
            {
                var dialog = new VistaFolderBrowserDialog();
                {
                    if ( dialog.ShowDialog() != true )
                        return;

                    foreach ( AnimationViewNode animationViewModel in Nodes )
                        animationViewModel.Data.Save( Path.Combine( dialog.SelectedPath, animationViewModel.Text + ".ganm" ) );
                }
            } );
            RegisterCustomHandler( "Export", "All (FBX)", () =>
            {
                var skeleton = ResolveSkeletonOrPrompt();
                if ( skeleton == null )
                    return;

                var dialog = new VistaFolderBrowserDialog();
                if ( dialog.ShowDialog() != true )
                    return;

                foreach ( AnimationViewNode animationViewModel in Nodes )
                {
                    var animName = animationViewModel.Text;
                    var fbxPath = Path.Combine( dialog.SelectedPath, animName + ".fbx" );
                    try
                    {
                        AnimationExportHelper.ExportFile( animationViewModel.Data, skeleton, animName, fbxPath );
                    }
                    catch ( Exception ex )
                    {
                        MessageBox.Show( $"Failed to export {animName}: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error );
                        return;
                    }
                }
            } );
            RegisterCustomHandler( "Add", "New animation", () =>
            {
                Data.Add( new Animation() );
                InitializeView( true );
            } );

            base.InitializeCore();
        }

        private Model ResolveSkeletonOrPrompt()
        {
            // Priority 1: model pack containing the animations
            var ancestor = Parent;
            while ( ancestor != null )
            {
                if ( ancestor is ModelPackViewNode modelPackNode && modelPackNode.Model?.Data != null )
                    return modelPackNode.Model.Data;
                ancestor = ancestor.Parent;
            }

            // Priority 2: model currently loaded in the model editor
            var editorModel = Controls.ModelViewControl.Instance.Model;
            if ( editorModel != null )
                return editorModel;

            // Priority 3: prompt the user for a model file
            var modelPack = ModuleImportUtilities.SelectImportFile<ModelPack>( "Select the model containing the skeleton for these animations." );
            return modelPack?.Model;
        }
    }
}