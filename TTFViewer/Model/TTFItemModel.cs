using System;
using System.Collections.Generic;
using System.IO;

namespace TTFViewer.Model
{
#pragma warning disable IDE1006

    public class nameBytes
    {
        public readonly UInt32 FilePosition;
        public readonly Byte[] Bytes;
        public readonly bool IsUnicode;

        public nameBytes(UInt32 filePosition, uint8[] array, bool isUnicode)
        {
            FilePosition = filePosition;
            int count = array.Length;
            Bytes = new Byte[count];
            for (int i = 0; i < count; i++)
                Bytes[i] = array[i];
            IsUnicode = isUnicode;
        }
    }


    public class TTFClassNotImplementedException : Exception
    {
        public TTFClassNotImplementedException(string message)
            :base(message)
        {
        }
    }


    public static class TTFItemModelHelper
    {
        public static object CreateFontTable(TTFItemModel im, Tag tag)
        {
            if(im is OffsetTableItemModel otim && im.Associations is TableRecord[] trs)
            {
                foreach (TableRecord tr in trs)
                {
                    if (tr.tableTag == tag)
                    {
                        TTFItemModel m = otim[tr.offset];
                        if (m != null)
                            return m.Value;
                        else
                            return null;
                    }
                }
            }
            return null;
        }
    }


    public class TTFItemModel
    {
        public TTFItemModel Parent { get; }
        public UInt32 FilePosition { get; }
        public virtual TTFItemModel this[UInt32 offset32] { get => null; }
        public virtual Type ValueType { get; }
        public virtual object Value { get => CreateObject(FilePosition, ValueType, null); }
        public virtual object[] Associations { get => null; }


        public TTFItemModel(TTFItemModel parent, UInt32 filePosition, Type valueType)
        {
            Parent = parent;
            FilePosition = filePosition;
            ValueType = valueType;
        }


        protected virtual object CreateObject(UInt32 filePosition, Type valueType, object[] parameters)
        {
            return Parent.CreateObject(filePosition, valueType, parameters);
        }

        protected virtual object CreateObjectArray(UInt32 filePosition, Type valueType, UInt32 count, object[] parameters)
        {
            return Parent.CreateObjectArray(filePosition, valueType, count, parameters);
        }
    }


    public class RootItemModel : TTFItemModel
    {
        public override TTFItemModel this[UInt32 offset32] { get => CreateChild(offset32); }
        public override object Value { get => null; }


        BinaryReader Reader;


        public RootItemModel(BinaryReader reader)
            : base(null, 0, null)
        {
            Reader = reader;
        }


        protected override object CreateObject(UInt32 filePosition, Type type, object[] parameters)
        {
            Reader.BaseStream.Position = filePosition;
            TTFClassLoader loader = new TTFClassLoader(type, parameters);
            return loader.Create(Reader);
        }


        protected override object CreateObjectArray(UInt32 filePosition, Type type, UInt32 count, params object[] parameters)
        {
            Reader.BaseStream.Position = filePosition;
            TTFClassLoader loader = new TTFClassLoader(type, parameters);
            return loader.CreateArray(Reader, count);
        }


        TTFItemModel CreateChild(UInt32 offset32)
        {
            if (offset32 == 0)
            {
                if (CreateObject(offset32, typeof(Tag), null) is Tag tag && tag == (Tag)"ttcf")
                    return new TTCHeaderItemModel(this, offset32);
                else
                    return new OffsetTableItemModel(this, offset32);
            }
            else
                throw new ArgumentException("bad offset number");
        }
    }


    public class TTCHeaderItemModel : TTFItemModel
    {
        public override TTFItemModel this[UInt32 offset32] { get => CreateChild(offset32); }
        public override Type ValueType { get; }


        public TTCHeaderItemModel(TTFItemModel parent, UInt32 filePosition)
            : base(parent, filePosition, null)
        {
            if (CreateObject(filePosition, typeof(TTCHeader_Version1), null) is TTCHeader_Version1 ttcHeader1)
            {
                UInt32 position = filePosition + CalcTTCHeader1Size(ttcHeader1);
                if (CreateObject(position, typeof(Tag), null) is Tag tag2 && tag2 == (Tag)"DSIG")
                    ValueType = typeof(TTCHeader_Version2);
                else
                    ValueType = typeof(TTCHeader_Version1);
            }
            else
                throw new NotSupportedException("TTCHeader is not found");
        }


