using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using XDevkit;

namespace JRPCPlusPlus
{
    public static class JRPCPlusPlus
    {
        public static uint connectionId;

        private static byte[] nulled = new byte[100];    
        private static byte[] m_SMCMessage = new byte[16], m_SMCReturn = new byte[16];
        public static readonly uint jrpcVersion = 2;
        private static readonly uint Int = 1;
        private static uint String = 2;
        private static readonly uint Void = 0;
        private static readonly uint Float = 3;
        private static readonly uint Byte = 4;
        private static readonly uint IntArray = 5;
        private static readonly uint FloatArray = 6;
        private static readonly uint ByteArray = 7;
        private static readonly uint Uint64 = 8;
        private static readonly uint Uint64Array = 9;

        private static object CallArgs(IXboxConsole console, bool SystemThread, uint Type, Type t, string module, int ordinal, uint Address, uint ArraySize, params object[] Arguments)
        {
            if (!JRPCPlusPlus.IsValidReturnType(t))
                throw new Exception("Invalid type " + t.Name + Environment.NewLine + "jrpc only supports: bool, byte, short, int, long, ushort, uint, ulong, float, double");
            console.ConnectTimeout = console.ConversationTimeout = 4000000U;
            object[] objArray1 = new object[13];
            objArray1[0] = (object)"consolefeatures ver=";
            objArray1[1] = (object)JRPCPlusPlus.jrpcVersion;
            objArray1[2] = (object)" type=";
            objArray1[3] = (object)Type;
            objArray1[4] = SystemThread ? (object)" system" : (object)"";
            object[] objArray2 = objArray1;
            int index1 = 5;
            string str1;
            if (module == null)
                str1 = "";
            else
                str1 = " module=\"" + module + "\" ord=" + (object)ordinal;
            objArray2[index1] = (object)str1;
            objArray1[6] = (object)" as=";
            objArray1[7] = (object)ArraySize;
            objArray1[8] = (object)" params=\"A\\";
            objArray1[9] = (object)Address.ToString("X");
            objArray1[10] = (object)"\\A\\";
            objArray1[11] = (object)Arguments.Length;
            objArray1[12] = (object)"\\";
            string str2 = string.Concat(objArray1);
            if (Arguments.Length > 37)
                throw new Exception("Can not use more than 37 paramaters in a call");
            foreach (object o in Arguments)
            {
                bool flag = false;
                if (o is uint)
                {
                    str2 = str2 + JRPCPlusPlus.Int + "\\" + JRPCPlusPlus.UIntToInt((uint)o) + "\\";
                    flag = true;
                }
                if (o is int || o is bool || o is byte)
                {
                    if (o is bool)
                        str2 = str2 + JRPCPlusPlus.Int + "/" + Convert.ToInt32((bool)o) + "\\";
                    else
                        str2 = str2 + JRPCPlusPlus.Int + "\\" + (o is byte ? Convert.ToByte(o).ToString() : (object)Convert.ToInt32(o).ToString()) + "\\";
                    flag = true;
                }
                else if (o is int[] || o is uint[])
                {
                    byte[] numArray = JRPCPlusPlus.IntArrayToByte((int[])o);
                    string str3 = str2 + JRPCPlusPlus.ByteArray.ToString() + "/" + (object)numArray.Length + "\\";
                    for (int index2 = 0; index2 < numArray.Length; ++index2)
                        str3 += numArray[index2].ToString("X2");
                    str2 = str3 + "\\";
                    flag = true;
                }
                else if (o is string)
                {
                    string str3 = (string)o;
                    str2 = str2 + JRPCPlusPlus.ByteArray.ToString() + "/" + str3.Length + "\\" + ((string)o).ToHexString() + "\\";
                    flag = true;
                }
                else if (o is double)
                {
                    double num = (double)o;
                    str2 = str2 + JRPCPlusPlus.Float.ToString() + "\\" + num.ToString() + "\\";
                    flag = true;
                }
                else if (o is float)
                {
                    float num = (float)o;
                    str2 = str2 + JRPCPlusPlus.Float.ToString() + "\\" + num.ToString() + "\\";
                    flag = true;
                }
                else if (o is float[])
                {
                    float[] numArray = (float[])o;
                    string str3 = str2 + JRPCPlusPlus.ByteArray.ToString() + "/" + (numArray.Length * 4).ToString() + "\\";
                    for (int index2 = 0; index2 < numArray.Length; ++index2)
                    {
                        byte[] bytes = BitConverter.GetBytes(numArray[index2]);
                        Array.Reverse((Array)bytes);
                        for (int index3 = 0; index3 < 4; ++index3)
                            str3 += bytes[index3].ToString("X2");
                    }
                    str2 = str3 + "\\";
                    flag = true;
                }
                else if (o is byte[])
                {
                    byte[] numArray = (byte[])o;
                    string str3 = str2 + JRPCPlusPlus.ByteArray.ToString() + "/" + (object)numArray.Length + "\\";
                    for (int index2 = 0; index2 < numArray.Length; ++index2)
                        str3 += numArray[index2].ToString("X2");
                    str2 = str3 + "\\";
                    flag = true;
                }
                if (!flag)
                    str2 = str2 + JRPCPlusPlus.Uint64.ToString() + "\\" + JRPCPlusPlus.ConvertToUInt64(o).ToString() + "\\";
            }
            string Command = str2 + "\"";
            string String = JRPCPlusPlus.SendCommand(console, Command);
            uint num1;
            for (string _Ptr = "buf_addr="; String.Contains(_Ptr); String = JRPCPlusPlus.SendCommand(console, "consolefeatures " + _Ptr + "0x" + num1.ToString("X")))
            {
                Thread.Sleep(250);
                num1 = uint.Parse(String.Substring(String.find(_Ptr) + _Ptr.Length), NumberStyles.HexNumber);
            }
            console.ConversationTimeout = 2000U;
            console.ConnectTimeout = 5000U;
            switch (Type)
            {
                case 1:
                    uint num2 = uint.Parse(String.Substring(String.find(" ") + 1), NumberStyles.HexNumber);
                    if (t == typeof(uint))
                        return (object)num2;
                    if (t == typeof(int))
                        return (object)JRPCPlusPlus.UIntToInt(num2);
                    if (t == typeof(short))
                        return (object)short.Parse(String.Substring(String.find(" ") + 1), NumberStyles.HexNumber);
                    if (t == typeof(ushort))
                        return (object)ushort.Parse(String.Substring(String.find(" ") + 1), NumberStyles.HexNumber);
                    break;
                case 2:
                    string str4 = String.Substring(String.find(" ") + 1);
                    if (t == typeof(string))
                        return (object)str4;
                    if (t == typeof(char[]))
                        return (object)str4.ToCharArray();
                    break;
                case 3:
                    if (t == typeof(double))
                        return (object)double.Parse(String.Substring(String.find(" ") + 1));
                    if (t == typeof(float))
                        return (object)float.Parse(String.Substring(String.find(" ") + 1));
                    break;
                case 4:
                    byte num3 = byte.Parse(String.Substring(String.find(" ") + 1), NumberStyles.HexNumber);
                    if (t == typeof(byte))
                        return (object)num3;
                    if (t == typeof(char))
                        return (object)(char)num3;
                    break;
                case 8:
                    if (t == typeof(long))
                        return (object)long.Parse(String.Substring(String.find(" ") + 1), NumberStyles.HexNumber);
                    if (t == typeof(ulong))
                        return (object)ulong.Parse(String.Substring(String.find(" ") + 1), NumberStyles.HexNumber);
                    break;
            }
            switch (Type)
            {
                case 5:
                    string str5 = String.Substring(String.find(" ") + 1);
                    int index4 = 0;
                    string s1 = "";
                    uint[] numArray1 = new uint[8];
                    foreach (char ch in str5)
                    {
                        switch (ch)
                        {
                            case ',':
                            case ';':
                                numArray1[index4] = uint.Parse(s1, NumberStyles.HexNumber);
                                ++index4;
                                s1 = "";
                                break;
                            default:
                                s1 += ch.ToString();
                                break;
                        }
                        if ((int)ch == 59)
                            break;
                    }
                    return (object)numArray1;
                case 6:
                    string str6 = String.Substring(String.find(" ") + 1);
                    int index5 = 0;
                    string s2 = "";
                    float[] numArray2 = new float[(int)(IntPtr)ArraySize];
                    foreach (char ch in str6)
                    {
                        switch (ch)
                        {
                            case ',':
                            case ';':
                                numArray2[index5] = float.Parse(s2);
                                ++index5;
                                s2 = "";
                                break;
                            default:
                                s2 += ch.ToString();
                                break;
                        }
                        if ((int)ch == 59)
                            break;
                    }
                    return (object)numArray2;
                case 7:
                    string str7 = String.Substring(String.find(" ") + 1);
                    int index6 = 0;
                    string s3 = "";
                    byte[] numArray3 = new byte[(int)(IntPtr)ArraySize];
                    foreach (char ch in str7)
                    {
                        switch (ch)
                        {
                            case ',':
                            case ';':
                                numArray3[index6] = byte.Parse(s3);
                                ++index6;
                                s3 = "";
                                break;
                            default:
                                s3 += ch.ToString();
                                break;
                        }
                        if ((int)ch == 59)
                            break;
                    }
                    return (object)numArray3;
                default:
                    if ((int)Type == (int)JRPCPlusPlus.Uint64Array)
                    {
                        string str3 = String.Substring(String.find(" ") + 1);
                        int index2 = 0;
                        string s4 = "";
                        ulong[] numArray4 = new ulong[(int)(IntPtr)ArraySize];
                        foreach (char ch in str3)
                        {
                            switch (ch)
                            {
                                case ',':
                                case ';':
                                    numArray4[index2] = ulong.Parse(s4);
                                    ++index2;
                                    s4 = "";
                                    break;
                                default:
                                    s4 += ch.ToString();
                                    break;
                            }
                            if ((int)ch == 59)
                                break;
                        }
                        if (t == typeof(ulong))
                            return (object)numArray4;
                        if (t == typeof(long))
                        {
                            long[] numArray5 = new long[(int)(IntPtr)ArraySize];
                            for (int index3 = 0; (long)index3 < (long)ArraySize; ++index3)
                                numArray5[index3] = BitConverter.ToInt64(BitConverter.GetBytes(numArray4[index3]), 0);
                            return (object)numArray5;
                        }
                    }
                    if ((int)Type == (int)JRPCPlusPlus.Void)
                        return (object)0;
                    return (object)ulong.Parse(String.Substring(String.find(" ") + 1), NumberStyles.HexNumber);
            }
        }

