using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TTFViewer.Model;

namespace TTFViewer.ViewModel
{
#pragma warning disable IDE1006

    public static class TTFContentManager
    {
        static Dictionary<Type, ItemContentTable> Dictionary =
        new Dictionary<Type, ItemContentTable>()
        {
            { typeof(TTCHeader_Version1), TTFTable.TTCHeader_Version1 },
            { typeof(TTCHeader_Version2), TTFTable.TTCHeader_Version2 },
            { typeof(OffsetTable), TTFTable.OffsetTable },
            { typeof(TableRecord), TTFTable.TableRecord },

            { typeof(DSIGHeader), DSIGTable.DSIG },
            { typeof(SignatureRecord), DSIGTable.SignatureRecord },
            { typeof(SignatureBlock_Format1), DSIGTable.SignatureBlock_Format1 },

            { typeof(OS2_Version0), OS2Table.OS2_Version0 },
            { typeof(OS2_Version1), OS2Table.OS2_Version1 },
            { typeof(OS2_Version2), OS2Table.OS2_Version2 },
            { typeof(OS2_Version3), OS2Table.OS2_Version3 },
            { typeof(OS2_Version4), OS2Table.OS2_Version4 },
            { typeof(OS2_Version5), OS2Table.OS2_Version5 },

            { typeof(cmapHeader), cmapTable.cmap },
            { typeof(EncodingRecord), cmapTable.EncodingRecord },
            { typeof(cmapSubtable_Format0), cmapTable.cmapSubtable_Format0 },
            { typeof(cmapSubtable_Format2), cmapTable.cmapSubtable_Format2 },
            { typeof(SubHeader), cmapTable.SubHeader },
            { typeof(cmapSubtable_Format4), cmapTable.cmapSubtable_Format4 },
            { typeof(cmapSubtable_Format6), cmapTable.cmapSubtable_Format6 },
            { typeof(cmapSubtable_Format8), cmapTable.cmapSubtable_Format8 },
            { typeof(SequentialMapGroup), cmapTable.SequentialMapGroup },
            { typeof(cmapSubtable_Format10), cmapTable.cmapSubtable_Format10 },
            { typeof(cmapSubtable_Format12), cmapTable.cmapSubtable_Format12 },
            { typeof(cmapSubtable_Format13), cmapTable.cmapSubtable_Format13 },
            { typeof(ConstantMapGroup), cmapTable.ConstantMapGroup },
            { typeof(cmapSubtable_Format14), cmapTable.cmapSubtable_Format14 },
            { typeof(VariationSelector), cmapTable.VariationSelector },
            { typeof(DefaultUVS), cmapTable.DefaultUVS },
            { typeof(UnicodeRange), cmapTable.UnicodeRange },
            { typeof(NonDefaultUVS), cmapTable.NonDefaultUVS },
            { typeof(UVSMapping), cmapTable.UVSMapping },

            { typeof(head), FontTableTable.head },
            { typeof(hhea), FontTableTable.hhea },
            { typeof(hmtx), FontTableTable.hmtx },
            { typeof(longHorMetric), FontTableTable.longHorMetric },
            { typeof(maxp_Version05), FontTableTable.maxp_Version05 },
            { typeof(maxp_Version10), FontTableTable.maxp_Version10 },

            { typeof(name_format0), nameTable.name_format0 },
            { typeof(name_format1), nameTable.name_format1 },
            { typeof(NameRecord), nameTable.NameRecord },
            { typeof(LangTagRecord), nameTable.LangTagRecord },

            { typeof(post_Version10), postTable.post_Version10 },
            { typeof(post_Version20), postTable.post_Version20 },
            { typeof(post_Version25), postTable.post_Version25 },
            { typeof(post_Version30), postTable.post_Version30 },
        };

        public static ItemContentTable Select(Type type)
        {
            if (type == null || !Dictionary.TryGetValue(type, out ItemContentTable result))
                result = ItemContentTable.DefaultTable;
            return result;
        }
    }


    public class ItemContentTable
    {
        public Dictionary<string, ItemContent> Dictionary;
        public ItemContentTable BaseTable;

        public static ItemContentTable DefaultTable = new ItemContentTable();

        public ItemContent Select(string fieldBaseName)
        {
            ItemContent result = null;
            if (fieldBaseName != null)
            {
                if (Dictionary == null || !Dictionary.TryGetValue(fieldBaseName, out result))
                {
                    if (BaseTable != null)
                        result = BaseTable.Select(fieldBaseName);
                }
            }

            return result;
        }
    }


    public class ItemContent
    {
        public Func<FieldViewModel, string> Text { get; }
        public Predicate<FieldViewModel> IsValid { get; }
        public Func<FieldViewModel, string> Description { get; }


        public ItemContent(Func<FieldViewModel, string> text, Predicate<FieldViewModel> isValid, Func<FieldViewModel, string> description)
        {
            Text = text;
            IsValid = isValid;
            Description = description;
        }


        public ItemContent(Func<FieldViewModel, string> text, Predicate<FieldViewModel> isValid, string description)
        {
            Text = text;
            IsValid = isValid;
            Description = (ivm)=> { return description; };
        }