        TTFItemModel CreateChild(UInt32 offset32)
        {
            object value = Value;
            if (value is TTCHeader_Version1 header1)
            {
                for (int i = 0; i < header1.numFonts; i++)
                {
                    if (offset32 == header1.offsetTable[i])
                        return new OffsetTableItemModel(this, offset32);
                }
            }
            if (value is TTCHeader_Version2 header2)
            {
                if (offset32 == header2.dsigOffset)
                    return new DSIGItemModel(this, offset32);
            }
            else
                throw new NotSupportedException("no TTCHeader");

            throw new ArgumentException("bad offset number");
        }


        static UInt32 CalcTTCHeader1Size(TTCHeader_Version1 ttcHeader1)
        {
            return 12 + ttcHeader1.numFonts * sizeof(UInt32);
        }
    }


    public class OffsetTableItemModel : TTFItemModel
    {
        const UInt32 SizeOfOffsetTable = 12;


        static Dictionary<Tag, Func<OffsetTableItemModel, UInt32, TTFItemModel>> Dictionary = new Dictionary<Tag, Func<OffsetTableItemModel, UInt32, TTFItemModel>>()
        {
            { "DSIG", (p,f)=>new DSIGItemModel(p, f) },
            { "OS/2", (p,f)=>new OS2ItemModel(p,f) },
            { "cmap", (p,f)=>new cmapItemModel(p,f) },
            { "head", (p,f)=>new TTFItemModel(p,f, typeof(head)) },
            { "hhea", (p,f)=>new TTFItemModel(p,f, typeof(hhea)) },
            { "hmtx", (p,f)=>new hmtxItemModel(p,f) },
            { "maxp", (p,f)=>new maxpItemModel(p,f) },
            { "name", (p,f)=>new nameItemModel(p,f) },
            { "post", (p,f)=>new postItemModel(p,f) },
        };


        public override TTFItemModel this[UInt32 offset32] { get => CreateChild(offset32); }
        public override object[] Associations { get => Entries; }


        TableRecord[] Entries;
        

        public OffsetTableItemModel(TTFItemModel parent, UInt32 filePosition)
            : base(parent, filePosition, typeof(OffsetTable))
        {
            if (Value is OffsetTable ot)
            {
                UInt32 position = FilePosition + SizeOfOffsetTable;
                UInt32 count = ot.numTables;
                Entries = CreateObjectArray(position, typeof(TableRecord), count, null) as TableRecord[];
            }
        }


        TTFItemModel CreateChild(UInt32 offset32)
        {
            TableRecord[] entries = Entries;
            if (entries != null)
            {
                foreach (TableRecord tr in entries)
                {
                    if (tr.offset == offset32)
                    {
                        if (Dictionary.TryGetValue(tr.tableTag, out Func<OffsetTableItemModel, UInt32, TTFItemModel> func))
                            return func(this, tr.offset);
                        throw new TTFClassNotImplementedException(string.Format("'{0}' is not implemented", tr.tableTag));
                    }
                }
                throw new ArgumentException("bad offset number");
            }
            throw new NotSupportedException("no TableRecord");
        }
    }


    public class DSIGItemModel : TTFItemModel
    {
        public override TTFItemModel this[UInt32 offset32] { get => CreateChild(offset32); }


        public DSIGItemModel(TTFItemModel parent, UInt32 filePosition)
            : base(parent, filePosition, typeof(DSIGHeader))
        {
        }


        TTFItemModel CreateChild(UInt32 offset32)
        {
            UInt32 filePosition = offset32 + FilePosition;
            if (Value is DSIGHeader dsig)
            {
                for (int i = 0; i < dsig.numSignatures; i++)
                {
                    if (dsig.signatureRecords[i].offset == offset32)
                    {
                        UInt32 version = dsig.signatureRecords[i].format;
                        if (version == 1)
                        {
                            Type childValueType = typeof(SignatureBlock_Format1);
                            return new TTFItemModel(this, filePosition, childValueType);
                        }
                        else
                            throw new TTFClassNotImplementedException(string.Format("Signature Block Format {0} is not implemented", version));
                    }
                }
                throw new ArgumentException("bad offset number");
            }
            throw new NotSupportedException("DSIG is not found");
        }                                                 
    }


