using System;
using System.Collections.Generic;
using Wave.Common;

namespace Wave.Platform.Messaging
{
    public static class FieldExtensions
    {
        public static int? AsNumber(this IFieldBase field)
        {
            if (field is Int16Field)
                return (field as Int16Field).Data;
            else if (field is Int32Field)
                return (field as Int32Field).Data;
            else if (field is ByteField)
                return (field as ByteField).Data;
            else
            {
                if (field != null)
                    DebugHelper.Out("Unexpected field: expected Int16Field / Int32Field / ByteField, got {0}", field.GetType().Name);
                
                return null;
            }
        }

        public static string AsText(this IFieldBase field)
        {
            if (field is StringField)
                return (field as StringField).Data;
            else if (field is BinaryFieldBase)
            {
                BinaryFieldBase bin = field as BinaryFieldBase;

                if (bin != null && bin.Data != null)
                    return StringHelper.GetString(bin.Data);
                else
                    return null;
            }
            else
            {
                if (field != null)
                    DebugHelper.Out("Unexpected field: expected StringField / BinaryFieldBase, got {0}", field.GetType().Name);
                
                return null;
            }
        }
        
        public static bool? AsBoolean(this IFieldBase field)
        {
            DebugHelper.CheckFieldType(field, typeof(BooleanField));
            
            return (field is BooleanField) ? (field as BooleanField).Data : (bool?)null;
        }

        public static byte? AsByte(this IFieldBase field)
        {
            DebugHelper.CheckFieldType(field, typeof(ByteField));
            
            return (field is ByteField) ? (field as ByteField).Data : (byte?)null;
        }

        public static byte[] AsByteArray(this IFieldBase field)
        {
            DebugHelper.CheckFieldType(field, typeof(BinaryFieldBase));
            
            return (field is BinaryFieldBase) ? (field as BinaryFieldBase).Data : null;
        }

        public static DateTime? AsDateTime(this IFieldBase field)
        {
            DebugHelper.CheckFieldType(field, typeof(DateTimeField));
            
            return (field is DateTimeField) ? (field as DateTimeField).Data : (DateTime?)null;
        }

        public static double? AsDouble(this IFieldBase field)
        {
            DebugHelper.CheckFieldType(field, typeof(DoubleField));

            return (field is DoubleField) ? (field as DoubleField).Data : (double?)null;
        }

        public static FieldList AsFieldList(this IFieldBase field)
        {
            DebugHelper.CheckFieldType(field, typeof(FieldList));

            return (field is FieldList) ? field as FieldList : null;
        }

        public static int? AsInteger(this IFieldBase field)
        {
            DebugHelper.CheckFieldType(field, typeof(Int32Field));

            return (field is Int32Field) ? (field as Int32Field).Data : (int?)null;
        }

        public static List<IFieldBase> AsList(this IFieldBase field)
        {
            DebugHelper.CheckFieldType(field, typeof(FieldList));

            return (field is FieldList) ? (field as FieldList).Data : null;
        }

        public static short? AsShort(this IFieldBase field)
        {
            DebugHelper.CheckFieldType(field, typeof(Int16Field));

            return (field is Int16Field) ? (field as Int16Field).Data : (short?)null;
        }

        public static string AsString(this IFieldBase field)
        {
            DebugHelper.CheckFieldType(field, typeof(StringField));

            return (field is StringField) ? (field as StringField).Data : null;
        }
    }
}
