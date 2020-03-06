using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TTFViewer.Model
{
    public class TTFClassLoader
    {
        static Dictionary<Type, Func<object, UInt32>[]> Dictionary;

        static TTFClassLoader()
        {
            Dictionary = new Dictionary<Type, Func<object, uint>[]>()
            {
                { typeof(TTCHeader_Version1), new Func<object, UInt32>[] { o => GetValue(o, "numFonts") } },
                { typeof(TTCHeader_Version2), new Func<object, UInt32>[] { o => GetValue(o, "numFonts") } },
                { typeof(DSIGHeader), new Func<object, UInt32>[] { o => GetValue(o, "numSignatures") } },
                { typeof(SignatureBlock_Format1), new Func<object, UInt32>[] { o => GetValue(o, "signatureLength") } },
                { typeof(cmapHeader), new Func<object, UInt32>[] { o => GetValue(o, "numTables") } },
                { typeof(cmapSubtable_Format0), new Func<object, UInt32>[] { o => 256 } },
                { typeof(cmapSubtable_Format2), new Func<object, UInt32>[] { o => 256, o => 0, o => 0 } },
                { typeof(cmapSubtable_Format4), new Func<object, UInt32>[] { o => GetValue(o, "segCountX2") / 2, o => GetValue(o, "segCountX2") / 2, o => GetValue(o, "segCountX2") / 2, o => GetValue(o, "segCountX2") / 2, o => 0 } },
                { typeof(cmapSubtable_Format6), new Func<object, UInt32>[] { o => GetValue(o, "entryCount") } },
                { typeof(cmapSubtable_Format8), new Func<object, UInt32>[] { o => 8192, o => GetValue(o, "numGroups") } },
                { typeof(cmapSubtable_Format10), new Func<object, UInt32>[] { o => 0 } },
                { typeof(cmapSubtable_Format12), new Func<object, UInt32>[] { o => GetValue(o, "numGroups") } },
                { typeof(cmapSubtable_Format13), new Func<object, UInt32>[] { o => GetValue(o, "numGroups") } },
                { typeof(cmapSubtable_Format14), new Func<object, UInt32>[] { o => GetValue(o, "numVarSelectorRecords") } },
                { typeof(DefaultUVS), new Func<object, UInt32>[] { o => GetValue(o, "numUnicodeValueRanges") } },
                { typeof(NonDefaultUVS), new Func<object, UInt32>[] { o => GetValue(o, "numUVSMappings") } },
                //{ typeof(hmtx), },
                { typeof(name_format0), new Func<object, UInt32>[] { o => GetValue(o, "count") } },
                { typeof(name_format1), new Func<object, UInt32>[] { o => GetValue(o, "count"), o => GetValue(o, "langTagCount") } },
                { typeof(OS2_Version0), new Func<object, UInt32>[] { o => 10 } },
                { typeof(OS2_Version1), new Func<object, UInt32>[] { o => 10 } },
                { typeof(OS2_Version2), new Func<object, UInt32>[] { o => 10 } },
                { typeof(OS2_Version3), new Func<object, UInt32>[] { o => 10 } },
                { typeof(OS2_Version4), new Func<object, UInt32>[] { o => 10 } },
                { typeof(OS2_Version5), new Func<object, UInt32>[] { o => 10 } },
                { typeof(post_Version20), new Func<object, UInt32>[] { o => GetValue(o, "numGlyphs"), o => postVersion20_numberNewGlyphs(o) } },
                { typeof(post_Version25), new Func<object, UInt32>[] { o => GetValue(o, "numGlyphs") } },
            };
        }


        static UInt32 GetValue(object obj, string fieldName)
        {
            Type type = obj.GetType();
            FieldInfo fi = type.GetField(fieldName);
            object value = fi.GetValue(obj);
            if (value is uint32 u32)
                return u32;
            else if (value is int32 i32)
                return (UInt32)(Int32)i32;
            else if (value is uint16 u16)
                return u16;
            else if (value is int16 i16)
                return (UInt32)(Int16)i16;
            else if (value is uint8 u8)
                return u8;
            else if (value is int8 i8)
                return (UInt32)(SByte)i8;
            return 0;
        }


#pragma warning disable IDE1006
        static UInt32 postVersion20_numberNewGlyphs(object obj)
        {
            UInt32 result = 0;
            if (obj is post_Version20 post20)
            {
                for (int i = 0; i < post20.numGlyphs; i++)
                {
                    if (post20.glyphNameIndex[i] >= 258)
                        result++;
                }
            }
            return result;
        }
#pragma warning restore IDE1006


        Type ObjectType;
        object[] Parameters;
        Func<object, UInt32>[] ArraySizeFuncs;

        public TTFClassLoader(Type objectType, object[] parameters)
        {
            ObjectType = objectType;
            Parameters = null;
            if (objectType == typeof(hmtx))
            {
                if (parameters == null)
                    throw new ArgumentException("ClassLoader : parameters is null.");
                hhea hhea = (hhea)parameters.Where(o => o is hhea).FirstOrDefault();
                if (hhea == null)
                    throw new ArgumentException("ClassLoader : hhea is not found.");
                maxp_Version05 maxp = (maxp_Version05)parameters.Where(o => o is maxp_Version05).FirstOrDefault();
                if (maxp == null)
                    throw new ArgumentException("ClassLoader : maxp is not found.");

                ArraySizeFuncs = new Func<object, UInt32>[] { o => hhea.numberOfHMetrics, o => (UInt32)maxp.numGlyphs - hhea.numberOfHMetrics };
            }
            else
                Dictionary.TryGetValue(objectType, out ArraySizeFuncs);
        }


        public object Create(BinaryReader reader)
        {
            object result = Activator.CreateInstance(ObjectType);
            if (result != null)
            {
                if (result is ILoadable loadable)
                    loadable.Load(reader);
                else
                {
                    IEnumerable<Func<object, UInt32>> p = ArraySizeFuncs;
                    IEnumerator<Func<object, UInt32>> enumerator = p?.GetEnumerator();
                    Load(result, reader, ObjectType, enumerator, Parameters);
                }
            }

            return result;
        }


        public Array CreateArray(BinaryReader reader, UInt32 count)
        {
            Array array = Array.CreateInstance(ObjectType, count);
            for (UInt32 i = 0; i < count; i++)
            {
                object value = Create(reader);
                array.SetValue(value, i);
            }
            return array;
        }


        static void Load(object obj, BinaryReader reader, Type type, IEnumerator<Func<object, UInt32>> enumArrayLength, params object[] parameters)
        {
            Type baseType = type.BaseType;
            if (baseType != null)
                Load(obj, reader, baseType, enumArrayLength, parameters);

            FieldInfo[] fis = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
            foreach (FieldInfo fi in fis)
            {
                Type fieldType = fi.FieldType;
                if (fieldType.IsArray)
                {
                    if(enumArrayLength == null || !enumArrayLength.MoveNext())
                        throw new ArgumentException("ClassLoader.Load : invalid enumArrayLength.");
                    UInt32 count = enumArrayLength.Current(obj);
                    Type elementType = fieldType.GetElementType();
                    if (elementType != null)
                    {
                        TTFClassLoader creator = new TTFClassLoader(elementType, parameters);
                        Array array = creator.CreateArray(reader, count);
                        fi.SetValue(obj, array);
                    }
                }
                else
                {
                    TTFClassLoader creator = new TTFClassLoader(fieldType, parameters);
                    object value = creator.Create(reader);
                    fi.SetValue(obj, value);
                }
            }
        }
    }
}
