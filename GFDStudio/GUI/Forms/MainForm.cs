using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GFDLibrary;
using GFDStudio.FormatModules;
using GFDStudio.GUI.Controls;
using GFDStudio.GUI.DataViewNodes;
using GFDStudio.IO;
using Ookii.Dialogs;

namespace GFDStudio.GUI.Forms
{
    public partial class MainForm : Form
    {
        private const string RECENTLY_OPENED_FILES_LIST_FILEPATH = "RecentlyOpenedFiles.txt";

        private RecentlyOpenedFilesList mRecentlyOpenedFilesList;

        public DataTreeView DataTreeView
        {
            get => mDataTreeView;
            private set => mDataTreeView = value;
        }

        public string LastOpenedFilePath { get; private set; }

        //
        // Initialization
        //
        public MainForm()
        {
            InitializeComponent();
            InitializeState();
            InitializeEvents();
        }

        private void InitializeState()
        {

#if DEBUG
            Text = $"{Program.Name} {Program.Version.Major}.{Program.Version.Minor}.{Program.Version.Revision} [DEBUG]";
#else
            Text = $"{Program.Name} {Program.Version.Major}.{Program.Version.Minor}.{Program.Version.Revision}";
#endif

            mRecentlyOpenedFilesList = new RecentlyOpenedFilesList( RECENTLY_OPENED_FILES_LIST_FILEPATH, 10, mOpenToolStripMenuItem.DropDown.Items,
                                                                    HandleOpenToolStripRecentlyOpenedFileClick );
            DataTreeView.LabelEdit = true;
            AllowDrop = true;
        }

        private void InitializeEvents()
        {
            DataTreeView.AfterSelect += HandleTreeViewAfterSelect;
            DataTreeView.UserPropertyChanged += HandleTreeViewUserPropertyChanged;
            DataTreeView.KeyDown += HandleKeyDown;
            mContentPanel.ControlAdded += HandleContentPanelControlAdded;
            mContentPanel.Resize += HandleContentPanelResize;
            DragDrop += HandleDragDrop;
            DragEnter += HandleDragEnter;
        }

        private void HandleKeyDown( object sender, KeyEventArgs e )
        {
            if ( e.Control )
                HandleControlShortcuts( e.KeyData );
        }

        private void HandleControlShortcuts( Keys keys )
        {
            var handled = false;

            // Check main menu strip shortcuts first
            foreach ( var item in MainMenuStrip.Items )
            {
                var menuItem = item as ToolStripMenuItem;
                if ( menuItem?.ShortcutKeys == keys )
                {
                    menuItem.PerformClick();
                    handled = true;
                }
            }

            if ( handled || DataTreeView.SelectedNode?.ContextMenuStrip == null )
                return;

            // If it's not a main menu shortcut, try checking if its a shortcut to one of the selected
            // node's context menu actions.
            foreach ( var item in DataTreeView.SelectedNode.ContextMenuStrip.Items )
            {
                var menuItem = item as ToolStripMenuItem;
                if ( menuItem?.ShortcutKeys == keys )
                    menuItem.PerformClick();
            }
        }

        private void HandleDragDrop( object sender, DragEventArgs e )
        {
            var data = e.Data.GetData( DataFormats.FileDrop );
            if ( data == null )
                return;

            var paths = ( string[] ) data;
            if ( paths.Length == 0 )
                return;

            var path = paths[ 0 ];
            if ( !File.Exists( path ) )
                return;

            OpenFile( path );
        }

