using System.Collections.Generic;
public class P2UTest : Packet
{
    public const int KEY = 0;
    public string text;
    public P2UTest(int keyCode, byte[] data) : base(keyCode, data)
    {
        int ___reader = 0;
        text = GetString(Data, ref ___reader);
    }
}
public class U2PTest : Packet
{
    public U2PTest(string text) : base(1)
    {
        List<byte> ___buffer = new List<byte>();
        AddString(___buffer, text);
        Data = ___buffer.ToArray();
    }
}
public partial class Packet
{
    public static void InitFactory()
    {
        AddGenerator(0, (keyCode, data) => new P2UTest(keyCode, data));
    }
}