    public class OS2ItemModel : TTFItemModel
    {
        public override Type ValueType { get; }


        public OS2ItemModel(TTFItemModel parent, UInt32 filePosition)
            : base(parent, filePosition, null)
        {
            UInt32 version = (UInt32)(uint16)CreateObject(filePosition, typeof(uint16), null);
            switch (version)
            {
                case 0: ValueType = typeof(OS2_Version0); break;
                case 1: ValueType = typeof(OS2_Version1); break;
                case 2: ValueType = typeof(OS2_Version2); break;
                case 3: ValueType = typeof(OS2_Version3); break;
                case 4: ValueType = typeof(OS2_Version4); break;
                case 5: ValueType = typeof(OS2_Version5); break;
                default:
                    throw new TTFClassNotImplementedException(string.Format("'OS/2' Version {0} is not implemented", version));
            }
        }
    }


    public class cmapItemModel : TTFItemModel
    {
        public override TTFItemModel this[UInt32 offset32] { get=> CreateChild(offset32);}


        public cmapItemModel(TTFItemModel parent, UInt32 filePosition)
            : base(parent, filePosition, typeof(cmapHeader))
        {
        }


        TTFItemModel CreateChild(UInt32 offset32)
        {
            if (Value is cmapHeader cmap)
            {
                for (int i = 0; i < cmap.numTables; i++)
                {
                    EncodingRecord encodingRecord = cmap.encodingRecords[i];
                    if (encodingRecord.offset == offset32)
                    {
                        UInt32 filePosition = FilePosition + offset32;
                        return new cmapSubtableItemModel(this, filePosition);
                    }
                }
                throw new ArgumentException("bad offset number");
            }
            throw new NotSupportedException("cmapHeader is not found");
        }
    }


    public class cmapSubtableItemModel : TTFItemModel
    {
        public override Type ValueType { get; }


        public cmapSubtableItemModel(TTFItemModel parent, UInt32 filePosition)
            : base(parent, filePosition, null)
        {
            UInt32 version = (uint16)CreateObject(filePosition, typeof(uint16), null);
            switch (version)
            {
                case 0: ValueType = typeof(cmapSubtable_Format0); break;
                case 2: ValueType = typeof(cmapSubtable_Format2); break;
                case 4: ValueType = typeof(cmapSubtable_Format4); break;
                case 6: ValueType = typeof(cmapSubtable_Format6); break;
                case 8: ValueType = typeof(cmapSubtable_Format8); break;
                case 10: ValueType = typeof(cmapSubtable_Format10); break;
                case 12: ValueType = typeof(cmapSubtable_Format12); break;
                case 13: ValueType = typeof(cmapSubtable_Format13); break;
                case 14: ValueType = typeof(cmapSubtable_Format14); break;
                default:
                    throw new TTFClassNotImplementedException(string.Format("cmap Subtable Format {0} is not implemented", version));
            }
        }
    }


    public class hmtxItemModel : TTFItemModel
    {
        public override object Value { get => GetValue(); }


        public hmtxItemModel(TTFItemModel parent, UInt32 filePosition)
            : base(parent, filePosition, typeof(hmtx))
        {
        }


        object GetValue()
        {
            object result = null;
            //if (Parent.Associations is TableRecord[] trs)
            {
                hhea hhea = TTFItemModelHelper.CreateFontTable(Parent, "hhea") as hhea;
                maxp_Version05 maxp = TTFItemModelHelper.CreateFontTable(Parent, "maxp") as maxp_Version05;
                if (hhea != null && maxp != null)
                    result = CreateObject(FilePosition, ValueType, new object[] { hhea, maxp });
            }
            return result;
        }
    }


    public class maxpItemModel : TTFItemModel
    {
        public override Type ValueType { get; }


