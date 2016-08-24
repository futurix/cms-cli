using System.IO;
using Wave.Common;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.Platform
{
    public class MapPluginBlockDefinition : BlockDefinition, ICacheable
    {
        public bool IsZoomEnabled { get; private set; }
        public bool IsScrollEnabled { get; private set; }
        public bool IsAnimationEnabled { get; private set; }
        public bool ShowUserLocation { get; private set; }
        public WaveMapMode Mode { get; private set; }

        public PaintStyle? Background { get; protected set; }
        
        public MapPluginBlockDefinition()
            : base()
        {
        }

        public void Unpack(FieldList source)
        {
            if (source == null)
                return;

            UnpackDefinitionID(source);

            Background = new PaintStyle(source, DefAgentFieldID.BackgroundPaintStyle);
            
            IsZoomEnabled = source[DefAgentFieldID.MapZoomEnabled].AsBoolean() ?? true;
            IsScrollEnabled = source[DefAgentFieldID.MapScrollEnabled].AsBoolean() ?? true;
            IsAnimationEnabled = source[DefAgentFieldID.MapAnimationsEnabled].AsBoolean() ?? true;
            ShowUserLocation = source[DefAgentFieldID.MapShowUserLocEnabled].AsBoolean() ?? false;
            Mode = (WaveMapMode)(source[DefAgentFieldID.MapMode].AsShort() ?? (short)WaveMapMode.Standard);

            UnpackBlockHints(source);

            // done
            IsUnpacked = true;
        }

        #region ICacheable implementation

        public override CacheableType StoredType
        {
            get { return CacheableType.MapPluginBlockDefinition; }
        }

        public override void Persist(Stream str)
        {
            base.Persist(str);

            str.WriteByte(0);

            // background
            str.WriteBool(Background.HasValue);

            if (Background.HasValue)
                Background.Value.Persist(str);

            // other settings
            str.WriteBool(IsZoomEnabled);
            str.WriteBool(IsScrollEnabled);
            str.WriteBool(IsAnimationEnabled);
            str.WriteBool(ShowUserLocation);
            str.WriteShort((short)Mode);
        }

        public override void Restore(Stream str)
        {
            base.Restore(str);

            if (str.ReadByte() == 0)
            {
                // background
                if (str.ReadBool())
                {
                    PaintStyle bg = new PaintStyle();
                    bg.Restore(str);

                    Background = bg;
                }
                else
                    Background = null;

                // other settings
                IsZoomEnabled = str.ReadBool();
                IsScrollEnabled = str.ReadBool();
                IsAnimationEnabled = str.ReadBool();
                ShowUserLocation = str.ReadBool();
                Mode = (WaveMapMode)str.ReadShort();
            }
        }

        #endregion
    }

    public enum WaveMapMode : short
    {
        Standard = 1,
        Satellite = 2,
        Hybrid = 3
    }
}
