using System;
using System.Collections.Generic;
using Wave.Common;
using Wave.Platform;
using Wave.Platform.Messaging;

namespace Wave.Services
{
    public class DefinitionAgent : IMessageEndpoint
    {
        public DefinitionAgent()
        {
        }

        public void OnMessageReceived(WaveServerComponent dest, Enum msgID, WaveMessage data)
        {
            if (msgID is DefAgentMessageID)
            {
                if (data != null)
                {
                    switch ((DefAgentMessageID)msgID)
                    {
                        case DefAgentMessageID.BulkDefinitionResponse:
                            {
                                if (data.RootList.Count > 0)
                                {
                                    List<FieldList> definitions = new List<FieldList>(data.RootList.Count);

                                    foreach (IFieldBase field in data.RootList)
                                    {
                                        BinaryField bin = field as BinaryField;

                                        if ((bin != null) && (bin.Data != null))
                                        {
                                            WaveMessage msg = new WaveMessage(bin.Data);

                                            if ((DefAgentMessageID)msg.MessageID == DefAgentMessageID.DefinitionResponse)
                                                definitions.Add(msg.RootList);
                                        }
                                    }

                                    UnpackDefinitions(definitions);
                                }

                                break;
                            }

                        case DefAgentMessageID.DefinitionResponse:
                            {
                                UnpackDefinition(data.RootList);
                                break;
                            }
                    }
                }
            }
        }

        public DefinitionBase UnpackDefinition(FieldList source)
        {
            int appID = source[MessageOutFieldID.ApplicationID].AsInteger() ?? 0;
            int defID = source[MessageOutFieldID.DefinitionID].AsInteger() ?? 0;

            DefinitionBase liveEntity = CreateDefinition(source);

            if (liveEntity != null)
            {
                // find cache item ID and cache hint
                CacheItemID? ciid = null;
                byte cacheHint = 0;
                
                byte[] ciidData = source[MessageOutFieldID.CacheItemID].AsByteArray();

                if (ciidData != null)
                {
                    ciid = new CacheItemID(ciidData);

                    cacheHint = source[MessageOutFieldID.CacheHint].AsByte() ?? 0;
                }

                // add to cache (global or local)
                if (ciid.HasValue)
                {
                    Core.Cache.Server.Add(
                        liveEntity, ciid, 
                        EntityID.Create(CacheEntityType.Definition, appID, defID), 
                        CacheEntityType.Definition, cacheHint, true);
                }
            }

            return liveEntity;
        }

        public void UnpackDefinitions(List<FieldList> sources)
        {
            if ((sources != null) && (sources.Count > 0))
            {
                List<CacheDataTemplate> templates = new List<CacheDataTemplate>(sources.Count);

                foreach (FieldList source in sources)
                {
                    int appID = source[MessageOutFieldID.ApplicationID].AsInteger() ?? 0;
                    int defID = source[MessageOutFieldID.DefinitionID].AsInteger() ?? 0;

                    DefinitionBase liveEntity = CreateDefinition(source);

                    if (liveEntity != null)
                    {
                        // find cache item ID and cache hint
                        CacheItemID? ciid = null;
                        byte cacheHint = 0;

                        byte[] ciidData = source[MessageOutFieldID.CacheItemID].AsByteArray();

                        if (ciidData != null)
                        {
                            ciid = new CacheItemID(ciidData);

                            cacheHint = source[MessageOutFieldID.CacheHint].AsByte() ?? 0;
                        }

                        if (ciid.HasValue)
                            templates.Add(new CacheDataTemplate(liveEntity, ciid, EntityID.Create(CacheEntityType.Definition, appID, defID), CacheEntityType.Definition, cacheHint));
                    }
                }

                Core.Cache.Server.AddRange(templates, true);
            }
        }

