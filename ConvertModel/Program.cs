using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertModel
{
    public static class BitConverter
    {
        public static byte[] GetBytes(uint value)
        {
            return new byte[4] {
                    (byte)(value & 0xFF),
                    (byte)((value >> 8) & 0xFF),
                    (byte)((value >> 16) & 0xFF),
                    (byte)((value >> 24) & 0xFF) };
        }

        public static byte[] GetBytes(ushort value)
        {
            byte[] arr = new byte[2] {
                (byte)(value & 0xFF),
                (byte)((value >> 8) & 0xFF) };

            Array.Reverse(arr);
            return arr;
        }

        public static unsafe byte[] GetBytes(float value)
        {
            uint val = *((uint*)&value);
            return GetBytes(val);
        }

        public static unsafe byte[] GetBytes(float value, ByteOrder order)
        {
            byte[] bytes = GetBytes(value);
            if (order != ByteOrder.LittleEndian)
            {
                System.Array.Reverse(bytes);
            }
            return bytes;
        }

        public static uint ToUInt32(byte[] value, int index)
        {
            return (uint)(
                value[0 + index] << 0 |
                value[1 + index] << 8 |
                value[2 + index] << 16 |
                value[3 + index] << 24);
        }

        public static unsafe float ToSingle(byte[] value, int index)
        {
            uint i = ToUInt32(value, index);
            return *(((float*)&i));
        }

        public static unsafe float ToSingle(byte[] value, int index, ByteOrder order)
        {
            if (order != ByteOrder.LittleEndian)
            {
                System.Array.Reverse(value, index, value.Length);
            }
            return ToSingle(value, index);
        }

        public enum ByteOrder
        {
            LittleEndian,
            BigEndian
        }

        static public bool IsLittleEndian
        {
            get
            {
                unsafe
                {
                    int i = 1;
                    char* p = (char*)&i;

                    return (p[0] == 1);
                }
            }
        }
    }

    class Program
    {
        static int readArrayLine(string line, List<float> arrayOut)
        {
            int layerSize = 0;
            string[] elements = line.Replace("[", "").Replace("]", "").Split(' ');
            foreach (string element in elements)
            {
                float fl;
                if (float.TryParse(element, out fl))
                {
                    arrayOut.Add(fl);
                    layerSize += 4;
                }
            }
            return layerSize;
        }

        static void convertConv(string layer, List<byte> outArr)
        {
            int layerSize = 0;
            int startIndex = outArr.Count;

            outArr.Add(0); outArr.Add(0); outArr.Add(0); outArr.Add(0);
            string[] lines = layer.Split('\n');
            //this data to byte arr
            string activation;
            ushort x = 0, y = 0, z = 0, k = 0;
            List<float> floatValues = new List<float>();
            foreach (string line in lines)
            {
                if(line.Contains('['))
                {
                    layerSize += readArrayLine(line, floatValues);
                }
                else if(line.Contains("activation"))
                {
                    activation = line.Substring(11);
                    layerSize += 32;
                }
                else if(line.Contains("bias"))
                {
                    layerSize += readArrayLine(line.Substring(5), floatValues);
                }
                else
                {
                    string[] dims = line.Split(',');
                    ushort.TryParse(dims[0], out x);
                    ushort.TryParse(dims[1], out y);
                    ushort.TryParse(dims[2], out z);
                    ushort.TryParse(dims[3], out k);
                }
            }

            byte[] s1 = BitConverter.GetBytes(x);
            byte[] s2 = BitConverter.GetBytes(y);
            byte[] s3 = BitConverter.GetBytes(z);
            byte[] s4 = BitConverter.GetBytes(k);
        }

        static void Main(string[] args)
        {
            string[] original;
            original = System.IO.File.ReadAllLines(@"C:\Users\chewycrashburn\Source\Repos\learn-guns\Release\model2.cnn");
            int buildID = -1;
            List<byte> outputArray = new List<byte>();
            StringBuilder strBdr = new StringBuilder();
            foreach(string line in original)
            {
                if(line.Contains("layer"))
                {
                    switch(buildID)
                    {
                        case 1:
                            {
                                convertConv(strBdr.ToString(), outputArray);
                            }
                            break;
                        case 2:
                            {
                                convertDense(strBdr.ToString(), outputArray);
                            }
                            break;
                        case 3:
                            {
                                convertMaxPooling(strBdr.ToString(), outputArray);
                            }
                            break;
                        case 4:
                            {
                                convertFlatten(strBdr.ToString(), outputArray);
                            }
                            break;
                    }
                    switch(line.Substring(6))
                    {
                        case "Conv2D":
                            buildID = 1;
                            break;
                        case "Dense":
                            buildID = 2;
                            break;
                        case "MaxPooling2D":
                            buildID = 3;
                            break;
                        case "Flatten":
                            buildID = 4;
                            break;
                        default:
                            buildID = -1;
                            break;
                    }
                }
                else
                {
                    strBdr.Append(line + "\n");
                }
            }
        }
    }
}
