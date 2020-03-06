using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TTFViewer.Model;

namespace TTFViewer.ViewModel
{
    static class ItemViewModelHelper
    {
        public static string FieldBaseName(string name)
        {
            string result = null;

            if (name != null)
            {
                int index = name.IndexOf('[');
                if (index >= 0)
                    result = name.Substring(0, index);
                else
                    result = name;
            }
            return result;
        }


        public static List<ItemViewModel> CreateModelChildren(ItemViewModel ivm, TTFItemModel model, UInt32 offset32)
        {
            var list = new List<ItemViewModel>();
            try
            {
                TTFItemModel m = model[offset32];
                list.Add(new TTFItemViewModel(m));

                object[] associations = m.Associations;
                if (associations != null)
                {
                    foreach (object item in associations)
                        list.Add(new AssociationViewModel(m, item));
                }
            }
            catch (TTFClassNotImplementedException e)
            {
                list.Add(new MessageViewModel(e.Message));
            }
            return list;
        }


        public static List<ItemViewModel> CreateOffset16Children(UInt16 o16, TTFItemModel model)
        {
            List<ItemViewModel> result = null;

            if (model.ValueType == typeof(name_format0) || model.ValueType == typeof(name_format1))
            {
                TTFItemModel m = model[o16];
                if (m != null && m.Value is nameBytes nameBytes)
                {
                    result = new List<ItemViewModel>();

                    string fp = string.Format("FilePosition=0x{0:X8}", nameBytes.FilePosition);
                    result.Add(new MessageViewModel(fp));

                    string str;
                    if (nameBytes.IsUnicode)
                        str = Encoding.BigEndianUnicode.GetString(nameBytes.Bytes);
                    else
                        str = Encoding.ASCII.GetString(nameBytes.Bytes);
                    result.Add(new MessageViewModel("String=" + str));

                    string byteArray = BitConverter.ToString(nameBytes.Bytes);
                    result.Add(new MessageViewModel("ByteArray=" + byteArray));
                }
            }
            return result;
        }


        public static List<ItemViewModel> CreateFieldChildren(ItemViewModel ivm, object obj)
        {
            List<ItemViewModel> result = null;
            if (obj != null)
            {
                result = new List<ItemViewModel>();
                Add(result, ivm, obj, obj.GetType());
            }
            return result;
        }


        static void Add(List<ItemViewModel> list, ItemViewModel ivm, object obj, Type type)
        {
            Type baseType = type.BaseType;
            if (baseType != null)
                Add(list, ivm, obj, baseType);


            FieldInfo[] fis = type.GetFields(
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (FieldInfo fi in fis)
            {
                Type fieldType = fi.FieldType;
                object fieldValue = fi.GetValue(obj);

                string fieldName = TTFOfficialName.GetValue(fi);
                if (string.IsNullOrEmpty(fieldName))
                    fieldName = fi.Name;

                if (fieldValue is Array array)
                {
                    Type elementType = fieldType.GetElementType(); 
                    for(int i = 0; i < array.Length; i++)
                    {
                        string name = string.Format("{0}[{1}]", fieldName, i);
                        list.Add(new FieldViewModel(elementType, name, array.GetValue(i), ivm));
                    }
                }
                else
                {
                    list.Add(new FieldViewModel(fieldType, fieldName, fieldValue, ivm));
                }
            }
        }
    }
}
