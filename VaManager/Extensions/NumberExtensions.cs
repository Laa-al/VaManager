namespace VaManager.Extensions;

public class NumberExtensions
{
    public static string GetFileLengthDesc(long length)
    {
        double len = length;
        return length switch
        {
            > 0x40000000 => $"{len / 0x40000000:F4}G",
            > 0x100000 => $"{len / 0x100000:F4}M",
            > 0x400 => $"{len / 0x400:F4}KB",
            _ => $"{length:F4}B"
        };
    }
}