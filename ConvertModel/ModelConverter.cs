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

        public static byte[] GetBytes(int value)
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

    class ModelConverter
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
            //size 16 min because title 16 bytes
            int layerSize = 16;

            string[] lines = layer.Split('\n');
            //this data to byte arr
            string activation = "";
            int x = 0, y = 0, z = 0, k = 0;
            List<float> floatValues = new List<float>();
            foreach (string line in lines)
            {
                string lineTrim = line.Trim();
                if (string.IsNullOrWhiteSpace(lineTrim)) continue;
                
                if(lineTrim.Contains("activation"))
                {
                    activation = lineTrim.Substring(11);
                    layerSize += 32;
                }
                else if(lineTrim.Contains("bias"))
                {
                    layerSize += readArrayLine(lineTrim.Substring(5), floatValues);
                }
                else if (lineTrim.Contains('['))
                {
                    layerSize += readArrayLine(lineTrim, floatValues);
                }
                else
                {
                    string[] dims = lineTrim.Split(',');
                    int.TryParse(dims[0], out x);
                    int.TryParse(dims[1], out y);
                    int.TryParse(dims[2], out z);
                    int.TryParse(dims[3], out k);
                    layerSize += 16;
                }
            }
            byte[] s0 = { (byte)'C', (byte)'o', (byte)'n', (byte)'v', (byte)'2', (byte)'D', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            byte[] s1 = BitConverter.GetBytes(x);
            byte[] s2 = BitConverter.GetBytes(y);
            byte[] s3 = BitConverter.GetBytes(z);
            byte[] s4 = BitConverter.GetBytes(k);
            byte[] s5 = new byte[32];
            for(int a=0;a<32;a++)
            {
                if(a < activation.Length)
                {
                    s5[a] = (byte)activation[a];
                }
                else
                {
                    s5[a] = 0;
                }
            }
            byte[] s6 = new byte[floatValues.Count * 4];
            for(int a=0;a<floatValues.Count;a++)
            {
                byte[] b = BitConverter.GetBytes(floatValues[a], BitConverter.ByteOrder.LittleEndian);
                s6[a * 4] = b[0];
                s6[a * 4 + 1] = b[1];
                s6[a * 4 + 2] = b[2];
                s6[a * 4 + 3] = b[3];
            }

            outArr.Add((byte)layerSize);
            outArr.Add((byte)(layerSize >> 8));
            outArr.Add((byte)(layerSize >> 16));
            outArr.Add((byte)(layerSize >> 24));

            outArr.AddRange(s0);
            outArr.AddRange(s1);
            outArr.AddRange(s2);
            outArr.AddRange(s3);
            outArr.AddRange(s4);
            outArr.AddRange(s5);
            outArr.AddRange(s6);
        }

        static void convertDense(string layer, List<byte> outArr)
        {
            int layerSize = 16;

            string[] lines = layer.Split('\n');
            //this data to byte arr
            string activation = "";

            int x = 0, y = 0;
            List<float> floatValues = new List<float>();
            foreach (string line in lines)
            {
                string lineTrim = line.Trim();
                if (string.IsNullOrWhiteSpace(lineTrim)) continue;
                
                if (lineTrim.Contains("activation"))
                {
                    activation = lineTrim.Substring(11);
                    layerSize += 32;
                }
                else if (lineTrim.Contains("bias"))
                {
                    layerSize += readArrayLine(lineTrim.Substring(5), floatValues);
                }
                else if (lineTrim.Contains('['))
                {
                    layerSize += readArrayLine(lineTrim, floatValues);
                }
                else
                {
                    string[] dims = lineTrim.Split(',');
                    int.TryParse(dims[0], out x);
                    int.TryParse(dims[1], out y);
                    layerSize += 8;
                }
            }

            byte[] s0 = { (byte)'D', (byte)'e', (byte)'n', (byte)'s', (byte)'e', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            byte[] s1 = BitConverter.GetBytes(x);
            byte[] s2 = BitConverter.GetBytes(y);
            byte[] s3 = new byte[32];
            for (int a = 0; a < 32; a++)
            {
                if (a < activation.Length)
                {
                    s3[a] = (byte)activation[a];
                }
                else
                {
                    s3[a] = 0;
                }
            }
            byte[] s4 = new byte[floatValues.Count * 4];
            for (int a = 0; a < floatValues.Count; a++)
            {
                byte[] b = BitConverter.GetBytes(floatValues[a], BitConverter.ByteOrder.LittleEndian);
                s4[a * 4] = b[0];
                s4[a * 4 + 1] = b[1];
                s4[a * 4 + 2] = b[2];
                s4[a * 4 + 3] = b[3];
            }

            outArr.Add((byte)layerSize);
            outArr.Add((byte)(layerSize >> 8));
            outArr.Add((byte)(layerSize >> 16));
            outArr.Add((byte)(layerSize >> 24));

            outArr.AddRange(s0);
            outArr.AddRange(s1);
            outArr.AddRange(s2);
            outArr.AddRange(s3);
            outArr.AddRange(s4);
        }

        static void convertMaxPooling(string layer, List<byte> outArr)
        {
            string[] downsample = layer.Trim().Split(',');

            outArr.Add(24);
            outArr.Add(0);
            outArr.Add(0);
            outArr.Add(0);

            int ds1, ds2;
            int.TryParse(downsample[0], out ds1);
            int.TryParse(downsample[1], out ds2);

            byte[] s0 = { (byte)'M', (byte)'a', (byte)'x', (byte)'P', (byte)'o', (byte)'o', (byte)'l', (byte)'i', (byte)'n', (byte)'g', (byte)'2', (byte)'D', 0, 0, 0, 0 };
            byte[] s1 = BitConverter.GetBytes(ds1);
            byte[] s2 = BitConverter.GetBytes(ds2);

            outArr.AddRange(s0);
            outArr.AddRange(s1);
            outArr.AddRange(s2);
        }

        static void convertFlatten(string layer, List<byte> outArr)
        {
            outArr.Add(16);
            outArr.Add(0);
            outArr.Add(0);
            outArr.Add(0);
            byte[] s0 = { (byte)'F', (byte)'l', (byte)'a', (byte)'t', (byte)'t', (byte)'e', (byte)'n', 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            outArr.AddRange(s0);
        }

        static void Main(string[] args)
        {
            string original;
            original = System.IO.File.ReadAllText(@"C:\Users\chewycrashburn\Source\Repos\learn-guns\Release\model2.cnn").Replace("\r", "");

            string[] originalLines = original.Split('\n');
            int buildID = -1;
            List<byte> outputArray = new List<byte>();
            StringBuilder strBdr = new StringBuilder();
            foreach(string line in originalLines)
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
                    strBdr.Clear();
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
            switch (buildID)
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

            byte[] finalArray = outputArray.ToArray();

            System.IO.File.WriteAllBytes("modelshort.cnn", finalArray);
        }
    }
}
