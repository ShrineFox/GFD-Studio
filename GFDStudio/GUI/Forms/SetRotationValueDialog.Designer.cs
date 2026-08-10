using MetroSet_UI.Controls;
using MetroSet_UI.Forms;

namespace GFDStudio.GUI.Forms
{
    partial class SetRotationValueDialog : MetroSetForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if ( disposing && ( components != null ) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( SetRotationValueDialog ) );
            tableLayoutPanel_Scale = new System.Windows.Forms.TableLayoutPanel();
            lbl_ScaleZ = new MetroSetLabel();
            lbl_scaleX = new MetroSetLabel();
            num_PosX = new System.Windows.Forms.NumericUpDown();
            num_PosZ = new System.Windows.Forms.NumericUpDown();
            num_PosY = new System.Windows.Forms.NumericUpDown();
            lbl_ScaleY = new MetroSetLabel();
            lbl_Rotation = new MetroSetLabel();
            CancelButton = new System.Windows.Forms.Button();
            OKButton = new System.Windows.Forms.Button();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            tableLayoutPanel_Scale.SuspendLayout();
            ( (System.ComponentModel.ISupportInitialize)num_PosX ).BeginInit();
            ( (System.ComponentModel.ISupportInitialize)num_PosZ ).BeginInit();
            ( (System.ComponentModel.ISupportInitialize)num_PosY ).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel_Scale
            // 
            tableLayoutPanel_Scale.ColumnCount = 2;
            tableLayoutPanel1.SetColumnSpan( tableLayoutPanel_Scale, 2 );
            tableLayoutPanel_Scale.ColumnStyles.Add( new System.Windows.Forms.ColumnStyle( System.Windows.Forms.SizeType.Percent, 19.52663F ) );
            tableLayoutPanel_Scale.ColumnStyles.Add( new System.Windows.Forms.ColumnStyle( System.Windows.Forms.SizeType.Percent, 80.47337F ) );
            tableLayoutPanel_Scale.Controls.Add( lbl_ScaleZ, 0, 3 );
            tableLayoutPanel_Scale.Controls.Add( lbl_scaleX, 0, 1 );
            tableLayoutPanel_Scale.Controls.Add( num_PosX, 1, 1 );
            tableLayoutPanel_Scale.Controls.Add( num_PosZ, 1, 3 );
            tableLayoutPanel_Scale.Controls.Add( num_PosY, 1, 2 );
            tableLayoutPanel_Scale.Controls.Add( lbl_ScaleY, 0, 2 );
            tableLayoutPanel_Scale.Controls.Add( lbl_Rotation, 1, 0 );
            tableLayoutPanel_Scale.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel_Scale.Location = new System.Drawing.Point( 4, 5 );
            tableLayoutPanel_Scale.Margin = new System.Windows.Forms.Padding( 4, 5, 4, 5 );
            tableLayoutPanel_Scale.Name = "tableLayoutPanel_Scale";
            tableLayoutPanel_Scale.RowCount = 5;
            tableLayoutPanel_Scale.RowStyles.Add( new System.Windows.Forms.RowStyle( System.Windows.Forms.SizeType.Percent, 20F ) );
            tableLayoutPanel_Scale.RowStyles.Add( new System.Windows.Forms.RowStyle( System.Windows.Forms.SizeType.Percent, 20F ) );
            tableLayoutPanel_Scale.RowStyles.Add( new System.Windows.Forms.RowStyle( System.Windows.Forms.SizeType.Percent, 20F ) );
            tableLayoutPanel_Scale.RowStyles.Add( new System.Windows.Forms.RowStyle( System.Windows.Forms.SizeType.Percent, 20F ) );
            tableLayoutPanel_Scale.RowStyles.Add( new System.Windows.Forms.RowStyle( System.Windows.Forms.SizeType.Percent, 20F ) );
            tableLayoutPanel_Scale.Size = new System.Drawing.Size( 343, 298 );
            tableLayoutPanel_Scale.TabIndex = 0;
            // 
            // lbl_ScaleZ
            // 
            lbl_ScaleZ.Anchor = System.Windows.Forms.AnchorStyles.Right;
            lbl_ScaleZ.AutoSize = true;
            lbl_ScaleZ.Font = new System.Drawing.Font( "Microsoft Sans Serif", 10F );
            lbl_ScaleZ.IsDerivedStyle = true;
            lbl_ScaleZ.Location = new System.Drawing.Point( 44, 196 );
            lbl_ScaleZ.Margin = new System.Windows.Forms.Padding( 4, 0, 4, 0 );
            lbl_ScaleZ.Name = "lbl_ScaleZ";
            lbl_ScaleZ.Size = new System.Drawing.Size( 18, 20 );
            lbl_ScaleZ.Style = MetroSet_UI.Enums.Style.Dark;
            lbl_ScaleZ.StyleManager = null;
            lbl_ScaleZ.TabIndex = 8;
            lbl_ScaleZ.Text = "Z";
            lbl_ScaleZ.ThemeAuthor = "Narwin";
            lbl_ScaleZ.ThemeName = "MetroDark";
            // 
            // lbl_scaleX
            // 
            lbl_scaleX.Anchor = System.Windows.Forms.AnchorStyles.Right;
            lbl_scaleX.AutoSize = true;
            lbl_scaleX.Font = new System.Drawing.Font( "Microsoft Sans Serif", 10F );
            lbl_scaleX.IsDerivedStyle = true;
            lbl_scaleX.Location = new System.Drawing.Point( 42, 78 );
            lbl_scaleX.Margin = new System.Windows.Forms.Padding( 4, 0, 4, 0 );
            lbl_scaleX.Name = "lbl_scaleX";
            lbl_scaleX.Size = new System.Drawing.Size( 20, 20 );
            lbl_scaleX.Style = MetroSet_UI.Enums.Style.Dark;
            lbl_scaleX.StyleManager = null;
            lbl_scaleX.TabIndex = 3;
            lbl_scaleX.Text = "X";
            lbl_scaleX.ThemeAuthor = "Narwin";
            lbl_scaleX.ThemeName = "MetroDark";
            // 
            // num_PosX
            // 
            num_PosX.Anchor =  System.Windows.Forms.AnchorStyles.Left  |  System.Windows.Forms.AnchorStyles.Right ;
            num_PosX.DecimalPlaces = 6;
            num_PosX.Location = new System.Drawing.Point( 70, 72 );
            num_PosX.Margin = new System.Windows.Forms.Padding( 4, 5, 4, 5 );
            num_PosX.Maximum = new decimal( new int[] { 999999, 0, 0, 0 } );
            num_PosX.Minimum = new decimal( new int[] { 999999, 0, 0, int.MinValue } );
            num_PosX.Name = "num_PosX";
            num_PosX.Size = new System.Drawing.Size( 269, 32 );
            num_PosX.TabIndex = 4;
            // 
            // num_PosZ
            // 
            num_PosZ.Anchor =  System.Windows.Forms.AnchorStyles.Left  |  System.Windows.Forms.AnchorStyles.Right ;
            num_PosZ.DecimalPlaces = 6;
            num_PosZ.Location = new System.Drawing.Point( 70, 190 );
            num_PosZ.Margin = new System.Windows.Forms.Padding( 4, 5, 4, 5 );
            num_PosZ.Maximum = new decimal( new int[] { 999999, 0, 0, 0 } );
            num_PosZ.Minimum = new decimal( new int[] { 999999, 0, 0, int.MinValue } );
            num_PosZ.Name = "num_PosZ";
            num_PosZ.Size = new System.Drawing.Size( 269, 32 );
            num_PosZ.TabIndex = 9;
            // 
            // num_PosY
            // 
            num_PosY.Anchor =  System.Windows.Forms.AnchorStyles.Left  |  System.Windows.Forms.AnchorStyles.Right ;
            num_PosY.DecimalPlaces = 6;
            num_PosY.Location = new System.Drawing.Point( 70, 131 );
            num_PosY.Margin = new System.Windows.Forms.Padding( 4, 5, 4, 5 );
            num_PosY.Maximum = new decimal( new int[] { 999999, 0, 0, 0 } );
            num_PosY.Minimum = new decimal( new int[] { 999999, 0, 0, int.MinValue } );
            num_PosY.Name = "num_PosY";
            num_PosY.Size = new System.Drawing.Size( 269, 32 );
            num_PosY.TabIndex = 7;
            // 
            // lbl_ScaleY
            // 
            lbl_ScaleY.Anchor = System.Windows.Forms.AnchorStyles.Right;
            lbl_ScaleY.AutoSize = true;
            lbl_ScaleY.Font = new System.Drawing.Font( "Microsoft Sans Serif", 10F );
            lbl_ScaleY.IsDerivedStyle = true;
            lbl_ScaleY.Location = new System.Drawing.Point( 43, 137 );
            lbl_ScaleY.Margin = new System.Windows.Forms.Padding( 4, 0, 4, 0 );
            lbl_ScaleY.Name = "lbl_ScaleY";
            lbl_ScaleY.Size = new System.Drawing.Size( 19, 20 );
            lbl_ScaleY.Style = MetroSet_UI.Enums.Style.Dark;
            lbl_ScaleY.StyleManager = null;
            lbl_ScaleY.TabIndex = 6;
            lbl_ScaleY.Text = "Y";
            lbl_ScaleY.ThemeAuthor = "Narwin";
            lbl_ScaleY.ThemeName = "MetroDark";
            // 
            // lbl_Rotation
            // 
            lbl_Rotation.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lbl_Rotation.AutoSize = true;
            lbl_Rotation.Font = new System.Drawing.Font( "Microsoft Sans Serif", 10F );
            lbl_Rotation.IsDerivedStyle = true;
            lbl_Rotation.Location = new System.Drawing.Point( 70, 19 );
            lbl_Rotation.Margin = new System.Windows.Forms.Padding( 4, 0, 4, 0 );
            lbl_Rotation.Name = "lbl_Rotation";
            lbl_Rotation.Size = new System.Drawing.Size( 71, 20 );
            lbl_Rotation.Style = MetroSet_UI.Enums.Style.Dark;
            lbl_Rotation.StyleManager = null;
            lbl_Rotation.TabIndex = 5;
            lbl_Rotation.Text = "Rotation";
            lbl_Rotation.ThemeAuthor = "Narwin";
            lbl_Rotation.ThemeName = "MetroDark";
            // 
            // CancelButton
            // 
            CancelButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            CancelButton.ForeColor = System.Drawing.Color.FromArgb( 30, 30, 30 );
            CancelButton.Location = new System.Drawing.Point( 14, 323 );
            CancelButton.Margin = new System.Windows.Forms.Padding( 13, 15, 13, 15 );
            CancelButton.Name = "CancelButton";
            CancelButton.Size = new System.Drawing.Size( 146, 47 );
            CancelButton.TabIndex = 1;
            CancelButton.Text = "Cancel";
            CancelButton.UseVisualStyleBackColor = true;
            // 
            // OKButton
            // 
            OKButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            OKButton.ForeColor = System.Drawing.Color.FromArgb( 30, 30, 30 );
            OKButton.Location = new System.Drawing.Point( 206, 323 );
            OKButton.Margin = new System.Windows.Forms.Padding( 13, 15, 13, 15 );
            OKButton.Name = "OKButton";
            OKButton.Size = new System.Drawing.Size( 114, 47 );
            OKButton.TabIndex = 2;
            OKButton.Text = "OK";
            OKButton.UseVisualStyleBackColor = true;
            OKButton.Click +=  OKButton_Click ;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add( new System.Windows.Forms.ColumnStyle( System.Windows.Forms.SizeType.Percent, 33.3333321F ) );
            tableLayoutPanel1.ColumnStyles.Add( new System.Windows.Forms.ColumnStyle( System.Windows.Forms.SizeType.Percent, 33.3333321F ) );
            tableLayoutPanel1.ColumnStyles.Add( new System.Windows.Forms.ColumnStyle( System.Windows.Forms.SizeType.Percent, 33.3333321F ) );
            tableLayoutPanel1.Controls.Add( tableLayoutPanel_Scale, 0, 0 );
            tableLayoutPanel1.Controls.Add( CancelButton, 0, 1 );
            tableLayoutPanel1.Controls.Add( OKButton, 1, 1 );
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point( 2, 0 );
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add( new System.Windows.Forms.RowStyle( System.Windows.Forms.SizeType.Percent, 80F ) );
            tableLayoutPanel1.RowStyles.Add( new System.Windows.Forms.RowStyle( System.Windows.Forms.SizeType.Percent, 20F ) );
            tableLayoutPanel1.Size = new System.Drawing.Size( 351, 385 );
            tableLayoutPanel1.TabIndex = 11;
            // 
            // SetRotationValueDialog
            // 
            AcceptButton = OKButton;
            BackColor = System.Drawing.Color.FromArgb( 30, 30, 30 );
            BackgroundColor = System.Drawing.Color.FromArgb( 30, 30, 30 );
            ClientSize = new System.Drawing.Size( 355, 387 );
            Controls.Add( tableLayoutPanel1 );
            DropShadowEffect = false;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            HeaderHeight = -40;
            Icon = (System.Drawing.Icon)resources.GetObject( "$this.Icon" );
            Margin = new System.Windows.Forms.Padding( 4, 5, 4, 5 );
            Name = "SetRotationValueDialog";
            Opacity = 0.99D;
            Padding = new System.Windows.Forms.Padding( 2, 0, 2, 2 );
            ShowHeader = true;
            ShowLeftRect = false;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Style = MetroSet_UI.Enums.Style.Dark;
            Text = "Rotation";
            ThemeName = "MetroDark";
            tableLayoutPanel_Scale.ResumeLayout( false );
            tableLayoutPanel_Scale.PerformLayout();
            ( (System.ComponentModel.ISupportInitialize)num_PosX ).EndInit();
            ( (System.ComponentModel.ISupportInitialize)num_PosZ ).EndInit();
            ( (System.ComponentModel.ISupportInitialize)num_PosY ).EndInit();
            tableLayoutPanel1.ResumeLayout( false );
            ResumeLayout( false );
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Scale;
        private new System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.Button OKButton;
        private MetroSetLabel lbl_scaleX;
        private MetroSetLabel lbl_ScaleZ;
        private MetroSetLabel lbl_ScaleY;
        private System.Windows.Forms.NumericUpDown num_PosX;
        private System.Windows.Forms.NumericUpDown num_PosY;
        private System.Windows.Forms.NumericUpDown num_PosZ;
        private MetroSetLabel lbl_Rotation;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}