using System.IO;
using System.Windows.Media.Imaging;

namespace VaManager.Extensions;

public static class StreamExtensions
{
    public static byte[] ReadAllBytes(this Stream stream)
    {
        List<byte> bytes = [];
        for (var i = stream.ReadByte(); i != -1; i = stream.ReadByte())
        {
            bytes.Add((byte)i);
        }

        return bytes.ToArray();
    }
}