using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "UEnvConfig", menuName = "Create UEnvConfig", order = 1)]
public class UEnvConfig : ScriptableObject
{
    [SerializeField] string pythonPath;
    [SerializeField] string scriptPath;
    public TextAsset packetDef;
    [SerializeField] string pyPacketFactoryPath;
    public TextAsset csPacketFactory;

    public string PyPacketFactoryPath => pyPacketFactoryPath.Replace("<project>", Application.dataPath);
    public string PythonPath => pythonPath.Replace("<project>", Application.dataPath);
    public string ScriptPath => scriptPath.Replace("<project>", Application.dataPath);
    
    public static UEnvConfig Load()
    {
        var assets = AssetDatabase.FindAssets("UEnvConfig t:UEnvConfig");
        if (assets.Length == 0)
        {
            Debug.LogError("Cannot find UEnvConfig");
            return null;
        }
        
        if (assets.Length >= 2)
        {
            Debug.LogError("Too many UEnvConfig");
            return null;
        }

        var assetPath = AssetDatabase.GUIDToAssetPath(assets[0]);
        var config = AssetDatabase.LoadAssetAtPath<UEnvConfig>(assetPath);
        
        return config;
    }
}