        private static void HandleDragEnter( object sender, DragEventArgs e )
        {
            e.Effect = e.Data.GetDataPresent( DataFormats.FileDrop ) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void HandleTreeViewUserPropertyChanged( object sender, DataViewNodePropertyChangedEventArgs args )
        {
            var controls = mContentPanel.Controls.Find( nameof( ModelViewControl ), true );

            if ( controls.Length == 1 )
            {
                var modelViewControl = ( ModelViewControl )controls[ 0 ];
                modelViewControl.LoadModel( ( Model ) DataTreeView.TopNode.Data );
            }
        }

        //
        // De-initialization
        //
        private void DeInitialize()
        {
            mRecentlyOpenedFilesList.Save( RECENTLY_OPENED_FILES_LIST_FILEPATH );
        }

        //
        // Recently opened files list
        //
        private void RecordOpenedFile( string filePath )
        {
            LastOpenedFilePath = filePath;
            mRecentlyOpenedFilesList.Add( filePath );
        }

        //
        // File IO
        //
        public string SelectAndOpenSelectedFile()
        {
            var filePath = SelectFileToOpen();
            if ( filePath != null )
                OpenFile( filePath );

            return filePath;
        }

        public string SelectFileToOpen()
        {
            using ( var dialog = new OpenFileDialog() )
            {
                dialog.Filter = ModuleFilterGenerator.GenerateFilterForAllSupportedImportFormats();
                if ( dialog.ShowDialog() != DialogResult.OK )
                    return null;

                return dialog.FileName;
            }
        }

        public void OpenFile( string filePath )
        {
            if ( !DataViewNodeFactory.TryCreate( filePath, out var node ) )
            {
                MessageBox.Show( "Hee file could not be loaded, ho.", "Error", MessageBoxButtons.OK );
                return;
            }

            RecordOpenedFile( filePath );
            DataTreeView.SetTopNode( node );
            OnDataViewNodeSelected( node );
        }

        public string SelectFileToSaveTo()
        {
            using ( var dialog = new SaveFileDialog() )
            {
                dialog.Filter = ModuleFilterGenerator.GenerateFilter( FormatModuleUsageFlags.Export );
                if ( dialog.ShowDialog() != DialogResult.OK )
                    return null;

                return dialog.FileName;
            }
        }

        public void SaveFile( string filePath )
        {
            var model = (Model)DataTreeView.TopNode.Data;

            //Check if a material's texture is missing
            foreach (Material material in model.Materials.Materials)
            {
                List<string> missingTextures = new List<string>();

                if (material.Flags.HasFlag(MaterialFlags.HasDiffuseMap) && !model.Textures.Keys.Contains(material.DiffuseMap.Name))
                    missingTextures.Add(material.DiffuseMap.Name);
                if (material.Flags.HasFlag(MaterialFlags.HasDetailMap) && !model.Textures.Keys.Contains(material.DetailMap.Name))
                    missingTextures.Add(material.DetailMap.Name);
                if (material.Flags.HasFlag(MaterialFlags.HasGlowMap) && !model.Textures.Keys.Contains(material.GlowMap.Name))
                    missingTextures.Add(material.GlowMap.Name);
                if (material.Flags.HasFlag(MaterialFlags.HasHighlightMap) && !model.Textures.Keys.Contains(material.HighlightMap.Name))
                    missingTextures.Add(material.HighlightMap.Name);
                if (material.Flags.HasFlag(MaterialFlags.HasNightMap) && !model.Textures.Keys.Contains(material.NightMap.Name))
                    missingTextures.Add(material.NightMap.Name);
                if (material.Flags.HasFlag(MaterialFlags.HasNormalMap) && !model.Textures.Keys.Contains(material.NormalMap.Name))
                    missingTextures.Add(material.NormalMap.Name);
                if (material.Flags.HasFlag(MaterialFlags.HasReflectionMap) && !model.Textures.Keys.Contains(material.ReflectionMap.Name))
                    missingTextures.Add(material.ReflectionMap.Name);
                if (material.Flags.HasFlag(MaterialFlags.HasShadowMap) && !model.Textures.Keys.Contains(material.ShadowMap.Name))
                    missingTextures.Add(material.ShadowMap.Name);
                if (material.Flags.HasFlag(MaterialFlags.HasSpecularMap) && !model.Textures.Keys.Contains(material.SpecularMap.Name))
                    missingTextures.Add(material.SpecularMap.Name);
                if (missingTextures.Count > 0)
                    MessageBox.Show($"Material \"{material.Name}\" references one or more Textures that cannot be found:\n{String.Join("\n",missingTextures.ToArray())}" +
                        $"\n\nThis will lead to an inevitable crash in-game.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            //Check if a geometry's material is missing
            foreach (var node in model.Scene.Nodes)
                for (int i = 0; i < node.Attachments.Count; i++)
                {
                    var attachment = node.Attachments[i];

                    switch (attachment.Type)
                    {
                        case NodeAttachmentType.Geometry:
                                Geometry geometry = attachment.GetValue<Geometry>();
                                if (!model.Materials.Keys.Contains(geometry.MaterialName))
                                    MessageBox.Show($"Scene Geometry under \"{node.Name}\" references a Material that cannot be found:\n{geometry.MaterialName}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            break;
                    }
                }

            if ( DataTreeView.Nodes.Count > 0 )
                DataTreeView.TopNode.Export( filePath );
        }

        public string SelectFileAndSave()
        {
            if ( DataTreeView.Nodes.Count > 0 )
                return DataTreeView.TopNode.Export();

            return null;
        }

        private void ClearContentPanel()
        {
            foreach ( var control in mContentPanel.Controls )
            {
                ( control as IDisposable )?.Dispose();
            }

            mContentPanel.Controls.Clear();
        }

        //
        // Event handlers
        //
        protected override void OnClosed( EventArgs e )
        {
            DeInitialize();

            // exit application when the main form is closed
            Application.Exit();
        }

        private void HandleTreeViewAfterSelect(object sender, TreeViewEventArgs e)
        {
            var node = ( DataViewNode )e.Node;
            OnDataViewNodeSelected( node );
        }

        private void OnDataViewNodeSelected( DataViewNode node )
        {
            // Set property grid to display properties of the currently selected node
            mPropertyGrid.SelectedObject = node;

            Control control = null;

            if ( FormatModuleRegistry.ModuleByType.TryGetValue( node.DataType, out var module ) )
            {
                if ( module.UsageFlags.HasFlag( FormatModuleUsageFlags.Bitmap ) )
                {
                    control = new BitmapViewControl( module.GetBitmap( node.Data ) );
                    control.Visible = false;
                }
                else if ( module.ModelType == typeof( Model ) )
                {
                    ClearContentPanel();
                    var modelViewControl = new ModelViewControl();
                    modelViewControl.Name = nameof( ModelViewControl );
                    modelViewControl.Visible = false;
                    modelViewControl.LoadModel( ( Model )node.Data );
                    control = modelViewControl;
                }
            }

            if ( control != null )
            {
                // Clear the content panel
                ClearContentPanel();
                mContentPanel.Controls.Add( control );
            }
        }

        private void HandleOpenToolStripMenuItemClick(object sender, EventArgs e)
        {
            mFileToolStripMenuItem.DropDown.Close();
            SelectAndOpenSelectedFile();
        }

        private void HandleOpenToolStripRecentlyOpenedFileClick( object sender, EventArgs e )
        {
            mFileToolStripMenuItem.DropDown.Close();
            OpenFile( (( ToolStripMenuItem )sender).Text );
        }

        private void HandleContentPanelControlAdded( object sender, ControlEventArgs e )
        {
            e.Control.Left = ( mContentPanel.Width - e.Control.Width ) / 2;
            e.Control.Top = ( mContentPanel.Height - e.Control.Height ) / 2;
            e.Control.Visible = true;
        }

        private void HandleContentPanelResize( object sender, EventArgs e )
        {
            foreach ( Control control in mContentPanel.Controls )
            {
                control.Visible = false;
                control.Left = ( mContentPanel.Width - control.Width ) / 2;
                control.Top = ( mContentPanel.Height - control.Height ) / 2;
                control.Visible = true;
            }
        }

        private void HandleSaveToolStripMenuItemClick( object sender, EventArgs e )
        {
            if ( LastOpenedFilePath != null )
            {
                SaveFile( LastOpenedFilePath );
            }
            else if ( DataTreeView.TopNode != null )
            {
                MessageBox.Show( "No hee file opened, ho! Use the 'Save as...' option instead.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
            }
            else
            {
                MessageBox.Show( "Nothing to save, hee ho!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
            }
        }

        private void HandleSaveAsToolStripMenuItemClick( object sender, EventArgs e )
        {
            if ( DataTreeView.TopNode == null )
            {
                MessageBox.Show( "Nothing to save, hee ho!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
                return;
            }

            var path = SelectFileAndSave();
            if ( path != null )
            {
                OpenFile( path );
            }
        }

        private void HandleNewModelToolStripMenuItemClick( object sender, EventArgs e )
        {
            var model = ModelConverterUtility.ConvertAssimpModel();
            if ( model != null )
            {
                var node = DataViewNodeFactory.Create( "Model", model );
                DataTreeView.SetTopNode( node );
                LastOpenedFilePath = null;
            }
        }

        private void HandleMakeAnimationsRelativeToolStripMenuItemClick( object sender, EventArgs e )
        {
            var originalScene = ModuleImportUtilities.SelectImportFile<Model>( "Select the original model file." )?.Scene;
            if ( originalScene == null )
                return;

            var newScene = ModuleImportUtilities.SelectImportFile<Model>( "Select the new model file." )?.Scene;
            if ( newScene == null )
                return;

            bool fixArms = MessageBox.Show( "Fix arms? If unsure, select No.", "Question", MessageBoxButtons.YesNo,
                                            MessageBoxIcon.Question, MessageBoxDefaultButton.Button2 ) == DialogResult.Yes;

            string directoryPath;
            using ( var dialog = new VistaFolderBrowserDialog() )
            {
                dialog.Description =
                    "Select a directory containing GAP files, or subdirectories containing GAP files to make relative to the new model.\n" +
                    "Note that this will replace the original files.";

                if ( dialog.ShowDialog() != DialogResult.OK )
                    return;

                directoryPath = dialog.SelectedPath;
            }

            var failures = new ConcurrentBag<string>();

            using ( var dialog = new ProgressDialog() )
            {
                dialog.DoWork += ( o, progress ) =>
                {
                    var filePaths = Directory.EnumerateFiles( directoryPath, "*.GAP", SearchOption.AllDirectories ).ToList();
                    var processedFileCount = 0;

                    Parallel.ForEach( filePaths, ( filePath, state ) =>
                    {
                        lock ( dialog )
                        {
                            if ( dialog.CancellationPending )
                            {
                                state.Stop();
                                return;
                            }

                            dialog.ReportProgress( ( int ) ( ( ( float ) ++processedFileCount / filePaths.Count ) * 100 ),
                                                   $"Processing {Path.GetFileName( filePath )}", null );
                        }

                        try
                        {
                            var animationPack = Resource.Load<AnimationPack>( filePath );
                            animationPack.MakeTransformsRelative( originalScene, newScene, fixArms );
                            animationPack.Save( filePath );
                        }
                        catch ( Exception ex )
                        {
                            failures.Add( filePath );
                        }
                    } );
                };

                dialog.ShowDialog();
            }

            if ( failures.Count > 0 )
            {
                MessageBox.Show( "An error occured while processing the following files:\n" + string.Join( "\n", failures ) );
            }
        }

        private void HandleMakeModelsAnimationsRelativeToolStripMenuItemClick(object sender, EventArgs e)
        {
            string newModelDirectoryPath;
            using (var dialog = new VistaFolderBrowserDialog())
            {
                dialog.Description =
                    "Select a character's new model folder.";

                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                newModelDirectoryPath = dialog.SelectedPath;
            }

            string oldModelDirectoryPath;
            using (var dialog = new VistaFolderBrowserDialog())
            {
                dialog.Description =
                    "Select the character's original model folder.";

                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                oldModelDirectoryPath = dialog.SelectedPath;
            }

            var failures = new ConcurrentBag<string>();

            foreach (string modelPath in Directory.GetFiles(newModelDirectoryPath))
            {
                string originalModelPath = $"{oldModelDirectoryPath}\\{Path.GetFileName(modelPath)}";

                var originalScene = ModuleImportUtilities.ImportFile<Model>(originalModelPath)?.Scene;
                if (originalScene == null)
                    return;

                var newScene = ModuleImportUtilities.ImportFile<Model>(modelPath)?.Scene;
                if (newScene == null)
                    return;

                string modelCostumeID = Path.GetFileNameWithoutExtension(modelPath).Split('_')[1];

                using (var dialog = new ProgressDialog())
                {
                    dialog.DoWork += (o, progress) =>
                    {
                        var filePaths = Directory.EnumerateFiles(newModelDirectoryPath, $"*_051_{modelCostumeID}*.GAP", SearchOption.AllDirectories).ToList();
                        var processedFileCount = 0;

                        Parallel.ForEach(filePaths, (filePath, state) =>
                        {
                            lock (dialog)
                            {
                                if (dialog.CancellationPending)
                                {
                                    state.Stop();
                                    return;
                                }

                                dialog.ReportProgress((int)(((float)++processedFileCount / filePaths.Count) * 100),
                                                       $"Processing {Path.GetFileName(filePath)}", null);
                            }

                            try
                            {
                                var animationPack = Resource.Load<AnimationPack>(filePath);
                                animationPack.MakeTransformsRelative(originalScene, newScene, false);
                                animationPack.Save(filePath);
                            }
                            catch (Exception ex)
                            {
                                failures.Add(filePath);
                            }
                        });
                    };

                    dialog.ShowDialog();
                }
            }

            if (failures.Count > 0)
            {
                MessageBox.Show("An error occured while processing the following files:\n" + string.Join("\n", failures));
            }
        }
    }
}
