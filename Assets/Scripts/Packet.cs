﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public partial class Packet
{
    private static Dictionary<int, Func<int, byte[], Packet>> generators = new Dictionary<int, Func<int, byte[], Packet>>();
    
    public static void AddGenerator(int key_code, Func<int, byte[], Packet> gen)
    {
        generators.Add(key_code, gen);
    }

    public static Packet GetPacket(int keyCode, byte[] data)
    {
        if (!generators.ContainsKey(keyCode))
            return new Packet(keyCode, data);
        return generators[keyCode](keyCode, data);
    }
    
    public readonly int KeyCode;
    public byte[] Data { get; protected set; }

    protected Packet(int keyCode)
    {
        KeyCode = keyCode;
    }
    
    protected Packet(int keyCode, byte[] data)
    {
        KeyCode = keyCode;
        Data = data;
    }

    public static void AddBool(List<byte> buffer, bool value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public static void AddFloat(List<byte> buffer, float value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public static void AddDouble(List<byte> buffer, double value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public static void AddInt(List<byte> buffer, int value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public static void AddString(List<byte> buffer, string value)
    {
        byte[] bytes = UTF8Encoding.UTF8.GetBytes(value);
        AddInt(buffer, bytes.Length);
        buffer.AddRange(bytes);
    }

    public static bool GetBool(byte[] buffer, ref int idx)
    {
        var value = BitConverter.ToBoolean(buffer, idx);
        idx += 1;
        return value;
    }

    public static float GetFloat(byte[] buffer, ref int idx)
    {
        var value = BitConverter.ToSingle(buffer, idx);
        idx += 4;
        return value;
    }

    public static double GetDouble(byte[] buffer, ref int idx)
    {
        var value = BitConverter.ToDouble(buffer, idx);
        idx += 8;
        return value;
    }

    public static int GetInt(byte[] buffer, ref int idx)
    {
        var value = BitConverter.ToInt32(buffer, idx);
        idx += 4;
        return value;
    }

    public static string GetString(byte[] buffer, ref int idx)
    {
        int byteCount = GetInt(buffer, ref idx);
        var value = UTF8Encoding.UTF8.GetString(buffer, idx, byteCount);
        idx += byteCount;
        return value;
    }
}