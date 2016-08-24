using System;
using Wave.Platform.Messaging;

namespace Wave.Services
{
    public interface IMessageEndpoint
    {
        void OnMessageReceived(WaveServerComponent dest, Enum msgID, WaveMessage data);
    }
}
