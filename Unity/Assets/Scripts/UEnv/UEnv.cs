using System;
using System.Buffers.Text;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
using Debug = UnityEngine.Debug;

public class UEnv : MonoBehaviour
{
    private string pythonPath;
    private string scriptPath;

    private Queue<Packet> receivedPackets;
    private Process process;
    private StreamWriter writer;

    private Dictionary<int, List<Action<Packet>>> packetHandlerDic = new Dictionary<int, List<Action<Packet>>>();
    
    public void Run()
    {
        Packet.InitFactory();
        
        receivedPackets = new Queue<Packet>();
        
        var config = UEnvConfig.Load();
        pythonPath = config.PythonPath;
        scriptPath = config.ScriptPath;
            
        RunPython();
    }

    public void RegisterPacketHandler(int key_code, Action<Packet> handler)
    {
        if (!packetHandlerDic.ContainsKey(key_code))
            packetHandlerDic.Add(key_code, new List<Action<Packet>>());
        packetHandlerDic[key_code].Add(handler);
    }

    public void UnregisterPacketHandler(int key_code, Action<Packet> handler)
    {
        packetHandlerDic[key_code].Remove(handler);
    }

    private void OnDestroy()
    {
        if (process != null)
        {
            process.Kill();
            process.Dispose();
        }

        process = null;
    }

    private void Update()
    {
        while (receivedPackets.Count > 0)
        {
            var packet = receivedPackets.Dequeue();
            if (packet == null)
                continue;
            if (packetHandlerDic.TryGetValue(packet.KeyCode, out var handlerList))
                foreach (var eachHandler in handlerList)
                    eachHandler(packet);
        }
    }

    public void SendPacket(Packet packet)
    {
        List<byte> buffer = new List<byte>();
        Packet.AddInt(buffer, packet.KeyCode);
        Packet.AddInt(buffer, packet.Data.Length);
        buffer.AddRange(packet.Data);
        writer.WriteLine(Convert.ToBase64String(buffer.ToArray()));
    }

    async void RunPython()
    {
        
        var task = Task.Run(() => {
            var psi = new ProcessStartInfo()
            {
                FileName = pythonPath,
                Arguments = scriptPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
            };

            psi.WorkingDirectory = Path.GetDirectoryName(scriptPath);

            var erros = string.Empty;
            var results = string.Empty;

            process = Process.Start(psi);
            writer = process.StandardInput;

            using (StreamReader reader = process.StandardOutput)
            {
                List<byte> buffer = new List<byte>();
                bool isReadingPacket = false;
                int packetKey = -1;
                int packetSize = int.MaxValue;
                List<Packet> newPackets = new List<Packet>();
                
                while (process != null && !process.HasExited)
                {
                    var base64string = reader.ReadLine();
                    if (base64string != null)
                    {
                        if (base64string.Length > 0 && base64string[0] == '!')
                        {
                            string log = base64string;
                            int type = 0;
                            if (log.Length >= 2 && log[1] == '!')
                                type = 1;
                            if (log.Length >= 3 && log[2] == '!' && type == 1)
                                type = 2;
                            switch (type)
                            {
                                case 0:
                                    if (base64string.Length >= 2)
                                    {
                                        var bytes = Convert.FromBase64String(base64string[1..]);
                                        Debug.Log(UTF8Encoding.UTF8.GetString(bytes, 0, bytes.Length));
                                    }
                                    break;
                                case 1:
                                    if (base64string.Length >= 3)
                                    {
                                        var bytes = Convert.FromBase64String(base64string[2..]);
                                        Debug.LogWarning(UTF8Encoding.UTF8.GetString(bytes, 0, bytes.Length));
                                    }
                                    break;
                                case 2:
                                    if (base64string.Length >= 4)
                                    {
                                        var bytes = Convert.FromBase64String(base64string[3..]);
                                        Debug.LogError(UTF8Encoding.UTF8.GetString(bytes, 0, bytes.Length));
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            try
                            {
                                byte[] bytes = Convert.FromBase64String(base64string);
                                buffer.AddRange(bytes);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError(e);
                                Debug.LogError(base64string);
                            }

                            if (buffer.Count > 0)
                            {
                                while ((!isReadingPacket && buffer.Count >= 8) || packetSize <= buffer.Count)
                                {
                                    if (!isReadingPacket)
                                    {
                                        isReadingPacket = true;
                                        var packetMetaBytes = buffer.GetRange(0, 8).ToArray();
                                        buffer.RemoveRange(0, 8);
                                        int readIdx = 0;
                                        packetKey = Packet.GetInt(packetMetaBytes, ref readIdx);
                                        packetSize = Packet.GetInt(packetMetaBytes, ref readIdx);
                                    }

                                    if (packetSize <= buffer.Count)
                                    {
                                        var packetBytes = buffer.GetRange(0, packetSize).ToArray();
                                        buffer.RemoveRange(0, packetSize);
                                        newPackets.Add(Packet.GetPacket(packetKey, packetBytes));
                                        isReadingPacket = false;
                                        packetSize = int.MaxValue;
                                    }
                                }

                                if (newPackets.Count > 0)
                                {
                                    lock (receivedPackets)
                                    {
                                        for (int i = 0; i < newPackets.Count; ++i)
                                            receivedPackets.Enqueue(newPackets[i]);
                                        newPackets.Clear();
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return Task.CompletedTask;
        });
        
        await task;
        
        UnityEngine.Debug.Log("End");
    }
}
