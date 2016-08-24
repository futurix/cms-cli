using System;
using System.Collections.Generic;
using System.Windows.Threading;
using Wave.Common;
using Wave.Platform.Messaging;
using Wave.UI;

namespace Wave.Services
{
    public class RequestManager
    {
        public const double NetworkRequestTimeout = 30;

        private Dictionary<short, RequestData> requests = new Dictionary<short, RequestData>();
        private Dictionary<short, DispatcherTimer> timers = new Dictionary<short, DispatcherTimer>();
        private short nextRequestID = 0;

        public RequestData this[short requestID]
        {
            get
            {
                RequestData res = null;

                requests.TryGetValue(requestID, out res);

                return res;
            }
        }

        public short Add(WaveMessage msg, long sourceViewID, NodeTransition transition, bool isPopup, bool startTimeoutTimer = true)
        {
            short result = -1;
            
            if (Core.UseRequestIDs)
            {
                result = GetNextRequestID();

                requests[result] = new RequestData(sourceViewID, transition, isPopup);
                msg.AddInt16(MessageOutFieldID.RequestID, result);
            }

            if (startTimeoutTimer && (result >= 0))
            {
                // safety first - try to remove old timer
                RemoveTimer(result);
                
                // start timeout timer
                DispatcherTimer timer = new DispatcherTimer();

                timer.Interval = TimeSpan.FromSeconds(NetworkRequestTimeout);
                timer.Tick += (s, e) => OnTimeout(result);

                timers[result] = timer;

                timer.Start();
            }

            return result;
        }

        public long ResolveView(short requestID)
        {
            if (requestID != -1)
            {
                RequestData res = this[requestID];

                if (res != null)
                    return res.ViewID;
                else
                    return -1;
            }
            else
                return Core.UI.RootViewID;
        }

        public RequestData Remove(short requestID)
        {
            RequestData res = this[requestID];
            
            RemoveTimer(requestID);

            if ((requestID >= 0) && requests.ContainsKey(requestID))
                requests.Remove(requestID);

            return res;
        }

        public long RemoveEx(short requestID)
        {
            long res = -1;

            if (requestID != -1)
            {
                RequestData rd = this[requestID];

                if (rd != null)
                    res = rd.ViewID;
            }
            else
                res = Core.UI.RootViewID;

            RemoveTimer(requestID);

            if ((requestID >= 0) && requests.ContainsKey(requestID))
                requests.Remove(requestID);

            return res;
        }

        #region Helper methods

        private void OnTimeout(short requestID)
        {
            DebugHelper.Out("Request timed out.");
            
            // remove timer
            RemoveTimer(requestID);

            // signal view
            View view = Core.UI[RemoveEx(requestID)];

            if (view != null)
                view.SignalNavigationFailure();
        }

        private void RemoveTimer(short requestID)
        {
            if ((requestID >= 0) && timers.ContainsKey(requestID))
            {
                DispatcherTimer timer;

                if (timers.TryGetValue(requestID, out timer))
                {
                    timers.Remove(requestID);

                    timer.Stop();
                    timer = null;
                }
            }
        }

        private short GetNextRequestID()
        {
            if (nextRequestID == short.MaxValue)
                return nextRequestID = 0;
            else
                return ++nextRequestID;
        }

        #endregion
    }

    public class RequestData
    {
        public long ViewID { get; private set; }

        public NodeTransition Transition { get; private set; }
        public bool IsPopup { get; private set; }

        public RequestData()
            : this(-1, NodeTransition.None, false)
        {
        }

        public RequestData(long sourceViewID)
            : this(sourceViewID, NodeTransition.None, false)
        {
        }

        public RequestData(long sourceViewID, NodeTransition transition, bool isPopup)
        {
            ViewID = sourceViewID;
            Transition = transition;
            IsPopup = isPopup;
        }
    }
}