        private static uint TypeToType<T>(bool Array) where T : struct
        {
            Type type = typeof(T);
            if (type == typeof(int) || type == typeof(uint) || (type == typeof(short) || type == typeof(ushort)))
            {
                if (Array)
                    return JRPCPlusPlus.IntArray;
                return JRPCPlusPlus.Int;
            }
            if (type == typeof(string) || type == typeof(char[]))
                return JRPCPlusPlus.String;
            if (type == typeof(float) || type == typeof(double))
            {
                if (Array)
                    return JRPCPlusPlus.FloatArray;
                return JRPCPlusPlus.Float;
            }
            if (type == typeof(byte) || type == typeof(char))
            {
                if (Array)
                    return JRPCPlusPlus.ByteArray;
                return JRPCPlusPlus.Byte;
            }
            if ((type == typeof(ulong) || type == typeof(long)) && Array)
                return JRPCPlusPlus.Uint64Array;
            return JRPCPlusPlus.Uint64;
        }

        public static T Call<T>(this IXboxConsole console, uint Address, params object[] Arguments) where T : struct
        {
            return (T)JRPCPlusPlus.CallArgs(console, true, JRPCPlusPlus.TypeToType<T>(false), typeof(T), (string)null, 0, Address, 0U, Arguments);
        }

        public static T Call<T>(this IXboxConsole console, string module, int ordinal, params object[] Arguments) where T : struct
        {
            return (T)JRPCPlusPlus.CallArgs(console, true, JRPCPlusPlus.TypeToType<T>(false), typeof(T), module, ordinal, 0U, 0U, Arguments);
        }

        public static T Call<T>(this IXboxConsole console, JRPCPlusPlus.ThreadType Type, uint Address, params object[] Arguments) where T : struct
        {
            return (T)JRPCPlusPlus.CallArgs(console, Type == JRPCPlusPlus.ThreadType.System, JRPCPlusPlus.TypeToType<T>(false), typeof(T), (string)null, 0, Address, 0U, Arguments);
        }

        public static T Call<T>(this IXboxConsole console, JRPCPlusPlus.ThreadType Type, string module, int ordinal, params object[] Arguments) where T : struct
        {
            return (T)JRPCPlusPlus.CallArgs(console, Type == JRPCPlusPlus.ThreadType.System, JRPCPlusPlus.TypeToType<T>(false), typeof(T), module, ordinal, 0U, 0U, Arguments);
        }

        public static T[] CallArray<T>(this IXboxConsole console, uint Address, uint ArraySize, params object[] Arguments) where T : struct
        {
            if ((int)ArraySize == 0)
                return new T[1];
            return (T[])JRPCPlusPlus.CallArgs(console, true, JRPCPlusPlus.TypeToType<T>(true), typeof(T), (string)null, 0, Address, ArraySize, Arguments);
        }

        public static T[] CallArray<T>(this IXboxConsole console, string module, int ordinal, uint ArraySize, params object[] Arguments) where T : struct
        {
            if ((int)ArraySize == 0)
                return new T[1];
            return (T[])JRPCPlusPlus.CallArgs(console, true, JRPCPlusPlus.TypeToType<T>(true), typeof(T), module, ordinal, 0U, ArraySize, Arguments);
        }

        public static T[] CallArray<T>(this IXboxConsole console, JRPCPlusPlus.ThreadType Type, uint Address, uint ArraySize, params object[] Arguments) where T : struct
        {
            if ((int)ArraySize == 0)
                return new T[1];
            return (T[])JRPCPlusPlus.CallArgs(console, Type == JRPCPlusPlus.ThreadType.System, JRPCPlusPlus.TypeToType<T>(true), typeof(T), (string)null, 0, Address, ArraySize, Arguments);
        }

