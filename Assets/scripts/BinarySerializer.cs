using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class BinarySerializer
{
    public static readonly BinaryFormatter Formatter = new();

    public static object Deserialize(byte[] serialized)
    {
        using (var stream = new MemoryStream(serialized))
        {
            var result = Formatter.Deserialize(stream);
            return result;
        }
    }

    public static byte[] Serialize(object toSerialize)
    {
        using (var stream = new MemoryStream())
        {
            Formatter.Serialize(stream, toSerialize);
            return stream.ToArray();
        }
    }
}