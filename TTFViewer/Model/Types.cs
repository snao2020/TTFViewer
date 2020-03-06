using System;
using System.IO;
using System.Text;

namespace TTFViewer.Model
{
#pragma warning disable IDE1006 // Naming rule violation

    public interface ILoadable
    {
        void Load(BinaryReader reader);
    }
          

    public struct int8 : ILoadable
    {
        SByte Value;

        public void Load(BinaryReader reader)
        {
            Value = reader.ReadSByte();
        }


        public static implicit operator SByte(int8 v)
        {
            return v.Value;
        }


        public override string ToString()
        {
            return Value.ToString();
        }
    }


    public struct uint8 : ILoadable
    {
        Byte Value;

        public void Load(BinaryReader reader)
        {
            Value = reader.ReadByte();
        }


        public static implicit operator Byte(uint8 v)
        {
            return v.Value;
        }


        public override string ToString()
        {
            return Value.ToString();
        }
    }


    public struct int16 : ILoadable
    {
        Int16 Value;

        public void Load(BinaryReader reader)
        {
            Value = BigEndian.ReadInt16(reader);
        }


        public static implicit operator Int16(int16 v)
        {
            return v.Value;
        }


        public override string ToString()
        {
            return Value.ToString();
        }
    }


    public struct uint16 : ILoadable
    {
        UInt16 Value;

        public void Load(BinaryReader reader)
        {
            Value = BigEndian.ReadUInt16(reader);
        }


        public static implicit operator UInt16(uint16 v)
        {
            return v.Value;
        }


        public override string ToString()
        {
            return Value.ToString();
        }
    }


    public struct uint24 : ILoadable
    {
        UInt32 Value;

        public void Load(BinaryReader reader)
        {
            Value = BigEndian.ReadUInt24(reader);
        }


        public static implicit operator UInt32(uint24 v)
        {
            return v.Value;
        }


        public override string ToString()
        {
            return Value.ToString();
        }

    }


    public struct int32 : ILoadable
    {
        Int32 Value;

        public void Load(BinaryReader reader)
        {
            Value = BigEndian.ReadInt32(reader);
        }


        public static implicit operator Int32(int32 v)
        {
            return v.Value;
        }


        public override string ToString()
        {
            return Value.ToString();
        }
    }


    public struct uint32 : ILoadable
    {
        UInt32 Value;

        public void Load(BinaryReader reader)
        {
            Value = BigEndian.ReadUInt32(reader);
        }


        public static implicit operator UInt32(uint32 v)
        {
            return v.Value;
        }


        public override string ToString()
        {
            return Value.ToString();
        }
    }


    public struct Offset16 : ILoadable
    {
        UInt16 Value;

        public void Load(BinaryReader reader)
        {
            Value = BigEndian.ReadUInt16(reader);
        }


        public static implicit operator UInt16(Offset16 v)
        {
            return v.Value;
        }


        public override string ToString()
        {
            return string.Format("0x{0:X4}", Value);
        }
    }


    public struct Offset32 : ILoadable
    {
        UInt32 Value;

        public void Load(BinaryReader reader)
        {
            Value = BigEndian.ReadUInt32(reader);
        }


        public static implicit operator UInt32(Offset32 v)
        {
            return v.Value;
        }


        public override string ToString()
        {
            return string.Format("0x{0:X8}", Value);
        }
    }


    public struct FWORD : ILoadable
    {
        Int16 Value;

        public void Load(BinaryReader reader)
        {
            Value = BigEndian.ReadInt16(reader);
        }
        

        public static implicit operator Int16(FWORD v)
        {
            return v.Value;
        }


        public override string ToString()
        {
            return Value.ToString();
        }
    }


    public struct UFWORD : ILoadable
    {
        UInt16 Value;

        public void Load(BinaryReader reader)
        {
            Value = BigEndian.ReadUInt16(reader);
        }


        public static implicit operator UInt16(UFWORD v)
        {
            return v.Value;
        }


        public override string ToString()
        {
            return Value.ToString();
        }
    }


    public struct Fixed : ILoadable
    {
        UInt32 Value;

        public void Load(BinaryReader reader)
        {
            Value = BigEndian.ReadUInt32(reader);
        }