        public static T[] CallArray<T>(this IXboxConsole console, JRPCPlusPlus.ThreadType Type, string module, int ordinal, uint ArraySize, params object[] Arguments) where T : struct
        {
            if ((int)ArraySize == 0)
                return new T[1];
            return (T[])JRPCPlusPlus.CallArgs(console, Type == JRPCPlusPlus.ThreadType.System, JRPCPlusPlus.TypeToType<T>(true), typeof(T), module, ordinal, 0U, ArraySize, Arguments);
        }

        public static string CallString(this IXboxConsole console, uint Address, params object[] Arguments)
        {
            return (string)JRPCPlusPlus.CallArgs(console, true, JRPCPlusPlus.String, typeof(string), (string)null, 0, Address, 0U, Arguments);
        }

        public static string CallString(this IXboxConsole console, string module, int ordinal, params object[] Arguments)
        {
            return (string)JRPCPlusPlus.CallArgs(console, true, JRPCPlusPlus.String, typeof(string), module, ordinal, 0U, 0U, Arguments);
        }

        public static string CallString(this IXboxConsole console, JRPCPlusPlus.ThreadType Type, uint Address, params object[] Arguments)
        {
            return (string)JRPCPlusPlus.CallArgs(console, Type == JRPCPlusPlus.ThreadType.System, JRPCPlusPlus.String, typeof(string), (string)null, 0, Address, 0U, Arguments);
        }

        public static string CallString(this IXboxConsole console, JRPCPlusPlus.ThreadType Type, string module, int ordinal, params object[] Arguments)
        {
            return (string)JRPCPlusPlus.CallArgs(console, Type == JRPCPlusPlus.ThreadType.System, JRPCPlusPlus.String, typeof(string), module, ordinal, 0U, 0U, Arguments);
        }

        public static void CallVoid(this IXboxConsole console, uint Address, params object[] Arguments)
        {
            JRPCPlusPlus.CallArgs(console, true, JRPCPlusPlus.Void, typeof(void), (string)null, 0, Address, 0U, Arguments);
        }

        public static void CallVoid(this IXboxConsole console, string module, int ordinal, params object[] Arguments)
        {
            JRPCPlusPlus.CallArgs(console, true, JRPCPlusPlus.Void, typeof(void), module, ordinal, 0U, 0U, Arguments);
        }

        public static void CallVoid(this IXboxConsole console, JRPCPlusPlus.ThreadType Type, uint Address, params object[] Arguments)
        {
            JRPCPlusPlus.CallArgs(console, Type == JRPCPlusPlus.ThreadType.System, JRPCPlusPlus.Void, typeof(void), (string)null, 0, Address, 0U, Arguments);
        }

        public static void CallVoid(this IXboxConsole console, JRPCPlusPlus.ThreadType Type, string module, int ordinal, params object[] Arguments)
        {
            JRPCPlusPlus.CallArgs(console, Type == JRPCPlusPlus.ThreadType.System, JRPCPlusPlus.Void, typeof(void), module, ordinal, 0U, 0U, Arguments);
        }

        public static bool Connect(this IXboxConsole console, out IXboxConsole Console, string XboxNameOrIP = "default")
        {
            if (XboxNameOrIP == "default")
                XboxNameOrIP = ((IXboxManager)new XboxManager()).DefaultConsole;
            IXboxConsole xboxConsole = (IXboxConsole)((IXboxManager)new XboxManager()).OpenConsole(XboxNameOrIP);
            int num = 0;
            bool flag = false;
            while (!flag)
            {
                try
                {
                    JRPCPlusPlus.connectionId = xboxConsole.OpenConnection((string)null);
                    flag = true;
                }
                catch (COMException ex)
                {
                    if (ex.ErrorCode == JRPCPlusPlus.UIntToInt(2195325184U))
                    {
                        if (num >= 3)
                        {
                            Console = xboxConsole;
                            return false;

                        }
                        ++num;
                        Thread.Sleep(100);
                    }
                    else
                    {
                        Console = xboxConsole;
                        return false;
                    }
                }
            }
            Console = xboxConsole;
            return true;
        }

        public static string ConsoleType(this IXboxConsole console)
        {
            string Command = "consolefeatures ver=" + JRPCPlusPlus.jrpcVersion + " type=17 params=\"A\\0\\A\\0\\\"";
            string String = JRPCPlusPlus.SendCommand(console, Command);
            return String.Substring(String.find(" ") + 1);
        }

        public static void constantMemorySet(this IXboxConsole console, uint Address, uint Value, uint TitleID)
        {
            JRPCPlusPlus.constantMemorySetting(console, Address, Value, false, 0U, true, TitleID);
        }

        public static void constantMemorySet(this IXboxConsole console, uint Address, uint Value, uint IfValue, uint TitleID)
        {
            JRPCPlusPlus.constantMemorySetting(console, Address, Value, true, IfValue, true, TitleID);
        }

        public static void constantMemorySetting(IXboxConsole console, uint Address, uint Value, bool useIfValue, uint IfValue, bool usetitleID, uint TitleID)
        {
            string Command = "consolefeatures ver=" + JRPCPlusPlus.jrpcVersion + " type=18 params=\"A\\" + Address.ToString("X") + "\\A\\5\\" + JRPCPlusPlus.Int + "\\" + JRPCPlusPlus.UIntToInt(Value) + "\\" + JRPCPlusPlus.Int + "\\" + (useIfValue ? 1 : 0) + "\\" + JRPCPlusPlus.Int + "\\" + IfValue + "\\" + JRPCPlusPlus.Int + "\\" + (usetitleID ? 1 : 0) + "\\" + JRPCPlusPlus.Int + "\\" + JRPCPlusPlus.UIntToInt(TitleID) + "\\\"";
            JRPCPlusPlus.SendCommand(console, Command);
        }

        internal static ulong ConvertToUInt64(object o)
        {
            if (o is bool)
                return (bool)o ? 1UL : 0UL;
            if (o is byte)
                return (ulong)(byte)o;
            if (o is short)
                return (ulong)(short)o;
            if (o is int)
                return (ulong)(int)o;
            if (o is long)
                return (ulong)(long)o;
            if (o is ushort)
                return (ulong)(ushort)o;
            if (o is uint)
                return (ulong)(uint)o;
            if (o is ulong)
                return (ulong)o;
            if (o is float)
                return (ulong)BitConverter.DoubleToInt64Bits((double)(float)o);
            if (o is double)
                return (ulong)BitConverter.DoubleToInt64Bits((double)o);
            return 0;
        }

