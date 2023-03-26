// See https://aka.ms/new-console-template for more information

using System.Runtime.InteropServices;
using System.Text;
using YamlDotNet.Serialization;

Console.WriteLine("Hello, World!");
Console.WriteLine();

Package package = new(0x05, 14, 0b0000_0101, 54325, 784, 0);

byte[] bytes = package.ToBytes();
bytes[^1] = CheckSumCalc(ref bytes, bytes.Length - 1);

Console.WriteLine("// " + package);
Console.WriteLine($"// Количество байт: {bytes.Length}");
string str = string.Join(" ", bytes.Select(b => $"0x{b:X2}")); 
Console.WriteLine("// Посылка: " + str);
Console.WriteLine();

foreach (byte b in bytes)
{
    Console.WriteLine(GetStringStim(b));
}

ISerializer serializer = new SerializerBuilder().Build();
string yaml = serializer.Serialize(package);
Console.WriteLine(yaml);


Console.ReadLine();

const string setBit = "PIND |= 0x01";
const string clrBit = "PIND &= 0xFE";
const string delay = "#417";

static string GetStringStim(byte data)
{
    StringBuilder sb = new();

    string header = $"// Формирование байта 0x{data:X2} ";
    sb.Append(header + new string('-', 80 - header.Length) + Environment.NewLine);
    sb.Append(Environment.NewLine);
    
    sb.Append($"// Старт бит" + Environment.NewLine);
    sb.Append(clrBit + Environment.NewLine);
    sb.Append(delay + Environment.NewLine);
    sb.Append(Environment.NewLine);
    
    sb.Append($"// Данные" + Environment.NewLine);
    for (int i = 0; i < 8; i++)
    {
        string str = (data & (1 << i)) switch
        {
            0 => clrBit,
            _ => setBit
        };
        sb.Append(str + Environment.NewLine);
        sb.Append(delay + Environment.NewLine);
    }
    sb.Append(Environment.NewLine);
    
    sb.Append($"// Стоп бит" + Environment.NewLine);
    sb.Append(setBit + Environment.NewLine);
    sb.Append(delay + Environment.NewLine);
    sb.Append(Environment.NewLine);
    
    sb.Append($"// Задержка после байта" + Environment.NewLine);
    sb.Append(delay + Environment.NewLine);

    return sb.ToString();
}

static byte CheckSumCalc(ref byte[] array, int size)
{
    int sum = 0;
    for (int i = 0; i < size; i++)
    {
        sum += array[i];
    }

    return (byte)sum;
}

public interface IBytes { }

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal record struct Package(
    byte Header,
    byte Counter,
    byte Flags,
    UInt16 Voltage,
    UInt16 Current,
    byte CheckSum) : IBytes;

public static class Ext
{
    public static byte[] ToBytes<T>(this T container) where T : IBytes
    {   
        byte[] bytes = new byte[Marshal.SizeOf(typeof(T))];  
        GCHandle pinStructure = GCHandle.Alloc(container, GCHandleType.Pinned);  
        try 
        {  
            Marshal.Copy(pinStructure.AddrOfPinnedObject(), bytes, 0, bytes.Length);  
            return bytes;  
        }  
        finally 
        {  
            pinStructure.Free();  
        }
    }
}
