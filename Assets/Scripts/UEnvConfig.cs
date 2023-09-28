using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "UEnvConfig", menuName = "Create UEnvConfig", order = 1)]
public class UEnvConfig : ScriptableObject
{
    public string pythonPath;
    public string scriptPath;
    public TextAsset packetDef;
    public string pyPacketFactoryPath;
    public TextAsset csPacketFactory;
    
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