using MetroSet_UI.Forms;
using System;
using System.Numerics;
using System.Windows.Forms;

namespace GFDStudio.GUI.Forms
{
    public partial class SetRotationValueDialog : MetroSetForm
    {
        public ResultValue Result { get; private set; }

        public SetRotationValueDialog()
        {
            InitializeComponent();
            Theme.Apply( this );
        }

        private void OKButton_Click( object sender, EventArgs e )
        {
            Result = new ResultValue( this );
        }

        public class ResultValue
        {
            private readonly SetRotationValueDialog mParent;

            internal ResultValue( SetRotationValueDialog parent )
            {
                mParent = parent;
            }

            public Vector3 Rotation => new Vector3 { 
                X = Convert.ToSingle( mParent.num_PosX.Value ), 
                Y = Convert.ToSingle( mParent.num_PosY.Value ), 
                Z = Convert.ToSingle( mParent.num_PosZ.Value ) };

        }
    }
}