        private static Dictionary<Type, int> ValueTypeSizeMap = new Dictionary<Type, int>()
       {
        {
        typeof (bool),
        4
      },
      {
        typeof (byte),
        1
      },
      {
        typeof (short),
        2
      },
      {
        typeof (int),
        4
      },
      {
        typeof (long),
        8
      },
      {
        typeof (ushort),
        2
      },
      {
        typeof (uint),
        4
      },
      {
        typeof (ulong),
        8
      },
      {
        typeof (float),
        4
      },
      {
        typeof (double),
        8
      }
    };
        private static Dictionary<Type, int> StructPrimitiveSizeMap = new Dictionary<Type, int>();
        private static HashSet<Type> ValidReturnTypes = new HashSet<Type>()
    {
      typeof (void),
      typeof (bool),
      typeof (byte),
      typeof (short),
      typeof (int),
      typeof (long),
      typeof (ushort),
      typeof (uint),
      typeof (ulong),
      typeof (float),
      typeof (double),
      typeof (string),
      typeof (bool[]),
      typeof (byte[]),
      typeof (short[]),
      typeof (int[]),
      typeof (long[]),
      typeof (ushort[]),
      typeof (uint[]),
      typeof (ulong[]),
      typeof (float[]),
      typeof (double[]),
      typeof (string[])
    };

        public static bool FanSpeed(int fan, int speed)
        {
            if (fan == 1)
                m_SMCMessage[0] = 0x94;
            else if (fan == 2)
                m_SMCMessage[0] = 0x89;
            else
                return false;
            if (speed > 100)
                speed = 100;
            if (speed <= 0)
                speed = 10;
            if (speed < 45)
                m_SMCMessage[1] = 0x7F;
            else
                m_SMCMessage[1] = (byte)(speed | 0x80);
            return true;
        }

        public static int find(this string String, string _Ptr)
        {
            if (_Ptr.Length == 0 || String.Length == 0)
                return -1;
            for (int index1 = 0; index1 < String.Length; ++index1)
            {
                if ((int)String[index1] == (int)_Ptr[0])
                {
                    bool flag = true;
                    for (int index2 = 0; index2 < _Ptr.Length; ++index2)
                    {
                        if ((int)String[index1 + index2] != (int)_Ptr[index2])
                            flag = false;
                    }
                    if (flag)
                        return index1;
                }
            }
            return -1;
        }

        public static string GetConsoleID(this IXboxConsole console)
        {
            string String = SendCommand(console, "getconsoleid");
            return String.Substring(String.find(" ") + 1).Replace("consoleid=","");
        }

        public static string GetCPUKey(this IXboxConsole console)
        {
            string Command = "consolefeatures ver=" + (object)JRPCPlusPlus.jrpcVersion + " type=10 params=\"A\\0\\A\\0\\\"";
            string String = JRPCPlusPlus.SendCommand(console, Command);
            return String.Substring(String.find(" ") + 1);
        }

        public static string GetDebugVersion(this IXboxConsole console)
        {
            return SendCommand(console, "dmversion").Replace("200- ", string.Empty);
        }

        public static uint XAMGamertagWCHARRetail = 0x81AA28FD;//0x81AA28FC;
        public static uint XAMOfflineXuidDevKit = 0x81D44460;
        private static uint XAMGamertagWCHARDevkit = 0x81D44475;
        private static uint XAMXuidRetail = 0x81AA291C;
        private static uint XAMGamertagRetail = 0x81AEE9FC; //81AA28FD
        private static uint XAMProfileIDRetail = 0x81AA28E8;
        //public static uint XAMOfflineXuid = 0;
        //public static bool IsDevkit;
        public static uint address { get; set; }

        public static string GetGamertag(this IXboxConsole console, bool IsDevkit)
        {           
            if (IsDevkit)
            {
                address = XAMGamertagWCHARDevkit;
            }
            else
            {
                address = XAMGamertagWCHARRetail;
            }
            byte[] memory = GetMemory(console, address, 30);          
            return Encoding.ASCII.GetString(memory);
        }
        public static ulong Xuid(this IXboxConsole console)
        {
            if (IsDevKit)
                return 0;
            else
            {
                return console.ReadUInt64(XAMXuidRetail);
            }
        }
        public static bool IsDevKit;
        public static string GetGamertagWinUI(this IXboxConsole console)
        {
            if (IsDevKit == true)
                address = XAMGamertagWCHARDevkit;
            else
                address = XAMGamertagWCHARRetail;
            byte[] mem = GetMemory(console, address, 30);
            return Encoding.Unicode.GetString(mem);
        }

        public static uint GetKernalVersion(this IXboxConsole console)
        {
            string Command = "consolefeatures ver=" + (object)JRPCPlusPlus.jrpcVersion + " type=13 params=\"A\\0\\A\\0\\\"";
            string String = JRPCPlusPlus.SendCommand(console, Command);
            return uint.Parse(String.Substring(String.find(" ") + 1));
        }

        public static byte[] GetMemory(this IXboxConsole console, uint Address, uint Length)
        {
            uint BytesRead = 0;
            byte[] Data = new byte[(int)(IntPtr)Length];
            console.DebugTarget.GetMemory(Address, Length, Data, out BytesRead);
            console.DebugTarget.InvalidateMemoryCache(true, Address, Length);
            return Data;
        }

        public static uint GetModuleHandle(this IXboxConsole console, string moduleName)
        {
            return console.Call<uint>(moduleName, 0x44e, new object[] { moduleName });
        }

        public static void GetSignInState(this IXboxConsole console)
        {
            console.ResolveFunction("xboxkrnl.exe", 528);
        }

        public static string GetSMCVersion(this IXboxConsole console)
        {
            byte[] bytes = GetMemory(console, 0x81AC7C50, 4);
            return string.Concat(new object[] { " ", bytes[2], " ", bytes[3] });
        }

        public static uint GetTemperature(this IXboxConsole console, JRPCPlusPlus.TemperatureType TemperatureType)
        {
            string Command = "consolefeatures ver=" + (object)JRPCPlusPlus.jrpcVersion + " type=15 params=\"A\\0\\A\\1\\" + (object)JRPCPlusPlus.Int + "\\" + (object)TemperatureType + "\\\"";
            string String = JRPCPlusPlus.SendCommand(console, Command);
            return uint.Parse(String.Substring(String.find(" ") + 1), NumberStyles.HexNumber);
        }

        public static string GetTitlePath(this IXboxConsole console)
        {
            string Command = "xbeinfo name=";
            string String = JRPCPlusPlus.SendCommand(console, Command);
            return String.Substring(String.find(" "));
        }

        private static byte[] IntArrayToByte(int[] iArray)
        {
            byte[] numArray = new byte[iArray.Length * 4];
            int index1 = 0;
            int num = 0;
            while (index1 < iArray.Length)
            {
                for (int index2 = 0; index2 < 4; ++index2)
                    numArray[num + index2] = BitConverter.GetBytes(iArray[index1])[index2];
                ++index1;
                num += 4;
            }
            return numArray;
        }

        public static bool IsTrayOpen
        {
            get
            {
                return false;
            }
            set
            {

            }
        }

        public static void OpenTray(IXboxConsole console)
        {
            console.CallVoid(ResolveFunction(console, "xam.xex", 0x60), new object[] { 0, 0, 0, 0 });
            IsTrayOpen = true;
        }

        public static void CloseTray(IXboxConsole console)
        {
            console.CallVoid(ResolveFunction(console, "xam.xex", 0x62), new object[] { 0, 0, 0, 0 });
            IsTrayOpen = false;
        }

        public static string memstat(IXboxConsole console)
        {
            return SendCommand(console, "consolemem");
        }
        public static void CmdLine(IXboxConsole console, string Command)
        {
            SendCommand(console, "cmdline=\"" + Command);
        }

