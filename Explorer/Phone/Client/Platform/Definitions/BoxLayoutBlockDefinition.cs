using System.Collections.Generic;
using System.IO;
using Wave.Common;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.Platform
{
    public class BoxLayoutBlockDefinition : ContainerBlockDefinition, ICacheable
    {
        public List<short> SizeRatios { get; private set; }
        
        public BoxLayoutBlockDefinition()
            : base()
        {
            SizeRatios = new List<short>();
        }

        public void Unpack(FieldList source)
        {
            if (source == null)
                return;

            UnpackDefinitionID(source);

            Background = new PaintStyle(source, DefAgentFieldID.BackgroundPaintStyle);
            FirstFocus = source[DefAgentFieldID.FirstFocus].AsByte() ?? (short)-1;
            
            if (FirstFocus == -1)
                FirstFocus = 0;

            BackgroundImageCrop = (CropStrategy)(source[DefAgentFieldID.BackgroundCropStrategy].AsNumber() ?? 0);

            UIFlowDirection fd = (UIFlowDirection)(source[DefAgentFieldID.FlowDirection].AsByte() ?? (byte)UIFlowDirection.Down);
            IsHorizontal = (fd == UIFlowDirection.Right);

            List<Int16Field> rs = source.GetItems<Int16Field>(DefAgentFieldID.RelativeSize);

            foreach (Int16Field field in rs)
                SizeRatios.Add(field.Data);

            UnpackMarginsAndPadding(source);
            UnpackBlockHints(source);

            // done
            IsUnpacked = true;
        }

        #region ICacheable implementation

        public override CacheableType StoredType
        {
            get { return CacheableType.BoxLayoutBlockDefinition; }
        }

        public override void Persist(Stream str)
        {
            base.Persist(str);

            str.WriteByte(0);
            str.WriteInteger(SizeRatios.Count);

            if (SizeRatios.Count > 0)
                foreach (short ratio in SizeRatios)
                    str.WriteShort(ratio);
        }

        public override void Restore(Stream str)
        {
            base.Restore(str);

            if (str.ReadByte() == 0)
            {
                int numberOfItems = str.ReadInteger();

                if (numberOfItems > 0)
                {
                    for (int i = 0; i < numberOfItems; i++)
                        SizeRatios.Add(str.ReadShort());
                }
            }
        }

        #endregion
    }
}
