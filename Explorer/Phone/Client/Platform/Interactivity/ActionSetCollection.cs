using System;
using System.Collections.Generic;
using Wave.Platform.Messaging;

namespace Wave.Platform
{
    public class ActionSetCollection
    {
        public int Count
        {
            get { return actions.Count; }
        }
        
        private List<ActionSet> actions = new List<ActionSet>();

        public ActionSetCollection()
        {
        }

        public ActionSet this[Anchor anchor]
        {
            get
            {
                for (int i = 0; i < actions.Count; i++)
                {
                    if (actions[i].Anchors.Contains(anchor))
                        return actions[i];
                }

                return null;
            }
        }

        public void Load(FieldList source, Enum fieldID)
        {
            // try to load new sets
            List<FieldList> results = source.GetItems<FieldList>(fieldID);

            if (results.Count > 0)
            {
                foreach (FieldList field in results)
                    Add(field);
            }
        }

        public void Add(ActionSet actionSet)
        {
            if (actionSet != null)
                actions.Add(actionSet);
        }

        public void Add(FieldList data)
        {
            if (data != null)
            {
                ActionSet temp = new ActionSet();
                temp.Unpack(data);

                Add(temp);
            }
        }

        public void Clear()
        {
            actions.Clear();
        }
    }
}