        internal static bool IsValidReturnType(Type t)
        {
            return JRPCPlusPlus.ValidReturnTypes.Contains(t);
        }

        internal static bool IsValidStructType(Type t)
        {
            if (!t.IsPrimitive)
                return t.IsValueType;
            return false;
        }

        public static void LaunchTitle(this IXboxConsole console, string path)
        {
            string mediaDirectory = path;

            mediaDirectory.Replace("default_mp.xex", "").Replace("default.xex", "");

            console.Reboot(path, mediaDirectory, null, XboxRebootFlags.Title);
        }

        public enum LEDState
        {
            OFF = 0,
            RED = 8,
            GREEN = 128, // 0x00000080
            ORANGE = 136, // 0x00000088
        }

        public static void LoadModule(this IXboxConsole console, string module)
        {
            //if (module.Contains("\\") == false)
               // module = "Hdd:\\" + module;

            console.Call<uint>(ThreadType.System, "xboxkrnl.exe", 409, new object[] { module, 8, 0, 0 });
        }

        public static void Push(this byte[] InArray, out byte[] OutArray, byte Value)
        {
            OutArray = new byte[InArray.Length + 1];
            InArray.CopyTo((Array)OutArray, 0);
            OutArray[InArray.Length] = Value;
        }

        public static bool ReadBool(this IXboxConsole console, uint Address)
        {
            return (int)console.GetMemory(Address, 1U)[0] != 0;
        }

        public static byte ReadByte(this IXboxConsole console, uint Address)
        {
            return console.GetMemory(Address, 1U)[0];
        }

        public static sbyte ReadSByte(this IXboxConsole console, uint Address)
        {
            return (sbyte)console.GetMemory(Address, 1U)[0];
        }

        public static float ReadFloat(this IXboxConsole console, uint Address)
        {
            byte[] memory = console.GetMemory(Address, 4U);
            ReverseBytes(memory, 4);
            return BitConverter.ToSingle(memory, 0);
        }

        public static float[] ReadFloat(this IXboxConsole console, uint Address, uint ArraySize)
        {
            float[] numArray = new float[(int)(IntPtr)ArraySize];
            byte[] memory = console.GetMemory(Address, ArraySize * 4U);
            ReverseBytes(memory, 4);
            for (int index = 0; (long)index < (long)ArraySize; ++index)
                numArray[index] = BitConverter.ToSingle(memory, index * 4);
            return numArray;
        }
        public static short ReadInt16(this IXboxConsole console, uint Address)
        {
            byte[] memory = console.GetMemory(Address, 2U);
            ReverseBytes(memory, 2);
            return BitConverter.ToInt16(memory, 0);
        }
        public static short[] ReadInt16(this IXboxConsole console, uint Address, uint ArraySize)
        {
            short[] numArray = new short[(int)(IntPtr)ArraySize];
            byte[] memory = console.GetMemory(Address, ArraySize * 2U);
            ReverseBytes(memory, 2);
            for (int index = 0; (long)index < (long)ArraySize; ++index)
                numArray[index] = BitConverter.ToInt16(memory, index * 2);
            return numArray;
        }
        public static ushort ReadUInt16(this IXboxConsole console, uint Address)
        {
            byte[] memory = console.GetMemory(Address, 2U);
            ReverseBytes(memory, 2);
            return BitConverter.ToUInt16(memory, 0);
        }

        public static ushort[] ReadUInt16(this IXboxConsole console, uint Address, uint ArraySize)
        {
            ushort[] numArray = new ushort[(int)(IntPtr)ArraySize];
            byte[] memory = console.GetMemory(Address, ArraySize * 2U);
            ReverseBytes(memory, 2);
            for (int index = 0; (long)index < (long)ArraySize; ++index)
                numArray[index] = BitConverter.ToUInt16(memory, index * 2);
            return numArray;
        }
        public static int ReadInt32(this IXboxConsole console, uint Address)
        {
            byte[] memory = console.GetMemory(Address, 4U);
            ReverseBytes(memory, 4);
            return BitConverter.ToInt32(memory, 0);
        }
        public static int[] ReadInt32(this IXboxConsole console, uint Address, uint ArraySize)
        {
            int[] numArray = new int[(int)(IntPtr)ArraySize];
            byte[] memory = console.GetMemory(Address, ArraySize * 4U);
            ReverseBytes(memory, 4);
            for (int index = 0; (long)index < (long)ArraySize; ++index)
                numArray[index] = BitConverter.ToInt32(memory, index * 4);
            return numArray;
        }
        public static uint ReadUInt32(this IXboxConsole console, uint Address)
        {
            byte[] memory = console.GetMemory(Address, 4U);
            ReverseBytes(memory, 4);
            return BitConverter.ToUInt32(memory, 0);
        }
        public static uint[] ReadUInt32(this IXboxConsole console, uint Address, uint ArraySize)
        {
            uint[] numArray = new uint[(int)(IntPtr)ArraySize];
            byte[] memory = console.GetMemory(Address, ArraySize * 4U);
            ReverseBytes(memory, 4);
            for (int index = 0; (long)index < (long)ArraySize; ++index)
                numArray[index] = BitConverter.ToUInt32(memory, index * 4);
            return numArray;
        }

        public static void SearchUInt32(this IXboxConsole console, uint StartAddress, uint Length, uint Value)
        {

        }
        public static long ReadInt64(this IXboxConsole console, uint Address)
        {
            byte[] memory = console.GetMemory(Address, 8U);
            ReverseBytes(memory, 8);
            return BitConverter.ToInt64(memory, 0);
        }

        public static long[] ReadInt64(this IXboxConsole console, uint Address, uint ArraySize)
        {
            long[] numArray = new long[(int)(IntPtr)ArraySize];
            byte[] memory = console.GetMemory(Address, ArraySize * 8U);
            ReverseBytes(memory, 8);
            for (int index = 0; (long)index < (long)ArraySize; ++index)
                numArray[index] = (long)BitConverter.ToUInt32(memory, index * 8);
            return numArray;
        }

        public static ulong ReadUInt64(this IXboxConsole console, uint Address)
        {
            byte[] memory = console.GetMemory(Address, 8U);
            ReverseBytes(memory, 8);
            return BitConverter.ToUInt64(memory, 0);
        }

        public static ulong[] ReadUInt64(this IXboxConsole console, uint Address, uint ArraySize)
        {
            ulong[] numArray = new ulong[(int)(IntPtr)ArraySize];
            byte[] memory = console.GetMemory(Address, ArraySize * 8U);
            ReverseBytes(memory, 8);
            for (int index = 0; (long)index < (long)ArraySize; ++index)
                numArray[index] = (ulong)BitConverter.ToUInt32(memory, index * 8);
            return numArray;
        }

        public static string ReadString(this IXboxConsole console, uint Address, uint size)
        {
            return Encoding.UTF8.GetString(console.GetMemory(Address, size));
        }