        public ItemContent(Func<FieldViewModel, string> text, Predicate<FieldViewModel> isValid)
        {
            Text = text;
            IsValid = isValid;
            Description = null;
        }
    }


    static class ItemContentHelper
    {
        public static string FieldText(FieldViewModel ivm)
        {
            object value = ivm.Value;
            return string.Format("{0}", value != null ? value.ToString() : "<null>");
        }

        public static string Hex16Text(FieldViewModel ivm)
        {
            object value = ivm.Value;
            if (value is Offset16 o16)
                return string.Format("0x{0:X4}", (UInt16)o16);
            else if (value is uint16 u16)
                return string.Format("0x{0:X4}", (UInt16)u16);
            else
                return null;
        }

        public static string Hex32Text(FieldViewModel ivm)
        {
            object value = ivm.Value;
            if (value is Offset32 o32)
                return string.Format("0x{0:X8}", (UInt32)o32);
            else if (value is uint32 u32)
                return string.Format("0x{0:X8}", (UInt32)u32);
            else
                return null;
        }


        public static string TagText(FieldViewModel ivm)
        {
            object value = ivm.Value;
            return string.Format("'{0}' (0x{1:X8})", value.ToString(), (UInt32)(Tag)value);
        }


        public static string NodeText(FieldViewModel ivm)
        {
            return null;
        }

        public static string DefaultText(Type fieldType, string fieldName, Type valueType, object value)
        {
            if (fieldType == null)
            {
                if (value == null)
                {
                    if (valueType == null)
                        return string.Format("<no fields>");
                    else
                        return string.Format("0 {0}", valueType.Name);
                }
                else
                    return string.Format("1 {0} {1}", valueType.Name, value);
            }
            else
            {
                string typeName = fieldType.Name;

                if (value == null)
                    return string.Format("2 {0} {1}=<null>", typeName, fieldType);

                else if (value is IEnumerable && !(value is string))
                {
                    PropertyInfo countProp = value.GetType().GetProperty("Count");
                    if (countProp != null)
                    {
                        int count = (int)countProp.GetValue(value, null);
                        return string.Format("3 {0} {1} (Count={2})", typeName, fieldName, count);
                    }
                    else
                        return string.Format("4 {0} {1}", typeName, fieldName);
                }
                else
                {
                    if (valueType != fieldType)
                    {
                        return string.Format("5 {0} {1}=(Source Type={2}) {3}",
                                             typeName, fieldName, valueType.Name, value);
                    }
                    else
                        return string.Format("6 {0} {1}={2}", typeName, fieldName, value);
                }
            }
        }


        public static string PlatformIDText(UInt16 platformID)
        {
            switch (platformID)
            {
                case 0: return "Platform name=Unicode, specific encoding IDs=Various, Language IDs=None";
                case 1: return "Platform name=Macintosh, specific encoding IDs=Script manager code, Language IDs=Various";
                case 2: return "Platform name=ISO [deprecated], specific encoding IDs=ISO encoding [deprecated], Language IDs=None";
                case 3: return "Platform name=Windows, specific encoding IDs=Windows encoding, Language IDs=Various";
                case 4: return "Platform name=Custom, specific encoding IDs=Custom, Language IDs=None";
                default:
                    if (240 <= platformID && platformID <= 255)
                        return "(reserved for user-defined platforms)";
                    else
                        return "(unknown platform";
            }
        }


        public static string EncodingIDText(UInt16 platformID, UInt16 encodingID)
        {
            switch (platformID)
            {
                case 0: // Unicode platform
                    switch (encodingID)
                    {
                        case 0: return "Unicode 1.0 semantics";
                        case 1: return "Unicode 1.1 semantics";
                        case 2: return "ISO / IEC 10646 semantics";
                        case 3: return "Unicode 2.0 and onwards semantics, Unicode BMP only('cmap' subtable formats 0, 4, 6)";
                        case 4: return "Unicode 2.0 and onwards semantics, Unicode full repertoire('cmap' subtable formats 0, 4, 6, 10, 12)";
                        case 5: return "Unicode Variation Sequences('cmap' subtable format 14)";
                        case 6: return "Unicode full repertoire('cmap' subtable formats 0, 4, 6, 10, 12, 13)";
                        default: return "(unknown)";
                    }
                case 3: // Windows platform
                    switch (encodingID)
                    {
                        case 0: return "Symbol";
                        case 1: return "Unicode BMP";
                        case 2: return "ShiftJIS";
                        case 3: return "PRC";
                        case 4: return "Big5";
                        case 5: return "Wansung";
                        case 6: return "Johab";
                        case 7:
                        case 8:
                        case 9: return " Reserved";
                        case 10: return "Unicode full repertoire";
                        default: return "(unknown)";
                    }
                case 2: // ISO platform [Deprecated]
                    switch (encodingID)
                    {
                        case 0: return "7-bit ASCII";
                        case 1: return "ISO 10646";
                        case 2: return "ISO 8859-1";
                        default: return "(unknown)";
                    }
                case 4: //Custom platform
                    if (0 <= encodingID && encodingID <= 255)
                        return "OTF Windows NT compatibility mapping";
                    else
                        return "(unknown)";
                default: return "";
            }
        }
    }
    
#pragma warning restore IDE1006
}
