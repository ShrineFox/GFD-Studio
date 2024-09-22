using System;
using System.Globalization;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Linq;
using GFDLibrary;
using GFDLibrary.Materials;
using MetroSet_UI.Forms;
using YamlDotNet.Serialization;
using System.Collections.Generic;

namespace GFDStudio.GUI.Forms
{
    public partial class ModelConverterOptionsDialog : MetroSetForm
    {

        private static string PresetLibraryPath = System.IO.Path.GetDirectoryName( Application.ExecutablePath ) + @"\Presets\";
        private static List<string> Presets = Directory.GetFiles( PresetLibraryPath, "*.yml", SearchOption.TopDirectoryOnly )
            .Select(x => Path.GetFileNameWithoutExtension(x) ).ToList();

        public object MaterialPreset
            => YamlSerializer.LoadYamlFile<Material>( PresetLibraryPath + Presets[MaterialPresetComboBox.SelectedIndex] );


        string CurrentPath = System.IO.Path.GetDirectoryName( Application.ExecutablePath );


        public uint Version
        {
            get
            {
                uint version;

                if ( VersionTextBox.Text.StartsWith( "0x" ) )
                {
                    uint.TryParse( VersionTextBox.Text.Substring( 2 ), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out version );
                }
                else
                {
                    uint.TryParse( VersionTextBox.Text, out version );
                }

                return version;
            }
        }

        public bool ConvertSkinToZUp
        {
            get => ConvertSkinToZUpCheckBox.Checked;
        }

        public bool GenerateVertexColors
        {
            get => GenerateVertexColorsCheckBox.Checked;
        }

        public bool MinimalVertexAttributes
        {
            get => MinimalVertexAttributesCheckBox.Checked;
        }

        public bool AutoAddGFDHelperIDs
        {
            get => AutoAddGFDHelperIDsCheckBox.Checked;
        }

        private void OpenEditPreset( string preset )
        {
            Process PresetEdit = new Process();
            PresetEdit.StartInfo = new ProcessStartInfo( preset )
            {
                UseShellExecute = true
            };
            PresetEdit.Start();
        }

        public ModelConverterOptionsDialog( bool showSceneOptionsOnly, Config settings )
        {
            InitializeComponent();
            Theme.Apply( this );
            VersionTextBox.Text = $"0x{ResourceVersion.Persona5Dancing:X8}";

            if ( showSceneOptionsOnly )
            {
                MaterialPresetComboBox.Enabled = false;
            }
            else
            {
                MaterialPresetComboBox.DataSource = Presets;
                if ( settings .UseDefaultPreset && Presets.Any( x => x.Equals( settings.DefaultPresetName ) ) )
                    MaterialPresetComboBox.SelectedIndex = Presets.IndexOf( settings.DefaultPresetName );
            }
        }

        private void OpenPresetButton_Click( object sender, EventArgs e )
        {
            Process OpenPreset = new Process();
            OpenPreset.StartInfo = new ProcessStartInfo( PresetLibraryPath )
            {
                UseShellExecute = true
            };
            OpenPreset.Start();
        }
    }
}