        public static uint ResolveFunction(this IXboxConsole console, string ModuleName, uint Ordinal)
        {
            string Command = "consolefeatures ver=" + (object)JRPCPlusPlus.jrpcVersion + " type=9 params=\"A\\0\\A\\2\\" + (object)JRPCPlusPlus.String + "/" + (object)ModuleName.Length + "\\" + ModuleName.ToHexString() + "\\" + (object)JRPCPlusPlus.Int + "\\" + (object)Ordinal + "\\\"";
            string String = JRPCPlusPlus.SendCommand(console, Command);
            return uint.Parse(String.Substring(String.find(" ") + 1), NumberStyles.HexNumber);
        }

        private static void ReverseBytes(byte[] buffer, int groupSize)
        {
            if (buffer.Length % groupSize != 0)
                throw new ArgumentException("Group size must be a multiple of the buffer length");
            int num1 = 0;
            while (num1 < buffer.Length)
            {
                int index1 = num1;
                for (int index2 = num1 + groupSize - 1; index1 < index2; --index2)
                {
                    byte num2 = buffer[index1];
                    buffer[index1] = buffer[index2];
                    buffer[index2] = num2;
                    ++index1;
                }
                num1 += groupSize;
            }
        }
        public static uint Search_Byte(this IXboxConsole console, uint StartOffset, uint Length, byte Byte)
        {
            return Search_Byte(console, StartOffset, Length, new byte[] { Byte });
        }

        public static uint Search_Byte(this IXboxConsole console, uint StartOffset, uint Length, byte[] Bytes)
        {
            //uint BytesWritten;
            uint addr = StartOffset;
            byte[] data = console.GetMemory(StartOffset, Length);
            for (uint i = 0; i < (Length - Bytes.Length); i++)
            {
                byte[] buffer = new byte[Bytes.Length];
                Array.Copy(data, i, buffer, 0, Bytes.Length);
                if (buffer.SequenceEqual(Bytes))
                {
                    addr += i++;
                    break;
                }
            }
            return addr;
        }

        public static uint Search_Int32(this IXboxConsole console, uint StartOffset, uint Length, int Int32)
        {
            byte[] bytes = BitConverter.GetBytes(Int32);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return Search_Byte(console, StartOffset, Length, bytes);
        }

        public static uint Search_Int64(this IXboxConsole console, uint StartOffset, uint Length, long Int64)
        {
            byte[] bytes = BitConverter.GetBytes(Int64);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return Search_Byte(console, StartOffset, Length, bytes);
        }

        public static uint Search_String(this IXboxConsole console, uint StartOffset, uint Length, string String)
        {
            return Search_Byte(console, StartOffset, (uint)String.Length, Encoding.ASCII.GetBytes(String));
        }

        public static uint Search_UInt16(this IXboxConsole console, uint StartOffset, uint Length, ushort UInt16)
        {
            byte[] bytes = BitConverter.GetBytes(UInt16);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return Search_Byte(console, StartOffset, Length, bytes);
        }

        public static uint Search_UInt32(this IXboxConsole console, uint StartOffset, uint Length, uint UInt32)
        {
            byte[] bytes = BitConverter.GetBytes(UInt32);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return Search_Byte(console, StartOffset, Length, bytes);
        }

        public static uint Search_UInt64(this IXboxConsole console, uint StartOffset, uint Length, ulong UInt64)
        {
            byte[] bytes = BitConverter.GetBytes(UInt64);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return Search_Byte(console, StartOffset, Length, bytes);
        }

        public static string SendCommand(this IXboxConsole console, string Command)
        {
            string Response;
            try
            {
                console.SendTextCommand(JRPCPlusPlus.connectionId, Command, out Response);
                if (Response.Contains("error="))
                    throw new Exception(Response.Substring(11));
                if (Response.Contains("DEBUG"))
                    throw new Exception("JRPC2 is not installed on the current console");
            }
            catch (COMException ex)
            {
                if (ex.ErrorCode == UIntToInt(2195324935U))
                    throw new Exception("JRPC2 is not installed on the current console");
                throw new Exception(ex.Message) ;
            }
            return Response;
        }

        public static void SetConsoleColor(this IXboxConsole console, XboxColor Color)
        {
            SendCommand(console, "setcolor name=" + Enum.GetName(typeof(int), Color).ToLower());
        }

        public static void SetLeds(this IXboxConsole console, uint Top_Left, uint Top_Right, uint Bottom_Left, uint Bottom_Right)
        {
            string Command = "consolefeatures ver=" + JRPCPlusPlus.jrpcVersion + " type=14 params=\"A\\0\\A\\4\\" + JRPCPlusPlus.Int + "\\" + (uint)Top_Left + "\\" + JRPCPlusPlus.Int + "\\" + (uint)Top_Right + "\\" + JRPCPlusPlus.Int + "\\" + (uint)Bottom_Left + "\\" + JRPCPlusPlus.Int + "\\" + (object)(uint)Bottom_Right + "\\\"";
            JRPCPlusPlus.SendCommand(console, Command);
        }

        public static void SetLeds_(this IXboxConsole console, string Top_Left, string Top_Right, string Bottom_Left, string Bottom_Right)
        {
            uint tl = 0;
            uint tr = 0;
            uint bl = 0;
            uint br = 0;
            //
            try
            {
                if (Top_Left == "OFF")
                    tl = 0;
                else if (Top_Left == "RED")
                    tl = 8;
                else if (Top_Left == "GREEN")
                    tl = 128;
                else if (Top_Left == "ORANGE")
                    tl = 136;
                if (Top_Right == "OFF")
                    tr = 0;
                else if (Top_Right == "RED")
                    tr = 8;
                else if (Top_Right == "GREEN")
                    tr = 128;
                else if (Top_Right == "ORANGE")
                    tr = 136;
                if (Bottom_Left == "OFF")
                    bl = 0;
                else if (Bottom_Left == "RED")
                    bl = 8;
                else if (Bottom_Left == "GREEN")
                    bl = 128;
                else if (Bottom_Left == "ORANGE")
                    bl = 136;
                if (Bottom_Right == "OFF")
                    br = 0;
                else if (Bottom_Right == "RED")
                    br = 8;
                else if (Bottom_Right == "GREEN")
                    br = 128;
                else if (Bottom_Right == "ORANGE")
                    br = 136;
                console.SetLeds(tl, tr, bl, br);
            }
            catch (COMException)
            {

            }
        }

        public static void SetMemory(this IXboxConsole console, uint Address, byte[] Data)
        {
            uint BytesWritten;
            console.DebugTarget.SetMemory(Address, (uint)Data.Length, Data, out BytesWritten);
        }

        public static void SetStats_LE(this IXboxConsole console, uint Address, string ByteString)
        {
            byte[] bytes = BitConverter.GetBytes(Convert.ToUInt32(ByteString));
            console.WriteUInt32(Address, Convert.ToUInt32(ByteString));
            Array.Reverse(BitConverter.GetBytes(Convert.ToUInt32(ByteString)));
        }

        public static void ShutDownConsole(this IXboxConsole console)
        {
            try
            {
                string Command = "consolefeatures ver=" + (object)JRPCPlusPlus.jrpcVersion + " type=11 params=\"A\\0\\A\\0\\\"";
                JRPCPlusPlus.SendCommand(console, Command);
            }
            catch
            {
            }
        }