        public static implicit operator double(Fixed v)
        {
            return (double)v.Value / (1 << 16);
        }


        public override string ToString()
        {
            double x = (double)Value / (1 << 16);
            return x.ToString("F5"); // 1 / (1 << 16) == 0.000015
        }
    }


    public struct F2DOT14 : ILoadable
    {
        UInt16 Value;

        public void Load(BinaryReader reader)
        {
            Value = BigEndian.ReadUInt16(reader);
        }


        public override string ToString()
        {
            double x = (double)Value / (1 << 14);
            return x.ToString("F5"); // 1 / (1 << 14) == 0.000061
        }
    }


    public struct LONGDATETIME : ILoadable
    {
        Int64 Value; // SecondsSince1904

        public void Load(BinaryReader reader)
        {
            Value = BigEndian.ReadInt64(reader);
        }

        public override string ToString()
        {
            DateTime start = new DateTime(1904, 1, 1, 0, 0, 0);
            return start.AddSeconds(Value).ToString();
        }
    }


    public struct Tag : ILoadable
    {
        public static readonly Tag Null = "";

        UInt32 Value;   // ex. 'ttcf'(0x74746366)


        public void Load(BinaryReader reader)
        {
            Value = BigEndian.ReadUInt32(reader);
        }


        public static implicit operator UInt32(Tag tag)
        {
            return tag.Value;
        }


        public static implicit operator Tag(string text)
        {
            Tag result;

            if (text.Length == 4)
            {
                Byte[] bytes = Encoding.ASCII.GetBytes(text);
                Array.Reverse(bytes);
                result.Value = BitConverter.ToUInt32(bytes, 0);
            }
            else
                result.Value = 0;

            return result;
        }


        public override string ToString()
        {
            Byte[] bytes = BitConverter.GetBytes(Value);
            Array.Reverse(bytes);
            string result;
            try
            {
                result = Encoding.ASCII.GetString(bytes);
            }
            catch (Exception)
            {
                result = string.Format("(0x{0:X8})", Value);
            }
            return result;
        }
    }


    public struct PascalString : ILoadable
    {
        Byte[] Array;

        public void Load(BinaryReader reader)
        {
            Byte b = reader.ReadByte();
            reader.BaseStream.Position--;
            Array = reader.ReadBytes(b + 1);
        }

        public static implicit operator string(PascalString ps)
        {
            return Encoding.ASCII.GetString(ps.Array, 1, ps.Array[0]);
        }


        public override string ToString()
        {
            return this;
        }
    }


    static class BigEndian
    {
        public static string ReadAscii(BinaryReader reader, int count)
        {
            Byte[] bytes = reader.ReadBytes(count);
            return Encoding.ASCII.GetString(bytes);
        }


        public static Int16 ReadInt16(BinaryReader reader)
        {
            Byte[] bytes = reader.ReadBytes(sizeof(Int16));
            Array.Reverse(bytes);
            return BitConverter.ToInt16(bytes, 0);
        }


        public static UInt16 ReadUInt16(BinaryReader reader)
        {
            Byte[] bytes = reader.ReadBytes(sizeof(UInt16));
            Array.Reverse(bytes);
            return BitConverter.ToUInt16(bytes, 0);
        }


        public static UInt32 ReadUInt24(BinaryReader reader)
        {
            Byte[] bytes = new Byte[4];
            bytes[0] = 0;
            reader.Read(bytes, 1, 3);
            Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }


        public static Int32 ReadInt32(BinaryReader reader)
        {
            Byte[] bytes = reader.ReadBytes(sizeof(Int32));
            Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }


        public static UInt32 ReadUInt32(BinaryReader reader)
        {
            Byte[] bytes = reader.ReadBytes(sizeof(UInt32));
            Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }


        public static Int64 ReadInt64(BinaryReader reader)
        {
            Byte[] bytes = reader.ReadBytes(sizeof(Int64));
            Array.Reverse(bytes);
            return BitConverter.ToInt64(bytes, 0);
        }


        public static UInt64 ReadUInt64(BinaryReader reader)
        {
            Byte[] bytes = reader.ReadBytes(sizeof(UInt64));
            Array.Reverse(bytes);
            return BitConverter.ToUInt64(bytes, 0);
        }
    }

#pragma warning restore IDE1006 // Naming rule violation
}
