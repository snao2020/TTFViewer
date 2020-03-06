using System;
using System.Collections.Generic;
using TTFViewer.Model;

namespace TTFViewer.ViewModel
{
#pragma warning disable IDE1006

    public static class TTFTable
    {
        public static ItemContentTable TTCHeader_Version1 = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "ttcTag", new ItemContent(ItemContentHelper.TagText, (ivm) => { return (Tag)ivm.Value == (Tag)"ttcf"; }, "Font Collection ID string: 'ttcf'") },
                { "majorVersion", new ItemContent(ItemContentHelper.FieldText, (ivm) => { return (uint16) ivm.Value == 1;}, "Major version of the TTC Header, = 1 or 2") },
                { "minorVersion", new ItemContent(ItemContentHelper.FieldText, (ivm) => { return (uint16) ivm.Value == 0;}, "Minor version of the TTC Header, = 0") },
                { "numFonts", new ItemContent(ItemContentHelper.FieldText, null) },
                { "offsetTable", new ItemContent(ItemContentHelper.FieldText, null, "TableDirectory") },
            },
            BaseTable = null
        };

        public static ItemContentTable TTCHeader_Version2 = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "dsigTag", new ItemContent(ItemContentHelper.TagText, IsDSIGorNull, "0x44534947 ('DSIG') (null if no signature) ") },
                { "dsigLength", new ItemContent(ItemContentHelper.Hex32Text, null) },
                { "dsigOffset", new ItemContent(ItemContentHelper.FieldText, null, "DSIG") },
            },
            BaseTable = TTCHeader_Version1
        };

        public static ItemContentTable OffsetTable = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "sfntVersion", new ItemContent(ItemContentHelper.Hex32Text, sfntIsValid, "0x00010000 or 0x4F54544F ('OTTO')") },
                { "numTables", new ItemContent(ItemContentHelper.FieldText, null, "Number of tables") },
                { "searchRange", new ItemContent(ItemContentHelper.Hex16Text, null, "(Maximum power of 2 <= numTables) x 16") },
                { "entrySelector", new ItemContent(ItemContentHelper.Hex16Text, null, "Log2(maximum power of 2 <= numTables)") },
                { "rangeShift", new ItemContent(ItemContentHelper.Hex16Text, null, "NumTables x 16-searchRange") },
                { "entries", new ItemContent(ItemContentHelper.NodeText, null, TableEntryDescription) },
            },
        };

        public static ItemContentTable TableRecord = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "tableTag", new ItemContent(ItemContentHelper.TagText, null, "Table identifier") },
                { "checkSum", new ItemContent(ItemContentHelper.Hex32Text, null, "CheckSum for this table") },
                { "offset", new ItemContent(ItemContentHelper.Hex32Text, null) },
                { "length", new ItemContent(ItemContentHelper.Hex32Text, null, "Length of this table") },
            },
        };

        static bool IsDSIGorNull(FieldViewModel ivm)
        {
            if (ivm.Value is Tag tag)
                return tag == (Tag)"DSIG" || tag == Tag.Null;
            else
                return false;
        }

        static bool sfntIsValid(FieldViewModel ivm)
        {
            UInt32 value = (UInt32)(uint32)ivm.Value;
            return value == 0x00010000 || value == 0x4F54544F;
        }

        static string TableEntryDescription(FieldViewModel ivm)
        {
            if (ivm.Value is TableRecord tr)
                return string.Format("{0}", tr.tableTag);
            else
                return null;
        }
    }


    public static class DSIGTable
    {
        public static ItemContentTable DSIG = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "version", new ItemContent(ItemContentHelper.Hex32Text, null, "0x00000001") },
                { "numSignatures", new ItemContent(ItemContentHelper.FieldText, null) },
                { "flags", new ItemContent(ItemContentHelper.Hex16Text, null, "permission flags Bit 0: cannot be resigned Bits 1-7: Reserved (Set to 0)") },
                { "signatureRecords", new ItemContent(ItemContentHelper.NodeText, null) },
            },
        };


        public static ItemContentTable SignatureRecord = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "format", new ItemContent(ItemContentHelper.FieldText, null, "Format of the signature") },
                { "length", new ItemContent(ItemContentHelper.Hex32Text, null, "Length of signature in bytes") },
                { "offset", new ItemContent(ItemContentHelper.FieldText, null, "SignatureBlock") },
            },
        };


        public static ItemContentTable SignatureBlock_Format1 = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "reserved1", new ItemContent(ItemContentHelper.FieldText, null, "Reserved for future use; set to zero") },
                { "reserved2", new ItemContent(ItemContentHelper.FieldText, null, "Reserved for future use; set to zero") },
                { "signatureLength", new ItemContent(ItemContentHelper.FieldText, null, "Length (in bytes) of the PKCS#7 packet in the signature field") },
                { "signature", new ItemContent(ItemContentHelper.FieldText, null, "PKCS#7 packet") },
            },
        };
    }


    public static class OS2Table
    {
        public static ItemContentTable OS2_Version0 = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "version", new ItemContent(ItemContentHelper.FieldText, null) },
                { "xAvgCharWidth", new ItemContent(ItemContentHelper.FieldText, null, RelativeValueDesription) },
                { "usWeightClass", new ItemContent(ItemContentHelper.FieldText, null) },
                { "usWidthClass", new ItemContent(ItemContentHelper.FieldText, null) },
                { "fsType", new ItemContent(ItemContentHelper.FieldText, null) },
                { "ySubspriptXSize", new ItemContent(ItemContentHelper.FieldText, null, RelativeValueDesription) },
                { "ySubspriptYSize", new ItemContent(ItemContentHelper.FieldText, null, RelativeValueDesription) },
                { "ySubscriptXOffset", new ItemContent(ItemContentHelper.FieldText, null, RelativeValueDesription) },
                { "ySubscriptYOffset", new ItemContent(ItemContentHelper.FieldText, null, RelativeValueDesription) },
                { "ySuperscriptXSize", new ItemContent(ItemContentHelper.FieldText, null, RelativeValueDesription) },
                { "ySuperscriptYSize", new ItemContent(ItemContentHelper.FieldText, null, RelativeValueDesription) },
                { "ySuperscriptXOffset", new ItemContent(ItemContentHelper.FieldText, null, RelativeValueDesription) },
                { "ySuperscriptYOffset", new ItemContent(ItemContentHelper.FieldText, null, RelativeValueDesription) },
                { "yStrikeoutSize", new ItemContent(ItemContentHelper.FieldText, null, RelativeValueDesription) },
                { "yStrikeoutPosition", new ItemContent(ItemContentHelper.FieldText, null, RelativeValueDesription) },
                { "xFamilyClass", new ItemContent(ItemContentHelper.FieldText, null) },
                { "panose", new ItemContent(ItemContentHelper.FieldText, null) },
                { "ulUnicodeRange1", new ItemContent(ItemContentHelper.FieldText, null) },
                { "ulUnicodeRange2", new ItemContent(ItemContentHelper.FieldText, null) },
                { "ulUnicodeRange3", new ItemContent(ItemContentHelper.FieldText, null) },
                { "ulUnicodeRange4", new ItemContent(ItemContentHelper.FieldText, null) },
                { "achVendID", new ItemContent(ItemContentHelper.FieldText, null) },
                { "sfSelection", new ItemContent(ItemContentHelper.FieldText, null) },
                { "usFirstCharIndex", new ItemContent(ItemContentHelper.FieldText, null) },
                { "usLastCharIndex", new ItemContent(ItemContentHelper.FieldText, null) },
                { "sTypoAscender", new ItemContent(ItemContentHelper.FieldText, null) },
                { "sTypoDescender", new ItemContent(ItemContentHelper.FieldText, null) },
                { "sTypoLineGap", new ItemContent(ItemContentHelper.FieldText, null) },
                { "usWinAscent", new ItemContent(ItemContentHelper.FieldText, null) },
                { "usWinDescent", new ItemContent(ItemContentHelper.FieldText, null) },
            },
        };

        public static ItemContentTable OS2_Version1 = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "ulCodePageRange1", new ItemContent(ItemContentHelper.FieldText, null) },
                { "ulCodePageRange2", new ItemContent(ItemContentHelper.FieldText, null) },
            },
            BaseTable = OS2_Version0,
        };

        public static ItemContentTable OS2_Version2 = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "sxHeight", new ItemContent(ItemContentHelper.FieldText, null) },
                { "sCapHeight", new ItemContent(ItemContentHelper.FieldText, null) },
                { "usDefaultChar", new ItemContent(ItemContentHelper.FieldText,null) },
                { "usBreakChar", new ItemContent(ItemContentHelper.FieldText, null) },
                { "usMaxContext", new ItemContent(ItemContentHelper.FieldText, null) },
            },
            BaseTable = OS2_Version1,
        };

        public static ItemContentTable OS2_Version3 = new ItemContentTable()
        {
            BaseTable = OS2_Version2,
        };

        public static ItemContentTable OS2_Version4 = new ItemContentTable()
        {
            BaseTable = OS2_Version2,
        };

        public static ItemContentTable OS2_Version5 = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "usLowerOpticalPointSize", new ItemContent(ItemContentHelper.FieldText, null) },
                { "usUpperOpticalPointSize", new ItemContent(ItemContentHelper.FieldText, null) },
            },
            BaseTable = OS2_Version4,
        };

        static string RelativeValueDesription(FieldViewModel ivm)
        {
            double rel = ivm.GetRelative();
            if (double.IsNaN(rel))
                return null;
            else
                return string.Format("relative={0}", rel);
        }

    }


    public static class cmapTable
    {
        public static ItemContentTable cmap = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "version", new ItemContent(ItemContentHelper.FieldText, null, "Table version number (0)") },
                { "numTables", new ItemContent(ItemContentHelper.FieldText, null) },
                { "encodingRecords", new ItemContent(ItemContentHelper.NodeText, null) },
            },
        };

        public static ItemContentTable EncodingRecord = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "platformID", new ItemContent(ItemContentHelper.FieldText, null, PlatformIDDescription) },
                { "encodingID", new ItemContent(ItemContentHelper.FieldText, null, EncodingIDDescription) },
                { "offset", new ItemContent(ItemContentHelper.FieldText, null, "Subtable") },
                //{ "offset", new ItemContent(ItemContentHelper.NodeText, null) },
            },
        };

        public static ItemContentTable cmapSubtable_Format0 = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "format", new ItemContent(ItemContentHelper.FieldText, null, "Format number is set to 0") },
                { "length", new ItemContent(ItemContentHelper.Hex16Text, null, "This is the length in bytes of the subtable") },
                { "language", new ItemContent(ItemContentHelper.Hex16Text, null, "For requirements on use of the language field") },
                { "glyphIdArray", new ItemContent(ItemContentHelper.FieldText, null, "An array that maps character codes to glyph index values") },
            },
        };

        public static ItemContentTable cmapSubtable_Format2 = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "format", new ItemContent(ItemContentHelper.FieldText, null, "Format number is set to 2") },
                { "length", new ItemContent(ItemContentHelper.Hex16Text, null, "This is the length in bytes of the subtable") },
                { "language", new ItemContent(ItemContentHelper.Hex16Text, null, "For requirements on use of the language field") },
                { "subHeaderKeys", new ItemContent(ItemContentHelper.Hex16Text, null, "Array that maps high bytes to subHeaders: value is subHeader index × 8") },
                { "subHeaders", new ItemContent(ItemContentHelper.FieldText, null, "Variable-length array of SubHeader records") },
                { "glyphIndexArray", new ItemContent(ItemContentHelper.FieldText, null, "Variable-length array containing subarrays used for mapping the low byte of 2-byte characters") },
            },
        };

        public static ItemContentTable SubHeader = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "firstCode", new ItemContent(ItemContentHelper.FieldText, null, "First valid low byte for this SubHeader") },
                { "entryCount", new ItemContent(ItemContentHelper.FieldText, null, "Number of valid low bytes for this SubHeader") },
                { "idDelta", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "idRangeOffset", new ItemContent(ItemContentHelper.FieldText, null, "") },
            },
        };

        public static ItemContentTable cmapSubtable_Format4 = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "format", new ItemContent(ItemContentHelper.FieldText, null, "Format number is set to 4") },
                { "length", new ItemContent(ItemContentHelper.Hex16Text, null, "This is the length in bytes of the subtable") },
                { "language", new ItemContent(ItemContentHelper.Hex16Text, null, "For requirements on use of the language field") },
                { "segCountX2", new ItemContent(ItemContentHelper.Hex16Text, null, "2 × segCount") },
                { "searchRange", new ItemContent(ItemContentHelper.Hex16Text, null, "2 × (2**floor(log2(segCount)))") },
                { "entrySelector", new ItemContent(ItemContentHelper.Hex16Text, null, "log2(searchRange/2)") },
                { "rangeShift", new ItemContent(ItemContentHelper.Hex16Text, null, "2 × segCount - searchRange") },
                { "endCode", new ItemContent(ItemContentHelper.Hex16Text, null, "End characterCode for each segment, last=0xFFFF") },
                { "reservedPad", new ItemContent(ItemContentHelper.Hex16Text, null, "Set to 0") },
                { "startCode", new ItemContent(ItemContentHelper.Hex16Text, null, "Start character code for each segment") },
                { "idDelta", new ItemContent(ItemContentHelper.FieldText, null, "Delta for all character codes in segment") },
                { "idRangeOffset", new ItemContent(ItemContentHelper.Hex16Text, null, "Offsets into glyphIdArray or 0") },
                { "glyphIdArray", new ItemContent(ItemContentHelper.Hex16Text, null, "Glyph index array (arbitrary length)") },
            },
        };

        public static ItemContentTable cmapSubtable_Format6 = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "format", new ItemContent(ItemContentHelper.FieldText, null, "Format number is set to 6") },
                { "length", new ItemContent(ItemContentHelper.Hex16Text, null, "This is the length in bytes of the subtable") },
                { "language", new ItemContent(ItemContentHelper.Hex16Text, null, "For requirements on use of the language field") },
                { "firstCode", new ItemContent(ItemContentHelper.Hex16Text, null, "First character code of subrange") },
                { "entryCount", new ItemContent(ItemContentHelper.FieldText, null, "Number of character codes in subrange") },
                { "glyphIdArray", new ItemContent(ItemContentHelper.FieldText, null, "Array of glyph index values for character codes in the range") },
            },
        };

        public static ItemContentTable cmapSubtable_Format8 = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "format", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "reserved", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "length", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "language", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "is32", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "numGroups", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "groups", new ItemContent(ItemContentHelper.FieldText, null, "") },
            },
        };

        public static ItemContentTable SequentialMapGroup = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "startCharCode", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "endCharCode", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "startGlyphID", new ItemContent(ItemContentHelper.FieldText, null, "") },
            },
        };

        public static ItemContentTable cmapSubtable_Format10 = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "format", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "reserved", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "length", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "language", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "startCharCode", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "numChars", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "glyphs", new ItemContent(ItemContentHelper.FieldText, null, "") },
            },
        };

        public static ItemContentTable cmapSubtable_Format12 = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "format", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "reserved", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "length", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "language", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "numGroups", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "groups", new ItemContent(ItemContentHelper.FieldText, null, "") },
            },
        };

        public static ItemContentTable cmapSubtable_Format13 = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "format", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "reserved", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "length", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "language", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "numGroups", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "groups", new ItemContent(ItemContentHelper.FieldText, null, "") },
            },
        };

        public static ItemContentTable ConstantMapGroup = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "startCharCode", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "endCharCode", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "glyphID", new ItemContent(ItemContentHelper.FieldText, null, "") },
            },
        };

        public static ItemContentTable cmapSubtable_Format14 = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "format", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "length", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "numVarSelectorRecords", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "varSelector", new ItemContent(ItemContentHelper.FieldText, null, "") },
            },
        };

        public static ItemContentTable VariationSelector = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "varSelector", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "defaultUVSOffset", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "nonDefaultUVSOffset", new ItemContent(ItemContentHelper.FieldText, null, "") },
            },
        };

        public static ItemContentTable DefaultUVS = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "numUnicodeValueRanges", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "ranges", new ItemContent(ItemContentHelper.FieldText, null, "") },
            },
        };

        public static ItemContentTable UnicodeRange = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "startUnicodeValue", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "additionalCount", new ItemContent(ItemContentHelper.FieldText, null, "") },
            },
        };

        public static ItemContentTable NonDefaultUVS = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "numUVSMappings", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "uvsMappings", new ItemContent(ItemContentHelper.FieldText, null, "") },
            },
        };

        public static ItemContentTable UVSMapping = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "unicodeValue", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "glyphID", new ItemContent(ItemContentHelper.FieldText, null, "") },
            },
        };


        static string PlatformIDDescription(FieldViewModel ivm)
        {
            UInt16 id = (UInt16)(uint16)ivm.Value;
            return ItemContentHelper.PlatformIDText(id);
        }


        static string EncodingIDDescription(FieldViewModel ivm)
        {
            if (ivm.Parent is FieldViewModel parent)
            {
                EncodingRecord er = (EncodingRecord)parent.Value;
                UInt16 platformID = er.platformID;
                UInt16 encodingID = er.encodingID;
                return ItemContentHelper.EncodingIDText(platformID, encodingID);
            }
            return null;
        }
    }


    public static class FontTableTable
    {
        public static ItemContentTable head = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "majorVersion", new ItemContent(ItemContentHelper.FieldText, null, "Major version number of the font header table — set to 1") },
                { "minorVersion", new ItemContent(ItemContentHelper.FieldText, null, "Minor version number of the font header table — set to 0") },
                { "fontRevision", new ItemContent(ItemContentHelper.FieldText, null, "Set by font manufacturer") },
                { "checkSumAdjustment", new ItemContent(ItemContentHelper.Hex32Text, null, "To compute: set it to 0, sum the entire font as uint32, then store 0xB1B0AFBA - sum") },
                { "magicNumber", new ItemContent(ItemContentHelper.Hex32Text, null, "Set to 0x5F0F3CF5") },
                { "flags", new ItemContent(ItemContentHelper.Hex16Text, null, "") },
                { "unitsPerEm", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "created", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "modified", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "xMin", new ItemContent(ItemContentHelper.FieldText, null, RelativeInt16Description) },
                { "yMin", new ItemContent(ItemContentHelper.FieldText, null, RelativeInt16Description) },
                { "xMax", new ItemContent(ItemContentHelper.FieldText, null, RelativeInt16Description) },
                { "yMax", new ItemContent(ItemContentHelper.FieldText, null, RelativeInt16Description) },
                { "macStyle", new ItemContent(ItemContentHelper.Hex16Text, null, macStyleDescription) },
                { "lowestRecPPEM", new ItemContent(ItemContentHelper.FieldText, null, "Smallest readable size in pixels") },
                { "fontDirectionHint", new ItemContent(ItemContentHelper.FieldText, null, fontDirectionHintDescription) },
                { "indexToLocFormat", new ItemContent(ItemContentHelper.FieldText, null, "0 for short offsets (Offset16), 1 for long (Offset32)") },
                { "glyphDataFormat", new ItemContent(ItemContentHelper.FieldText, null, "0 for current format") },
            },
        };

        static string RelativeValueDesription(FieldViewModel ivm)
        {
            double rel = ivm.GetRelative();
            if (double.IsNaN(rel))
                return null;
            else
                return string.Format("relative={0}", rel);
        }

        static string RelativeInt16Description(FieldViewModel ivm)
        {
            //double em = (double)(UInt16)(uint16)((head)ivm.Parent.Value).unitsPerEm;
            //return string.Format("For all glyph bounding boxes (relative={0})", (double)(Int16)(int16)ivm.Value / em);

            double rel = ivm.GetRelative();
            if (double.IsNaN(rel))
                return string.Format("For all glyph bounding boxes");
            else
                //return string.Format("relative={0}", rel);
                return string.Format("For all glyph bounding boxes (relative={0})", rel);
        }

        static string macStyleDescription(FieldViewModel ivm)
        {
            UInt16 style = (UInt16)(uint16)ivm.Value;
            string result = style != 0 ? " " : "";
            if ((style & 1 << 0) != 0)
                result += "Bold ";
            if ((style & 1 << 1) != 0)
                result += "Italic";
            if ((style & 1 << 2) != 0)
                result += "Underline";
            if ((style & 1 << 3) != 0)
                result += "Outline";
            if ((style & 1 << 4) != 0)
                result += "Shadow";
            if ((style & 1 << 5) != 0)
                result += "Condensed";
            if ((style & 1 << 6) != 0)
                result += "Extended";
            return result;
        }

        static string fontDirectionHintDescription(FieldViewModel ivm)
        {
            string result = "Deprecated (Set to 2)";
            switch ((Int16)(int16)ivm.Value)
            {
                case 0: result += " Fully mixed directional glyphs"; break;
                case 1: result += " Only strongly left to right"; break;
                case 2: result += " Like 1 but also contains neutrals"; break;
                case -1: result += " Only strongly right to left"; break;
                case -2: result += " Like - 1 but also contains neutrals"; break;
            }
            return result;
        }

        public static ItemContentTable hhea = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "majorVersion", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "minorVersion", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "ascender", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "descender", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "lineGap", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "advanceWidthMax", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "minLeftSideBearing", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "minRightSideBearing", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "xMaxExtent", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "caretSlopeRise", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "caretSlopeRun", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "caretOffset", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "(reserved)", new ItemContent(ItemContentHelper.FieldText, null, "set to 0") },

                { "metricDataFormat", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "numberOfHMetrics", new ItemContent(ItemContentHelper.FieldText, null, "") },
            },
        };

        public static ItemContentTable hmtx = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "hMetrics", new ItemContent(ItemContentHelper.NodeText, null) },
                { "leftSideBearing", new ItemContent(ItemContentHelper.FieldText, null) },
            },
        };

        public static ItemContentTable longHorMetric = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "advanceWidth", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "lsb", new ItemContent(ItemContentHelper.FieldText, null, "") },
            },
        };

        public static ItemContentTable maxp_Version05 = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "version", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "numGlyphs", new ItemContent(ItemContentHelper.FieldText, null, "") },
            },
        };

        public static ItemContentTable maxp_Version10 = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "maxPoints", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "maxContours", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "maxCompositePoints", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "maxCompositeContours", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "maxZones", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "maxTwilightPoints", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "maxStorage", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "maxFunctionDefs", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "msxInstrusionDefs", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "maxStackElements", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "msxSizeOfInstructions", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "maxComponentElements", new ItemContent(ItemContentHelper.FieldText, null, "") },
                { "maxComponentDepth", new ItemContent(ItemContentHelper.FieldText, null, "") },
            },
            BaseTable = maxp_Version05,
        };
    }


    public static class nameTable
    {
        public static ItemContentTable name_format0 = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "format", new ItemContent(ItemContentHelper.FieldText, null, "Format selector (0 or 1)") },
                { "count", new ItemContent(ItemContentHelper.FieldText, null, "Number of name records") },
                { "stringOffset", new ItemContent(ItemContentHelper.FieldText, null, "Offset to start of string storage (from start of table)") },
                { "nameRecords", new ItemContent(ItemContentHelper.NodeText, null) },
            },
        };

        public static ItemContentTable name_format1 = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "langTagCount", new ItemContent(ItemContentHelper.FieldText, null, "format=1 only (Number of language-tag records)") },
                { "langTagRecord", new ItemContent(ItemContentHelper.FieldText, null) },
            },
            BaseTable = name_format0,
        };

        static string PlatformIDDescription(FieldViewModel ivm)
        {
            UInt16 platformID = (UInt16)(uint16)ivm.Value;
            return ItemContentHelper.PlatformIDText(platformID);
        }

        static string EncodingIDDescription(FieldViewModel ivm)
        {
            if (ivm.Parent is FieldViewModel parent)
            {
                NameRecord nr = (NameRecord)parent.Value;
                return ItemContentHelper.EncodingIDText(nr.platformID, nr.encodingID);
            }
            return null;
        }

        static string NameIDDescription(FieldViewModel ivm)
        {
            switch ((UInt16)(uint16)ivm.Value)
            {
                case 0: return "Copyright notice";
                case 1: return "Font Family name";
                case 2: return "Font Subfamily name";
                case 3: return "Unique font identifier";
                case 4: return "Full font name";
                case 5: return "Version string";
                case 6: return "PostScript name for the font";
                case 7: return "Trademark";
                case 8: return "Manufacturer Name";
                case 9: return "Designer; name of the designer of the typeface";
                case 10: return "Description";
                case 11: return "URL Vendor; URL of font vendor(with protocol, e.g., http://, ftp://). If a unique serial number is embedded in the URL, it can be used to register the font";
                case 12: return "URL Designer; URL of typeface designer(with protocol";
                case 13: return "License Description; description of how the font may be legally used, or different example scenarios for licensed use. This field should be written in plain language, not legalese";
                case 14: return "License Info URL; URL where additional licensing information can be found";
                case 15: return "Reserved";
                case 16: return "Typographic Family name";
                case 17: return "Typographic Subfamily name";
                case 18: return "Compatible Full(Macintosh only)";
                case 19: return "Sample text";
                case 20: return "PostScript CID findfont name";
                case 21: return "WWS Family Name";
                case 22: return "WWS Subfamily Name";
                case 23: return "Light Background Palette";
                case 24: return "Dark Background Palette";
                case 25: return "Variations PostScript Name Prefix";
                default: return "<unknown>";
            }
        }

        public static ItemContentTable NameRecord = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "platformID", new ItemContent(ItemContentHelper.FieldText, null, PlatformIDDescription) },
                { "encodingID", new ItemContent(ItemContentHelper.FieldText, null, EncodingIDDescription) },
                { "languageID", new ItemContent(ItemContentHelper.Hex16Text, null, "Language ID") },
                { "nameID", new ItemContent(ItemContentHelper.FieldText, null, NameIDDescription) },
                { "length", new ItemContent(ItemContentHelper.FieldText, null, "String length (in bytes)") },
                { "offset", new ItemContent(ItemContentHelper.FieldText, null, "String offset from start of storage area (in bytes)") },
            },
        };

        public static ItemContentTable LangTagRecord = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "length", new ItemContent(ItemContentHelper.FieldText, null, "Language-tag string length (in bytes)") },
                { "offset", new ItemContent(ItemContentHelper.FieldText, null, "Language-tag string offset from start of storage area (in bytes)") },
            },
        };
    }


    public static class postTable
    {
        public static ItemContentTable post_Header = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "version", new ItemContent(ItemContentHelper.FieldText, null) },
                { "italicAngle", new ItemContent(ItemContentHelper.FieldText, null) },
                { "underlinePosition", new ItemContent(ItemContentHelper.FieldText, null) },
                { "underlineThichness", new ItemContent(ItemContentHelper.FieldText, null) },
                { "isFixedPitch", new ItemContent(ItemContentHelper.FieldText, null) },
                { "minMemType42", new ItemContent(ItemContentHelper.FieldText, null) },
                { "maxMemType42", new ItemContent(ItemContentHelper.FieldText, null) },
                { "minMemType1", new ItemContent(ItemContentHelper.FieldText, null) },
                { "maxMemType1", new ItemContent(ItemContentHelper.FieldText, null) },
            },
        };

        public static ItemContentTable post_Version10 = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
            },
            BaseTable = post_Header,
        };


        public static ItemContentTable post_Version20 = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "numGlyphs", new ItemContent(ItemContentHelper.FieldText, null) },
                { "glyphNameIndex", new ItemContent(ItemContentHelper.FieldText, null) },
                { "names", new ItemContent(ItemContentHelper.FieldText, null) },
            },
            BaseTable = post_Header,
        };


        public static ItemContentTable post_Version25 = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
                { "numGlyphs", new ItemContent(ItemContentHelper.FieldText, null) },
                { "offset", new ItemContent(ItemContentHelper.FieldText, null) },
            },
            BaseTable = post_Header,
        };


        public static ItemContentTable post_Version30 = new ItemContentTable()
        {
            Dictionary = new Dictionary<string, ItemContent>()
            {
            },
            BaseTable = post_Header,
        };
    }

#pragma warning restore IDE1006
}
