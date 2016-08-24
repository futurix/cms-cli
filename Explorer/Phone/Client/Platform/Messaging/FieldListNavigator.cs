using System;
using Wave.Common;

namespace Wave.Platform.Messaging
{
    public class FieldListNavigator
    {
        private FieldList fieldList = null;
        private int index = -1;

        public FieldListNavigator(FieldList source)
        {
            if (source == null)
                throw new Exception("Cannot navigate null.");

            if (source.Count < 1)
                throw new Exception("Cannot navigate empty fieldlists.");

            fieldList = source;
        }

        public IFieldBase Current
        {
            get { return ValidateIndex(index) ? fieldList[index] : null; }
        }

        public short CurrentID
        {
            get { return ValidateIndex(index) ? fieldList[index].FieldID : (short)-1; }
        }

        public IFieldBase Next
        {
            get { return ValidateIndex(index + 1) ? fieldList[index + 1] : null; }
        }

        public IFieldBase NextNext
        {
            get { return ValidateIndex(index + 2) ? fieldList[index + 2] : null; }
        }

        public IFieldBase Previous
        {
            get { return ValidateIndex(index - 1) ? fieldList[index - 1] : null; }
        }

        public bool IsFirst
        {
            get { return (index == 0); }
        }

        public bool IsLast
        {
            get { return (index == (fieldList.Count - 1)); }
        }

        public bool MoveNext()
        {
            if (!IsLast)
            {
                index++;
                return true;
            }

            return false;
        }

        public bool MovePrevious()
        {
            if (!IsFirst)
            {
                index--;
                return true;
            }

            return false;
        }

        public bool FindFirst(Enum id)
        {
            return FindFirst(Convert.ToInt16(id));
        }

        public bool FindFirst(short id)
        {
            for (int i = 0; i < fieldList.Count; i++)
            {
                if (fieldList[i].FieldID == id)
                {
                    index = i;
                    return true;
                }
            }

            return false;
        }

        public bool FindNext(Enum id)
        {
            return FindNext(Convert.ToInt16(id));
        }

        public bool FindNext(short id)
        {
            if (!IsLast)
            {
                for (int i = index + 1; i < fieldList.Count; i++)
                {
                    if (fieldList[i].FieldID == id)
                    {
                        index = i;
                        return true;
                    }
                }
            }

            return false;
        }

        public bool FindPrevious(Enum id)
        {
            return FindPrevious(Convert.ToInt16(id));
        }

        public bool FindPrevious(short id)
        {
            if (!IsFirst)
            {
                for (int i = index - 1; i >= 0; i--)
                {
                    if (fieldList[i].FieldID == id)
                    {
                        index = i;
                        return true;
                    }
                }
            }

            return false;
        }

        public bool FindLast(Enum id)
        {
            return FindLast(Convert.ToInt16(id));
        }

        public bool FindLast(short id)
        {
            for (int i = fieldList.Count - 1; i >= 0; i--)
            {
                if (fieldList[i].FieldID == id)
                {
                    index = i;
                    return true;
                }
            }

            return false;
        }

        public override string ToString()
        {
            if (fieldList == null)
                return "Field-list navigator: no linked field-list??";

            if (index != -1)
                return String.Format("Field-list navigator: internal index -> {0}, fields -> {1}", index, fieldList.Count);
            else
                return "Field-list navigator: navigation not started yet";
        }

        private bool ValidateIndex(int index)
        {
            return ((index >= 0) && (index < fieldList.Count));
        }
    }
}
