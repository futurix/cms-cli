using System;
using System.Device.Location;
using Wave.Common;
using Wave.Platform.Messaging;

namespace Wave.Services
{
    public sealed class LocationObserver : IDisposable
    {
        /// <summary>
        /// Multiplier to turn a value in degrees to microdegrees (1 million).
        /// </summary>
        public const int DegreesToMicrodegrees = 1000000;

        public WaveLocation LatestLocation
        {
            get
            {
                if ((watcher != null) && (watcher.Status == GeoPositionStatus.Ready))
                    return new WaveLocation(watcher.Position.Location.Latitude, watcher.Position.Location.Longitude);
                else
                    return null;
            }
        }

        private GeoCoordinateWatcher watcher = null;
        private TimeSpan maximumDelay = TimeSpan.FromHours(1);

        private bool disposed = false;

        public LocationObserver()
        {
            watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);

            watcher.MovementThreshold = 20; // update location only if it changes by more than 20 metres
            watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);
            watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(watcher_StatusChanged);
        }

        public void Dispose()
        {
            if (!disposed)
            {
                if (watcher != null)
                    watcher.Dispose();

                disposed = true;
            }
        }

        public void Start()
        {
            watcher.Start();
        }

        public void Stop()
        {
            watcher.Stop();
        }

        public void AddLocationData(WaveMessage msg)
        {
            if ((watcher != null) && (watcher.Status == GeoPositionStatus.Ready) && 
                ((DateTimeOffset.UtcNow - watcher.Position.Timestamp) <= maximumDelay))
            {
                msg.AddInt32(DefAgentFieldID.MapLatitude, (int)(watcher.Position.Location.Latitude * DegreesToMicrodegrees));
                msg.AddInt32(DefAgentFieldID.MapLongitude, (int)(watcher.Position.Location.Longitude * DegreesToMicrodegrees));
            }
        }

        private void watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            DebugHelper.Out("Location observer status: {0}", e.Status);

            if ((e.Status == GeoPositionStatus.Disabled) || (e.Status == GeoPositionStatus.NoData))
                Core.NotifyLocationUnavailable(this);
        }

        private void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            DebugHelper.Out("Location changed: {0} on {1}", e.Position.Location, e.Position.Timestamp);

            if (watcher.Status == GeoPositionStatus.Ready)
                Core.NotifyLocationChanged(this);
        }
    }

    public class WaveLocation
    {
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }

        public WaveLocation(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}