        public static byte[] StringToByteArray(string hex)
        {

            return (from x in Enumerable.Range(0, hex.Length)

                    where (x % 2) == 0

                    select Convert.ToByte(hex.Substring(x, 2), 0x10)).ToArray<byte>();

        }

        public enum TemperatureType
        {
            CPU,
            GPU,
            EDRAM,
            MotherBoard,
        }

        public enum ThreadType
        {
            System,
            Title,
        }

        public static byte[] ToByteArray(this string String)
        {
            byte[] numArray = new byte[String.Length + 1];
            for (int index = 0; index < String.Length; ++index)
                numArray[index] = (byte)String[index];
            return numArray;
        }

        public static string ToHexString(this string String)
        {
            string str = "";
            foreach (byte num in String)
                str += num.ToString("X2");
            return str;
        }

        public static byte[] ToWCHAR(this string String)
        {
            return JRPCPlusPlus.WCHAR(String);
        }

        public static int UIntToInt(uint Value)
        {
            return BitConverter.ToInt32(BitConverter.GetBytes(Value), 0);
        }

        public static short[] GetInt16(this IXboxConsole console, uint Address, uint ArraySize)
        {
            {
                short[] num = new short[ArraySize];
                byte[] memory = console.GetMemory(Address, ArraySize * 2);
                ReverseBytes(memory, 2);
                for (int i = 0; i < ArraySize; i++)
                {
                    num[i] = BitConverter.ToInt16(memory, i * 2);
                }
                return num;
            }
        }

        public static void UnloadImage(this IXboxConsole console, string ModuleName, bool isSysDll)
        {
            uint moduleHandle = console.GetModuleHandle(ModuleName);
            if (moduleHandle != 0)
            {
                if (isSysDll)
                {
                    GetInt16(console, moduleHandle + 0x40, 1);
                }
                object[] arguments = new object[] { moduleHandle };
                console.CallVoid("xboxkrnl.exe", 0x1a1, arguments);
            }
        }

        public static void UnloadModule(this IXboxConsole xbCon, string module)
        {
            uint handle = xbCon.Call<uint>(ThreadType.System, "xam.xex", 1102, new object[] { module });
            if (handle != 0u)
            {
                xbCon.WriteInt16(handle + 0x40, 1);
                xbCon.Call<uint>(ThreadType.System, "xboxkrnl.exe", 417, new object[] { handle });
            }
        }

        public static byte[] WCHAR(string String)
        {
            byte[] numArray = new byte[String.Length * 2 + 2];
            int index = 1;
            foreach (byte num in String)
            {
                numArray[index] = num;
                index += 2;
            }
            return numArray;
        }

        public static void WriteByte(this IXboxConsole console, uint Address, byte Value)
        {
            console.SetMemory(Address, new byte[1] { Value });
        }

        public static void WriteSByte(this IXboxConsole console, uint Address, sbyte Value)
        {
            console.SetMemory(Address, new byte[1]
            {
              BitConverter.GetBytes((short) Value)[0]
            });
        }

        public static void WriteSByte(this IXboxConsole console, uint Address, sbyte[] Value)
        {
            byte[] OutArray = new byte[0];
            foreach (byte num in Value)
                OutArray.Push(out OutArray, num);
            console.SetMemory(Address, OutArray);
        }

        public static void WriteByte(this IXboxConsole console, uint Address, byte[] Value)
        {
            console.SetMemory(Address, Value);
        }
        public static void WriteFloat(this IXboxConsole console, uint Address, float Value)
        {
            byte[] bytes = BitConverter.GetBytes(Value);
            Array.Reverse((Array)bytes);
            console.SetMemory(Address, bytes);
        }

        public static void WriteFloat(this IXboxConsole console, uint Address, float[] Value)
        {
            byte[] numArray = new byte[Value.Length * 4];
            for (int index = 0; index < Value.Length; ++index)
                BitConverter.GetBytes(Value[index]).CopyTo((Array)numArray, index * 4);
            ReverseBytes(numArray, 4);
            console.SetMemory(Address, numArray);
        }
        public static void Write_Hook(this IXboxConsole console, uint Offset, uint Destination, bool Linked)
        {
            uint[] Func = new uint[4];
            if ((Destination & 0x8000) != 0)
                Func[0] = 0x3D600000 + (((Destination >> 16) & 0xFFFF) + 1);
            else
                Func[0] = 0x3D600000 + ((Destination >> 16) & 0xFFFF);

            Func[1] = 0x396B0000 + (Destination & 0xFFFF);
            Func[2] = 0x7D6903A6;
            if (Linked)
                Func[3] = 0x4E800421;
            else
                Func[3] = 0x4E800420;
            byte[] buffer = new byte[0x10];
            byte[] f1 = BitConverter.GetBytes(Func[0]);
            byte[] f2 = BitConverter.GetBytes(Func[1]);
            byte[] f3 = BitConverter.GetBytes(Func[2]);
            byte[] f4 = BitConverter.GetBytes(Func[3]);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(f1);
                Array.Reverse(f2);
                Array.Reverse(f3);
                Array.Reverse(f4);
            }
            for (int i = 0; i < 4; i++)
                buffer[i] = f1[i];
            for (int i = 4; i < 8; i++)
                buffer[i] = f2[i - 4];
            for (int i = 8; i < 0xC; i++)
                buffer[i] = f3[i - 8];
            for (int i = 0xC; i < 0x10; i++)
                buffer[i] = f4[i - 0xC];
            console.WriteByte(Offset, buffer);
        }

        public static void WriteInt16(this IXboxConsole console, uint Address, short Value)
        {
            byte[] bytes = BitConverter.GetBytes(Value);
            ReverseBytes(bytes, 2);
            console.SetMemory(Address, bytes);
        }

        public static void WriteInt16(this IXboxConsole console, uint Address, short[] Value)
        {
            byte[] numArray = new byte[Value.Length * 2];
            for (int index = 0; index < Value.Length; ++index)
                BitConverter.GetBytes(Value[index]).CopyTo((Array)numArray, index * 2);
            ReverseBytes(numArray, 2);
            console.SetMemory(Address, numArray);
        }

        public static void WriteUInt16(this IXboxConsole console, uint Address, ushort Value)
        {
            byte[] bytes = BitConverter.GetBytes(Value);
            ReverseBytes(bytes, 2);
            console.SetMemory(Address, bytes);
        }

        public static void WriteUInt16(this IXboxConsole console, uint Address, ushort[] Value)
        {
            byte[] numArray = new byte[Value.Length * 2];
            for (int index = 0; index < Value.Length; ++index)
                BitConverter.GetBytes(Value[index]).CopyTo((Array)numArray, index * 2);
            ReverseBytes(numArray, 2);
            console.SetMemory(Address, numArray);
        }

        public static void WriteInt32(this IXboxConsole console, uint Address, int Value)
        {
            byte[] bytes = BitConverter.GetBytes(Value);
            ReverseBytes(bytes, 4);
            console.SetMemory(Address, bytes);
        }

