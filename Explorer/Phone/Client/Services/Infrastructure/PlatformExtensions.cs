using System;
using Wave.Platform.Messaging;

namespace Wave.Services
{
    public static class PlatformExtensions
    {
        public static void Send(this WaveMessage msg, WaveServerComponent destination, Enum msgID)
        {
            Core.SendMessage(destination, msgID, msg);
        }
    }
}
