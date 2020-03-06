using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace TTFViewer.Model
{
#pragma warning disable IDE1006

    [System.AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
    public class TTFOfficialName : Attribute
    {
        private string Name;


        public TTFOfficialName(string name)
        {
            Name = name;
        }


        public static string GetValue(MemberInfo mi)
        {
            Attribute attr = Attribute.GetCustomAttribute(mi, typeof(TTFOfficialName));
            if (attr is TTFOfficialName on)
                return on.Name;
            else
                return null;
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("TTC Header Version 1.0")]
    public class TTCHeader_Version1
    {
        public Tag ttcTag;              // original type is TAG (typo ?)
        public uint16 majorVersion;
        public uint16 minorVersion;
        public uint32 numFonts;
        public Offset32[] offsetTable;  // [numFonts] -->Array of offsets to the OffsetTable for each font from the beginning of the file
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("TTC Header Version 2.0")]
    public class TTCHeader_Version2 : TTCHeader_Version1
    {
        public Tag dsigTag;         // original type is uint32
        public uint32 dsigLength;
        public Offset32 dsigOffset; // original type is uint32
                                    // --> The offset (in bytes) of the DSIG table from the beginning of the TTC file 
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("Offset Table")]
    public class OffsetTable
    {
        public uint32 sfntVersion;
        public uint16 numTables;
        public uint16 searchRange;
        public uint16 entrySelector;
        public uint16 rangeShift;
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("Table Record")]
    public class TableRecord
    {
        public Tag tableTag;
        public uint32 checkSum;
        public Offset32 offset; //-->Offset from beginning of TrueType font file
        public uint32 length;
    }

    //----------------------------------------------

    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("DSIG - Digital Signature Table")]
    public class DSIGHeader
    {
        public uint32 version;
        public uint16 numSignatures;
        public uint16 flags;
        public SignatureRecord[] signatureRecords;  // [numSignatures]
    }


    [StructLayout(LayoutKind.Sequential)]
    public class SignatureRecord
    {
        public uint32 format;
        public uint32 length;
        public Offset32 offset; // Offset to the signature block from the beginning of the table
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("Signature Block Format 1")]
    public class SignatureBlock_Format1
    {
        public uint16 reserved1;
        public uint16 reserved2;
        public uint32 signatureLength;
        public uint8[] signature;   // [signatureLength]
    }


    //---------------------------------------------

    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("OS/2 — OS/2 and Windows Metrics Table, Version 0")]
    public class OS2_Version0
    {
        public uint16 version;
        public int16 xAvgCharWidth;         // Font design units
        public uint16 usWeightClass;        // Weight class
        public uint16 usWidthClass;         // Width class
        public uint16 fsType;               // Type flags
        public int16 ySubspriptXSize;       // Font design units
        public int16 ySubspriptYSize;       // Font design units
        public int16 ySubscriptXOffset;     // Font design units
        public int16 ySubscriptYOffset;     // Font design units
        public int16 ySuperscriptXSize;     // Font design units
        public int16 ySuperscriptYSize;     // Font design units
        public int16 ySuperscriptXOffset;   // Font design units
        public int16 ySuperscriptYOffset;   // Font design units
        public int16 yStrikeoutSize;        // Font design units
        public int16 yStrikeoutPosition;    // Font design units
        public int16 xFamilyClass;          // Font-family class and subclass
        public uint8[] panose;              // [10]
        public uint32 ulUnicodeRange1;      // (Bits 0–31)
        public uint32 ulUnicodeRange2;      // (Bits 32–63)
        public uint32 ulUnicodeRange3;      // (Bits 64–95)
        public uint32 ulUnicodeRange4;      // (Bits 96–127)
        public Tag achVendID;
        public uint16 sfSelection;
        public uint16 usFirstCharIndex;
        public uint16 usLastCharIndex;
        public int16 sTypoAscender;
        public int16 sTypoDescender;
        public int16 sTypoLineGap;
        public uint16 usWinAscent;
        public uint16 usWinDescent;
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("OS/2 — OS/2 and Windows Metrics Table, Version 1")]
    public class OS2_Version1 : OS2_Version0
    {
        public uint32 ulCodePageRange1;
        public uint32 ulCodePageRange2;
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("OS/2 — OS/2 and Windows Metrics Table, Version 2")]
    public class OS2_Version2 : OS2_Version1
    {
        public int16 sxHeight;
        public int16 sCapHeight;
        public uint16 usDefaultChar;
        public uint16 usBreakChar;
        public uint16 usMaxContext;
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("OS/2 — OS/2 and Windows Metrics Table, Version 3")]
    public class OS2_Version3 : OS2_Version2
    {
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("OS/2 — OS/2 and Windows Metrics Table, Version 4")]
    public class OS2_Version4 : OS2_Version2
    {
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("OS/2 — OS/2 and Windows Metrics Table, Version 5")]
    public class OS2_Version5 : OS2_Version4
    {
        public uint16 usLowerOpticalPointSize;  // TWIPs
        public uint16 usUpperOpticalPointSize;  // TWIPs
    }


    //-------------------------------------

    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("'cmap' - Character to Glyph Index Mapping Table")]
    public class cmapHeader
    {
        public uint16 version;
        public uint16 numTables;
        public EncodingRecord[] encodingRecords;    // [nemTables]
    }


    [StructLayout(LayoutKind.Sequential)]
    public class EncodingRecord
    {
        public uint16 platformID;
        public uint16 encodingID;
        public Offset32 offset; // Byte offset from beginning of table to the subtable for this encoding
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("'cmap' Subtable Format 0")]
    public class cmapSubtable_Format0
    {
        public uint16 format;
        public uint16 length;
        public uint16 language;
        public uint8[] glyphIdArray; // [256]
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("'cmap' Subtable Format 2")]
    public class cmapSubtable_Format2
    {
        public uint16 format;
        public uint16 length;
        public uint16 language;
        public uint16[] subHeaderKeys; //[256]
        public SubHeader[] subHeaders;
        public uint16[] glyphIndexArray;
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("SubHeader Record")]
    public class SubHeader
    {
        public uint16 firstCode;
        public uint16 entryCount;
        public int16 idDelta;
        public uint16 idRangeOffset;
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("'cmap' Subtable Format 4")]
    public class cmapSubtable_Format4
    {
        public uint16 format;
        public uint16 length;
        public uint16 language;
        public uint16 segCountX2;
        public uint16 searchRange;
        public uint16 entrySelector;
        public uint16 rangeShift;
        public uint16[] endCode;        // [segCount]
        public uint16 reservedPad;
        public uint16[] startCode;      // [segCount]
        public int16[] idDelta;         // [segCount]
        public uint16[] idRangeOffset;  // [segCount]
        public uint16[] glyphIdArray;
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("'cmap' Subtable Format 6")]
    public class cmapSubtable_Format6
    {
        public uint16 format;
        public uint16 length;
        public uint16 language;
        public uint16 firstCode;
        public uint16 entryCount;
        public uint16[] glyphIdArray; // [entryCount]
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("'cmap' Subtable Format 8")]
    public class cmapSubtable_Format8
    {
        public uint16 format;
        public uint16 reserved;
        public uint32 length;
        public uint32 language;
        public uint8[] is32;    // [8192]
        public uint32 numGroups;
        public SequentialMapGroup[] groups; // [numGroups]
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("SequentialMapGroup Record")]
    public class SequentialMapGroup
    {
        public uint32 startCharCode;
        public uint32 endCharCode;
        public uint32 startGlyphID;
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("'cmap' Subtable Format 10")]
    public class cmapSubtable_Format10
    {
        public uint16 format;
        public uint16 reserved;
        public uint32 length;
        public uint32 language;
        public uint32 startCharCode;
        public uint32 numChars;
        public uint16[] glyphs;
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("'cmap' Subtable Format 12")]
    public class cmapSubtable_Format12
    {
        public uint16 format;
        public uint16 reserved;
        public uint32 length;
        public uint32 language;
        public uint32 numGroups;
        public SequentialMapGroup[] groups; // [numGroups]
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("'cmap' Subtable Format 13")]
    public class cmapSubtable_Format13
    {
        public uint16 format;
        public uint16 reserved;
        public uint32 length;
        public uint32 language;
        public uint32 numGroups;
        public ConstantMapGroup[] groups; // [numGroups]
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("ConstantMapGroup Record")]
    public class ConstantMapGroup
    {
        public uint32 startCharCode;
        public uint32 endCharCode;
        public uint32 glyphID;
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("'cmap' Subtable Format 14")]
    public class cmapSubtable_Format14
    {
        public uint16 format;
        public uint32 length;
        public uint32 numVarSelectorRecords;
        public VariationSelector[] varSelector; // [numVarSelectorRecords]
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("VariationSelector Record")]
    public class VariationSelector
    {
        public uint24 varSelector;
        public Offset32 defaultUVSOffset;   // Offset from the start of the format 14 subtable to Default UVS Table. May be 0.
        public Offset32 nonDefaultUVSOffset;// Offset from the start of the format 14 subtable to Non-Default UVS Table. May be 0.
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("DefaultUVS Table")]
    public class DefaultUVS
    {
        public uint32 numUnicodeValueRanges;
        public UnicodeRange[] ranges; // [numUnicodeValueRanges]
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("UnicodeRange Record")]
    public class UnicodeRange
    {
        public uint24 startUnicodeValue;
        public uint8 additionalCount;
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("NonDefaultUVS Table")]
    public class NonDefaultUVS
    {
        public uint32 numUVSMappings;
        public UVSMapping[] uvsMappings; // [numUVSMappings]
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("UVSMapping Record")]
    public class UVSMapping
    {
        public uint24 unicodeValue;
        public uint16 glyphID;
    }


    //-----------------------------------------

    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("head - Font Header Table")]
    public class head
    {
        public uint16 majorVersion;
        public uint16 minorVersion;
        public Fixed fontRevision;
        public uint32 checkSumAdjustment;
        public uint32 magicNumber;       //0x5F0F3CF5
        public uint16 flags;
        public uint16 unitsPerEm;
        public LONGDATETIME created;
        public LONGDATETIME modified;
        public int16 xMin;
        public int16 yMin;
        public int16 xMax;
        public int16 yMax;
        public uint16 macStyle;
        public uint16 lowestRecPPEM;
        public int16 fontDirectionHint;
        public int16 indexToLocFormat;
        public int16 glyphDataFormat;
    }

    //-----------------------------------------

    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("hhea — Horizontal Header Table")]
    public class hhea
    {
        public uint16 majorVersion;
        public uint16 minorVersion;
        public FWORD ascender;
        public FWORD descender;
        public FWORD lineGap;
        public UFWORD advanceWidthMax;
        public FWORD minLeftSideBearing;
        public FWORD minRightSideBearing;
        public FWORD xMaxExtent;
        public int16 caretSlopeRise;// metrics variations (MVAR) table
        public int16 caretSlopeRun; // metrics variations (MVAR) table
        public int16 caretOffset;   // metrics variations (MVAR) table
        [TTFOfficialName("(reserved)")]
        public int16 reserved0;     // (reserved)
        [TTFOfficialName("(reserved)")]
        public int16 reserved1;     // (reserved)
        [TTFOfficialName("(reserved)")]
        public int16 reserved2;     // (reserved)
        [TTFOfficialName("(reserved)")]
        public int16 reserved3;     // (reserved)
        public int16 metricDataFormat;
        public uint16 numberOfHMetrics;
    }

    //-----------------------------------------------

    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("hmtx — Horizontal Metrics Table")]
    public class hmtx
    {
        public longHorMetric[] hMetrics;    // [hhea.numberOfHMetrics]
        public int16[] leftSideBearing;     // [maxp.numGlyphs - numberOfHMetrics]
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("longHorMetric Record")]
    public class longHorMetric
    {
        public uint16 advanceWidth;
        public int16 lsb;
    }


    //------------------------------------------

    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("maxp — Maximum Profile, Version 0.5")]
    public class maxp_Version05
    {
        public Fixed version;   // 0x00005000 or 0x00010000
        public uint16 numGlyphs;
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("maxp — Maximum Profile, Version 1.0")]
    public class maxp_Version10 : maxp_Version05
    {
        public uint16 maxPoints;
        public uint16 maxContours;
        public uint16 maxCompositePoints;
        public uint16 maxCompositeContours;
        public uint16 maxZones;
        public uint16 maxTwilightPoints;
        public uint16 maxStorage;
        public uint16 maxFunctionDefs;
        public uint16 msxInstrusionDefs;
        public uint16 maxStackElements;
        public uint16 msxSizeOfInstructions;
        public uint16 maxComponentElements;
        public uint16 maxComponentDepth;
    }


    //----------------------------------------------

    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("Naming table format 0")]
    public class name_format0
    {
        public uint16 format;
        public uint16 count;
        public Offset16 stringOffset;   // Offset to start of string storage (from start of table).
        public NameRecord[] nameRecords; // [count]
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("Naming table format 1")]
    public class name_format1 : name_format0
    {
        public uint16 langTagCount;
        public LangTagRecord[] langTagRecord;   // [langTagCount]
    }


    [StructLayout(LayoutKind.Sequential)]
    public class NameRecord
    {
        public uint16 platformID;
        public uint16 encodingID;
        public uint16 languageID;
        public uint16 nameID;
        public uint16 length;       // String length (in bytes)
        public Offset16 offset;     // String offset from start of storage area (in bytes)
    }


    [StructLayout(LayoutKind.Sequential)]
    public class LangTagRecord
    {
        public uint16 length;   // Language-tag string length (in bytes)
        public Offset16 offset; // Language-tag string offset from start of storage area (in bytes)
    }

    //----------------------------------------

    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("Header")]
    public class post_Header
    {
        public Fixed version;           // 0x00010000, 0x00020000,0x00025000, 0x00030000 
        public Fixed italicAngle;
        public FWORD underlinePosition;
        public FWORD underlineThichness;
        public uint32 isFixedPitch;
        public uint32 minMemType42;
        public uint32 maxMemType42;
        public uint32 minMemType1;
        public uint32 maxMemType1;
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("post — PostScript Table, Version 1.0")]
    public class post_Version10 : post_Header
    {
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("post — PostScript Table, Version 2.0")]
    public class post_Version20 : post_Header
    {
        public uint16 numGlyphs;
        public uint16[] glyphNameIndex; // [numGlyphs]
        public PascalString[] names;    // [numberNewGlyphs], original int8 names[numberNewGlyphs] 
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("post — PostScript Table, Version 2.5")]
    public class post_Version25 : post_Header
    {
        public uint16 numGlyphs;
        public uint8[] offset;          // [numGlyphs]
    }


    [StructLayout(LayoutKind.Sequential)]
    [TTFOfficialName("post — PostScript Table, Version 3.0")]
    public class post_Version30 : post_Header
    {
    }
    

#pragma warning restore IDE1006
}