        public static void WriteInt32(this IXboxConsole console, uint Address, int[] Value)
        {
            byte[] numArray = new byte[Value.Length * 4];
            for (int index = 0; index < Value.Length; ++index)
                BitConverter.GetBytes(Value[index]).CopyTo((Array)numArray, index * 4);
            ReverseBytes(numArray, 4);
            console.SetMemory(Address, numArray);
        }

        public static void WriteUInt32(this IXboxConsole console, uint Address, uint Value)
        {
            byte[] bytes = BitConverter.GetBytes(Value);
            ReverseBytes(bytes, 4);
            console.SetMemory(Address, bytes);
        }

        public static void WriteUInt32(this IXboxConsole console, uint Address, uint[] Value)
        {
            byte[] numArray = new byte[Value.Length * 4];
            for (int index = 0; index < Value.Length; ++index)
                BitConverter.GetBytes(Value[index]).CopyTo((Array)numArray, index * 4);
            ReverseBytes(numArray, 4);
            console.SetMemory(Address, numArray);
        }

        public static void WriteInt64(this IXboxConsole console, uint Address, long Value)
        {
            byte[] bytes = BitConverter.GetBytes(Value);
            ReverseBytes(bytes, 8);
            console.SetMemory(Address, bytes);
        }

        public static void WriteInt64(this IXboxConsole console, uint Address, long[] Value)
        {
            byte[] numArray = new byte[Value.Length * 8];
            for (int index = 0; index < Value.Length; ++index)
                BitConverter.GetBytes(Value[index]).CopyTo((Array)numArray, index * 8);
            ReverseBytes(numArray, 8);
            console.SetMemory(Address, numArray);
        }

        public static void WriteUInt64(this IXboxConsole console, uint Address, ulong Value)
        {
            byte[] bytes = BitConverter.GetBytes(Value);
            ReverseBytes(bytes, 8);
            console.SetMemory(Address, bytes);
        }

        public static void WriteUInt64(this IXboxConsole console, uint Address, ulong[] Value)
        {
            byte[] numArray = new byte[Value.Length * 8];
            for (int index = 0; index < Value.Length; ++index)
                BitConverter.GetBytes(Value[index]).CopyTo((Array)numArray, index * 8);
            ReverseBytes(numArray, 8);
            console.SetMemory(Address, numArray);
        }

        public static void WriteString(this IXboxConsole console, uint Address, string String)
        {
            byte[] OutArray = new byte[0];
            foreach (byte num in String)
                OutArray.Push(out OutArray, num);
            OutArray.Push(out OutArray, (byte)0);
            console.SetMemory(Address, OutArray);
        }

        public static uint XamGetCurrentTitleId(this IXboxConsole console)
        {
            string Command = "consolefeatures ver=" + JRPCPlusPlus.jrpcVersion + " type=16 params=\"A\\0\\A\\0\\\"";
            string String = JRPCPlusPlus.SendCommand(console, Command);
            return uint.Parse(String.Substring(String.find(" ") + 1), NumberStyles.HexNumber);
        }

        public static string XboxIP(this IXboxConsole console)
        {
            byte[] bytes = BitConverter.GetBytes(console.IPAddress);
            Array.Reverse((Array)bytes);
            return new IPAddress(bytes).ToString();
        }

        public static void XNotify(this IXboxConsole console, string Text)
        {
            console.XNotify(Text, 34U);
        }

        public static void XNotify(this IXboxConsole console, string Text, uint Type)
        {
            string Command = "consolefeatures ver=" + JRPCPlusPlus.jrpcVersion + " type=12 params=\"A\\0\\A\\2\\" + JRPCPlusPlus.String + "/" + Text.Length + "\\" + Text.ToHexString() + "\\" + JRPCPlusPlus.Int + "\\" + Type + "\\\"";
            JRPCPlusPlus.SendCommand(console, Command);
        }

        public enum XboxColor
        {
            Black,
            Blue,
            BlueGray,
            White,
        };

        public static uint LocateXUserAwardAvatarAssets(this IXboxConsole console)
        {
            byte[] targetSequence = { 0x60, 0x84, 0x00, 0x71 };
            byte[] bytes = console.GetMemory(0x82000000, 0x1D00000);

            for (int i = 0; i <= bytes.Length - targetSequence.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < targetSequence.Length; j++)
                {
                    if (bytes[i + j] != targetSequence[j])
                    {
                        match = false;
                        break;
                    }
                }
                uint address = 0x82000000 + (uint)i;
                if (match)
                {
                    uint addr2 = address - 0x8;
                    byte[] testBytes = console.GetMemory(addr2, 4);
                    byte[] target2 = { 0x38, 0xE0, 0x00, 0x08 };
                    if (testBytes.Length == target2.Length && testBytes.SequenceEqual(target2))
                    {
                        return address;
                    }
                }
            }
            return 0;
        }

        public static uint LocateXUserWriteAchievements(this IXboxConsole console)
        {
            byte[] targetSequence = { 0x60, 0x84, 0x00, 0x08 };
            byte[] bytes = console.GetMemory(0x82300000, 0x1bc0000); 

            for (int i = 0; i <= bytes.Length - targetSequence.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < targetSequence.Length; j++)
                {
                    if (bytes[i + j] != targetSequence[j])
                    {
                        match = false;
                        break;
                    }
                }
                uint address = 0x82000000 + (uint)i;
                if (match)
                {
                    uint addr2 = address - 0x8;
                    byte[] testBytes = console.GetMemory(addr2, 4);
                    byte[] target2 = { 0x38, 0xE0, 0x00, 0x08 };
                    if (testBytes.Length == target2.Length && testBytes.SequenceEqual(target2))
                    {
                        return address;
                    }
                }
            }
            throw new Exception("Failed to locate XUserWriteAchievements!");
            //return 0;
        }

        public static uint XamFreeMemory17559(this IXboxConsole console)
        {
            uint xamFreeMem = 0x81AA1D30;

            if (console.ReadUInt32(xamFreeMem) != 0)
            {
                xamFreeMem = 0x81D48680; 
            }
            return xamFreeMem;
        }

        public static void XUserAwardAvatarAsset(this IXboxConsole console, uint avatarAssetCallAddr, uint assetIdPtr, uint xoverlappedPtr, int awardCount)
        {
            console.WriteUInt32(xoverlappedPtr, 0);

            for (int i = 0; i < awardCount; i++)
            {
                console.WriteUInt64(assetIdPtr, (ulong)i);
                console.CallVoid(avatarAssetCallAddr, 1, assetIdPtr, xoverlappedPtr);

                while (console.ReadUInt32(xoverlappedPtr) != 0) Thread.Sleep(10);
            }
            Thread.Sleep(50);
        }

        public static void XUserWriteAchievements(this IXboxConsole console, uint achievementCallAddr, uint achievementIdPtr, uint xoverlappedPtr, int achievementCount)
        {
            console.WriteUInt32(xoverlappedPtr, 0);

            for (int i = 0; i < achievementCount; i++)
            {
                console.WriteUInt64(achievementIdPtr, (ulong)i);
                console.CallVoid(achievementCallAddr, 1, achievementIdPtr, xoverlappedPtr);

                while (console.ReadUInt32(xoverlappedPtr) != 0) Thread.Sleep(5);
            }
            Thread.Sleep(50);
        }
    }
}
