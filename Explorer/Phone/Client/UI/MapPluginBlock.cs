using System;
using System.Device.Location;
using System.Windows;
using Microsoft.Phone.Controls.Maps;
using Wave.Platform;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.UI
{
    public class MapPluginBlock : BlockBase
    {
        private const string BingAppID = "Ap2SPMR4hW4MW5f96qwrYV3R7DYzOSqGz9cMxUe6WVX-BCUv7h9zgmzNbQaKvW5G"; //TODO: this probably should be a configuration setting

        private MapPluginBlockDefinition data = null;
        private Map map = null;
        
        public MapPluginBlock(View hostView, Node parentNode, BlockBase parentBlock, BlockDefinition definition, FieldList content, bool isRoot)
            : base(hostView, parentNode, parentBlock, definition, content, isRoot)
        {
            data = Definition as MapPluginBlockDefinition;
            
            // creating map control
            map = new Map();
            map.CredentialsProvider = new ApplicationIdCredentialsProvider(BingAppID);

            Children.Add(map);

            if (data != null)
            {
                map.AnimationLevel = data.IsAnimationEnabled ? AnimationLevel.Full : AnimationLevel.None;

                switch (data.Mode)
                {
                    default:
                    case WaveMapMode.Standard:
                        map.Mode = new RoadMode();
                        break;

                    case WaveMapMode.Satellite:
                        map.Mode = new AerialMode(false);
                        break;

                    case WaveMapMode.Hybrid:
                        map.Mode = new AerialMode(true);
                        break;
                }
            }

            if (Content != null)
            {
                int latitude = Content[DefAgentFieldID.MapLatitude].AsNumber() ?? -1;
                int longitude = Content[DefAgentFieldID.MapLongitude].AsNumber() ?? -1;

                if ((latitude != -1) && (longitude != -1))
                    map.Center = new GeoCoordinate(
                        (double)latitude / (double)LocationObserver.DegreesToMicrodegrees, 
                        (double)longitude / (double)LocationObserver.DegreesToMicrodegrees);

                map.ZoomLevel = 15; //HACK
            }
        }

        #region Layout

        protected override Size MeasureOverride(Size availableSize)
        {
            if (Double.IsInfinity(availableSize.Width) || Double.IsInfinity(availableSize.Height) || (availableSize.Width == 0) || (availableSize.Height == 0))
            {
                return new Size(0, 0);
            }
            else
            {
                map.Measure(availableSize);

                return availableSize;
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            map.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));

            return finalSize;
        }

        #endregion
    }
}