        public maxpItemModel(TTFItemModel parent, UInt32 filePosition)
            : base(parent, filePosition, null)
        {
            UInt32 version = (UInt32)(uint32)CreateObject(filePosition, typeof(uint32), null);
            switch (version)
            {
                case 0x00005000: ValueType = typeof(maxp_Version05); break;
                case 0x00010000: ValueType = typeof(maxp_Version10); break;
                default:
                    throw new TTFClassNotImplementedException(string.Format("'maxp' Version {0} is not implemented", version));
            }
        }
    }


    public class nameItemModel : TTFItemModel
    {
        public override TTFItemModel this[UInt32 offset32] { get => CreateChild(offset32); }
        public override Type ValueType { get; }


        public nameItemModel(TTFItemModel parent, UInt32 filePosition)
            : base(parent, filePosition, null)
        {
            UInt32 version = (UInt32)(uint16)CreateObject(filePosition, typeof(uint16), null);
            switch (version)
            {
                case 0: ValueType = typeof(name_format0); break;
                case 1: ValueType = typeof(name_format1); break;
                default:
                    throw new TTFClassNotImplementedException(string.Format("'name' format {0} is not implemented", version));
            }
        }


        TTFItemModel CreateChild(UInt32 offset32)
        {
            object value = Value;

            if (value is name_format0 name0)
            {
                for (int i = 0; i < name0.count; i++)
                {
                    NameRecord nr = name0.nameRecords[i];
                    if (nr.offset == offset32)
                    {
                        UInt32 filePosition = FilePosition + name0.stringOffset + offset32;
                        return new nameBytesItemModel(this, filePosition);
                    }
                }
            }
            if (value is name_format1 name1)
            {
                for (int i = 0; i < name1.langTagCount; i++)
                {
                    LangTagRecord ltr = name1.langTagRecord[i];
                    if (ltr.offset == offset32)
                    {
                        UInt32 filePosition = FilePosition + offset32;
                        return new nameBytesItemModel(this, filePosition);
                    }
                }
            }
            return null;
        }
    }


    public class nameBytesItemModel : TTFItemModel
    {
        public override object Value { get => GetValue(); }


        public nameBytesItemModel(TTFItemModel parent, UInt32 filePosition)
            : base(parent, filePosition, typeof(nameBytes))
        {
        }


        object GetValue()
        {
            object parentValue = Parent.Value;
            if (parentValue is name_format0 name0)
            {
                for (int i = 0; i < name0.count; i++)
                {
                    NameRecord nr = name0.nameRecords[i];
                    UInt32 filePosition = Parent.FilePosition + name0.stringOffset + nr.offset;
                    if (filePosition == FilePosition)
                    {
                        uint8[] array = (uint8[])CreateObjectArray(filePosition, typeof(uint8), nr.length, null);
                        return new nameBytes(filePosition, array,
                                                nr.platformID == 3 && (nr.encodingID == 0 || nr.encodingID == 1 || nr.encodingID == 10));
                    }
                }
            }
            if (parentValue is name_format1 name1)
            {
                for (int i = 0; i < name1.langTagCount; i++)
                {
                    LangTagRecord ltr = name1.langTagRecord[i];
                    UInt32 filePosition = Parent.FilePosition + ltr.offset;
                    if (filePosition == FilePosition)
                    {
                        uint8[] array = (uint8[])CreateObjectArray(filePosition, typeof(uint8), ltr.length, null);
                        return new nameBytes(filePosition, array, true);
                    }
                }
            }
            return null;
        }
    }


    public class postItemModel : TTFItemModel
    {
        public override Type ValueType { get; }

        public postItemModel(TTFItemModel parent, UInt32 filePosition)
            : base(parent, filePosition, null)
        {
            UInt32 version = (UInt32)(uint32)CreateObject(filePosition, typeof(uint32), null);
            switch (version)
            {
                case 0x00010000: ValueType = typeof(post_Version10); break;
                case 0x00020000: ValueType = typeof(post_Version20); break;
                case 0x00025000: ValueType = typeof(post_Version25); break;
                case 0x00030000: ValueType = typeof(post_Version30); break;
                default:
                    throw new TTFClassNotImplementedException(string.Format("'post' Version {0} is not implemented", version));
            }
        }
    }

#pragma warning restore IDE1006
}
