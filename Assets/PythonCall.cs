using UnityEngine;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

public class PythonCall : MonoBehaviour
{
    Process process;
    StreamWriter writer;
    int i = 0;

    // Start is called before the first frame update
    void Start()
    {
        RunPython();
    }

    private void OnDestroy()
    {
        process.Kill();
    }

    private void Update()
    {
        writer.Flush();
        writer.WriteLine($"good {i++}");
    }

    async void RunPython()
    {
        var pythonPath = @"C:\Users\Jun\AppData\Local\Programs\Python\Python310\python.exe"; 
        var scriptPath = @"C:\Users\Jun\Desktop\Projects\ProjectAl\UEnv\Assets\Python\test2.py";

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

            var erros = string.Empty;
            var results = string.Empty;

            process = Process.Start(psi);
            writer = process.StandardInput;

            using (StreamReader reader = process.StandardOutput)
            {
                while (!process.HasExited)
                {
                    UnityEngine.Debug.Log("===");
                    byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(reader.ReadLine());
                    for (int i = 0; i < bytes.Length; ++i)
                        UnityEngine.Debug.Log(bytes[i]);
                }
            }

            return Task.CompletedTask;
        });

        UnityEngine.Debug.Log("End");

        await task;
    }
}
