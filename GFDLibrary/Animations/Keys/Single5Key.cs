using GFDLibrary.IO;

namespace GFDLibrary.Animations
{
    public class Single5Key : Key
    {
        public float UVOffsetX { get; set; }

        public float UVOffsetY { get; set; }

        public float UVScaleX { get; set; }

        public float UVScaleY { get; set; }

        public float UVRotation { get; set; }

        public Single5Key() : this( KeyType.Single5 ) { }

        public Single5Key( KeyType type ) : base( type )
        {
        }

        internal override void Read( ResourceReader reader )
        {
            UVOffsetX = reader.ReadSingle();
            UVOffsetY = reader.ReadSingle();
            UVScaleX  = reader.ReadSingle();
            UVScaleY  = reader.ReadSingle();
            UVRotation = reader.ReadSingle();
        }

        internal override void Write( ResourceWriter writer )
        {
            writer.WriteSingle( UVOffsetX );
            writer.WriteSingle( UVOffsetY );
            writer.WriteSingle( UVScaleX );
            writer.WriteSingle( UVScaleY );
            writer.WriteSingle( UVRotation );
        }
    }
}
