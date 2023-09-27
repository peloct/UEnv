using System.Collections.Generic;
public class AddButton : Packet
{
    public const int KEY = 0;
    public string id;
    public AddButton(int keyCode, byte[] data) : base(keyCode, data)
    {
        int ___reader = 0;
        id = GetString(Data, ref ___reader);
    }
}
public class UIEvent : Packet
{
    public UIEvent(string clicked_button, int key) : base(1)
    {
        List<byte> ___buffer = new List<byte>();
        AddString(___buffer, clicked_button);
        AddInt(___buffer, key);
        Data = ___buffer.ToArray();
    }
}
public partial class Packet
{
    public static void InitFactory()
    {
        AddGenerator(0, (keyCode, data) => new AddButton(keyCode, data));
    }
}