        private DefinitionBase CreateDefinition(FieldList source)
        {
            if (source == null)
                return null;
            
            int defID = source[MessageOutFieldID.DefinitionID].AsInteger() ?? 0;
            short primitiveType = source[DefAgentFieldID.DefinitionType].AsShort() ?? 0;
            short secondaryCharacteristic = 0;

            DefinitionBase res = null;

            switch ((DefinitionType)primitiveType)
            {
                case DefinitionType.AtomicBlock:
                    {
                        FieldList firstStateData = (FieldList)source[DefAgentFieldID.DataPerComponentState];

                        if (firstStateData != null)
                        {
                            secondaryCharacteristic = firstStateData[DefAgentFieldID.LayoutType].AsByte() ?? 0;

                            switch ((LayoutType)secondaryCharacteristic)
                            {
                                case LayoutType.Table:
                                    AtomicBlockDefinition ab = new AtomicBlockDefinition();
                                    ab.Unpack(source);

                                    res = ab;
                                    break;

                                case LayoutType.SingleSlot:
                                    SingleSlotBlockDefinition ssb = new SingleSlotBlockDefinition();
                                    ssb.Unpack(source);

                                    res = ssb;
                                    break;

                                case LayoutType.ScrollingText:
                                    ScrollingTextBlockDefinition stbd = new ScrollingTextBlockDefinition();
                                    stbd.Unpack(source);

                                    res = stbd;
                                    break;

                                default:
                                    DebugHelper.Out("Unsupported layout type for atomic: {0}", ((LayoutType)secondaryCharacteristic).ToString());
                                    break;
                            }
                        }

                        break;
                    }

                case DefinitionType.Container:
                    {
                        secondaryCharacteristic = source[DefAgentFieldID.LayoutType].AsByte() ?? 0;

                        switch ((LayoutType)secondaryCharacteristic)
                        {
                            case LayoutType.Box:
                                BoxLayoutBlockDefinition bx = new BoxLayoutBlockDefinition();
                                bx.Unpack(source);

                                res = bx;
                                break;

                            case LayoutType.Flow:
                                ListBlockDefinition fl = new ListBlockDefinition();
                                fl.Unpack(source);

                                res = fl;
                                break;

                            case LayoutType.Grid:
                                GridBlockDefinition gr = new GridBlockDefinition();
                                gr.Unpack(source);

                                res = gr;
                                break;

                            default:
                                DebugHelper.Out("Unsupported layout type for container: {0}", ((LayoutType)secondaryCharacteristic).ToString());
                                break;
                        }

                        break;
                    }

                case DefinitionType.Frame:
                    {
                        FrameDefinition fd = new FrameDefinition();
                        fd.Unpack(source);

                        res = fd;
                        break;
                    }

                case DefinitionType.Plugin:
                    {
                        secondaryCharacteristic = source[DefAgentFieldID.PluginType].AsByte() ?? 0;

                        switch ((PluginType)secondaryCharacteristic)
                        {
                            case PluginType.Map:
                                MapPluginBlockDefinition mpbd = new MapPluginBlockDefinition();
                                mpbd.Unpack(source);

                                res = mpbd;
                                break;

                            default:
                                DebugHelper.Out("Unsupported plug-in type: {0}", ((PluginType)secondaryCharacteristic).ToString());
                                break;
                        }
                        
                        break;
                    }

                case DefinitionType.Palette:
                    {
                        PaletteDefinition palette = new PaletteDefinition();
                        palette.Unpack(source);

                        res = palette;
                        break;
                    }

                case DefinitionType.Font:
                    {
                        FontDefinition font = new FontDefinition();
                        font.Unpack(source);

                        res = font;
                        break;
                    }

                case DefinitionType.ApplicationEvents:
                    {
                        ApplicationEventsDefinition appEvents = new ApplicationEventsDefinition();
                        appEvents.Unpack(source);

                        res = appEvents;
                        break;
                    }

                default:
                    DebugHelper.Out("Unsupported definition type: {0}", ((DefinitionType)primitiveType).ToString());
                    break;
            }

            if ((res != null) && res.IsUnpacked)
                return res;
            else
                return null;
        }

        public DefinitionBase Find(int applicationID, int definitionID, bool createIfNeeded = false)
        {
            byte[] entityID = EntityID.Create(CacheEntityType.Definition, applicationID, definitionID);
            object data = Core.Cache.Server.FindLiveEntity(null, entityID);

            if (data == null)
                data = Core.Cache.Server[entityID];

            if (data != null)
            {
                if (data is DefinitionBase)
                    return data as DefinitionBase;
                else if (createIfNeeded && (data is FieldList))
                    return UnpackDefinition((FieldList)data); //TODO: cache this?
            }

            return null;
        }

        public ActionSetCollection GetApplicationEvents(int applicationID)
        {
            ApplicationEventsDefinition def = Find(applicationID, 1) as ApplicationEventsDefinition;

            if (def != null)
                return def.Events;
            else
                return null;
        }
    }
}
