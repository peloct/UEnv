using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class Packet
{
    /*
    총 패킷 byte 사이즈
    최초의 바이트 코드를 통해 패킷 종류 분석
     */

    protected void AddBool(List<byte> buffer, bool value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    protected void AddFloat(List<byte> buffer, float value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    protected void AddDouble(List<byte> buffer, double value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    protected void AddInt(List<byte> buffer, int value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    protected void AddString(List<byte> buffer, string value)
    {
        byte[] bytes = UTF8Encoding.UTF8.GetBytes(value);
        AddInt(buffer, bytes.Length);
        buffer.AddRange(bytes);
    }

    protected bool GetBool(byte[] buffer, ref int idx)
    {
        var value = BitConverter.ToBoolean(buffer, idx);
        idx += 1;
        return value;
    }

    protected float GetFloat(byte[] buffer, ref int idx)
    {
        var value = BitConverter.ToSingle(buffer, idx);
        idx += 4;
        return value;
    }

    protected double GetDouble(byte[] buffer, ref int idx)
    {
        var value = BitConverter.ToDouble(buffer, idx);
        idx += 8;
        return value;
    }

    protected int GetInt(byte[] buffer, ref int idx)
    {
        var value = BitConverter.ToInt32(buffer, idx);
        idx += 4;
        return value;
    }

    protected string GetString(byte[] buffer, ref int idx)
    {
        int byteCount = GetInt(buffer, ref idx);
        var value = UTF8Encoding.UTF8.GetString(buffer, idx, byteCount);
        idx += byteCount;
        return value;
    }
}