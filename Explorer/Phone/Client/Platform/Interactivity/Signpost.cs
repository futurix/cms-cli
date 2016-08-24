using System;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.Platform
{
    public class Signpost
    {
        public const int Invalid = -1;
        
        public int ID { get; private set; }

        public string Name { get; private set; }
        public string FrameID { get; private set; }
        
        public Signpost(FieldListNavigator nav)
        {
            if (Core.CSLVersion == WaveCSLVersion.Version5)
            {
                nav.FindNext(NaviAgentFieldID.SignpostSpec);
                FieldList spec = nav.Current.AsFieldList();

                if (spec != null)
                {
                    ID = spec[NaviAgentFieldID.Signpost].AsInteger() ?? FieldList.FieldNotFound;

                    if (ID == FieldList.FieldNotFound)
                    {
                        nav.FindNext(NaviAgentFieldID.FrameID);
                        FrameID = nav.Current.AsString();

                        nav.FindNext(NaviAgentFieldID.SignpostName);
                        Name = nav.Current.AsString();
                    }
                }
            }
            else
            {
                nav.FindNext(NaviAgentFieldID.ActionPayload);
                ID = nav.Current.AsNumber() ?? FieldList.FieldNotFound;
            }
        }

        public override string ToString()
        {
            return String.Format(
                "Signpost: ID -> {0}, name -> {1}, frame ID -> {2}",
                ID,
                (Name != null) ? Name : "none",
                (FrameID != null) ? FrameID : "none");
        }
    }
}
