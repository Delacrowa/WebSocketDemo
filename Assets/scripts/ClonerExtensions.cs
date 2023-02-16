using System.IO;

public static class ClonerExtensions
{
    public static TObject Clone<TObject>(this TObject toClone)
    {
        var formatter = BinarySerializer.Formatter;

        using (var memoryStream = new MemoryStream())
        {
            formatter.Serialize(memoryStream, toClone);

            memoryStream.Position = 0;

            return (TObject) formatter.Deserialize(memoryStream);
        }
    }
}