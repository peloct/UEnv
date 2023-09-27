using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Xml;
using System.Text;
using System.IO;


public class AutoPacketGen
{
    [MenuItem("UEnv/Generate Packet")]
    public static void GeneratePacket()
    {
        Dictionary<string, string> fieldTypeToCSDataType = new Dictionary<string, string>()
        {
            {"bool", "bool"},
            {"int", "int"},
            {"float", "float"},
            {"string", "string"},
        };
        
        Dictionary<string, string> fieldTypeToPYDataType = new Dictionary<string, string>()
        {
            {"bool", "bool"},
            {"int", "int"},
            {"float", "float"},
            {"string", "str"},
        };
        
        Dictionary<string, string> fieldTypeToPYDataParser = new Dictionary<string, string>()
        {
            {"bool", "read_bool"},
            {"int", "read_int"},
            {"float", "read_float"},
            {"string", "read_str"},
        };
        
        Dictionary<string, string> fieldTypeToCSDataParser = new Dictionary<string, string>()
        {
            {"bool", "GetBool"},
            {"int", "GetInt"},
            {"float", "GetFloat"},
            {"double", "GetDouble"},
            {"string", "GetString"},
        };
        
        Dictionary<string, string> fieldTypeToCSDataWriter = new Dictionary<string, string>()
        {
            {"bool", "AddBool"},
            {"int", "AddInt"},
            {"float", "AddFloat"},
            {"double", "AddDouble"},
            {"string", "AddString"},
        };
        
        string FindAttributeValue(XmlAttributeCollection attributeCollection, string name)
        {
            for (int i = 0; i < attributeCollection.Count; ++i)
                if (attributeCollection[i].Name == name)
                    return attributeCollection[i].Value;
            return null;
        }
        
        var config = UEnvConfig.Load();
        var packetDef = config.packetDef.text;
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(packetDef);
        
        var packets = doc.DocumentElement;
        var childNodes = packets.ChildNodes;
        
        var path = Application.dataPath;
        
        var csPacketFactoryFullpath = path.Replace("Assets", AssetDatabase.GetAssetPath(config.csPacketFactory));
        var pyPacketFactoryFullpath = config.pyPacketFactoryPath;
        
        StringBuilder pyPacketFactory = new StringBuilder();
        StringBuilder pyPacketFactoryInit = new StringBuilder();
        StringBuilder csPacketFactory = new StringBuilder();
        StringBuilder csPacketFactoryInit = new StringBuilder();

        csPacketFactory.AppendLine("using System.Collections.Generic;");
        pyPacketFactory.AppendLine("from packet import *");

        int nextPacketKeyCode = 0;
        
        for (int i = 0; i < childNodes.Count; ++i)
        {
            if (childNodes[i].Name != "packet")
                continue;

            var packetKeyCode = nextPacketKeyCode++;
            
            var eachAttributes = childNodes[i].Attributes;
            Dictionary<string, string> attributes = new Dictionary<string, string>();
            for (int j = 0; j < eachAttributes.Count; ++j)
                attributes.Add(eachAttributes[j].Name, eachAttributes[j].Value);

            List<string> fieldNames = new List<string>();
            Dictionary<string, string> fields = new Dictionary<string, string>();
            var eachFields = childNodes[i].ChildNodes;
            for (int j = 0; j < eachFields.Count; ++j)
            {
                if (eachFields[j].Name != "field")
                    continue;

                var att = eachFields[j].Attributes;
                var fieldName = FindAttributeValue(att, "name");
                var fieldType = FindAttributeValue(att, "type");
                fields.Add(fieldName, fieldType);
                fieldNames.Add(fieldName);
            }
            
            var packetName = attributes["name"];
            var type = attributes["type"];
            if (type == "p2u")
            {
                string pyConstInput;
                string dataInput;
                
                csPacketFactoryInit.AppendLine($"        AddGenerator({packetKeyCode}, (keyCode, data) => new {packetName}(keyCode, data));");

                if (fieldNames.Count > 0)
                {
                    List<string> fieldNamesWithPYType = new List<string>();
                    for (int j = 0; j < fieldNames.Count; ++j)
                    {
                        var pyType = fieldTypeToPYDataType[fields[fieldNames[j]]];
                        fieldNamesWithPYType.Add($"{fieldNames[j]}:{pyType}");
                    }
                    
                    pyConstInput = "self, " + string.Join(", ", fieldNamesWithPYType);
                    dataInput = $"pack_data({string.Join(", ", fieldNames)})";
                }
                else
                {
                    pyConstInput = "self";
                    dataInput = "bytes(0)";
                }

                StringBuilder csFieldDecl = new StringBuilder();
                StringBuilder csFieldParse = new StringBuilder();
                
                csFieldParse.AppendLine($"        int ___reader = 0;");
                csFieldDecl.AppendLine($"    public const int KEY = {packetKeyCode};");
                for (int j = 0; j < fieldNames.Count; ++j)
                {
                    var fieldType = fields[fieldNames[j]];
                    var csType = fieldTypeToCSDataType[fieldType];
                    var parser = fieldTypeToCSDataParser[fieldType];
                    
                    csFieldDecl.AppendLine($"    public {csType} {fieldNames[j]};");
                    csFieldParse.AppendLine($"        {fieldNames[j]} = {parser}(Data, ref ___reader);");
                }
                
                pyPacketFactory.AppendLine(
$@"
class {packetName}(Packet):
    def __init__({pyConstInput}):
        super().__init__({packetKeyCode}, {dataInput})
");
                
                csPacketFactory.AppendLine($"public class {packetName} : Packet");
                csPacketFactory.AppendLine("{");
                csPacketFactory.Append(csFieldDecl.ToString());
                csPacketFactory.AppendLine(
$@"    public {packetName}(int keyCode, byte[] data) : base(keyCode, data)");
                csPacketFactory.AppendLine("    {");
                csPacketFactory.Append(csFieldParse.ToString());
                csPacketFactory.AppendLine("    }");
                csPacketFactory.AppendLine("}");
            }
            else if(type == "u2p")
            {
                string csConstInput;

                pyPacketFactoryInit.AppendLine($"    PacketFactory.add_generator({packetKeyCode}, {packetName})");
                
                if (fieldNames.Count > 0)
                {
                    List<string> fieldNamesWithPYType = new List<string>();
                    for (int j = 0; j < fieldNames.Count; ++j)
                    {
                        var csType = fieldTypeToCSDataType[fields[fieldNames[j]]];
                        fieldNamesWithPYType.Add($"{csType} {fieldNames[j]}");
                    }
                    
                    csConstInput = string.Join(", ", fieldNamesWithPYType);
                }
                else
                {
                    csConstInput = "";
                }

                pyPacketFactory.AppendLine($"class {packetName}(Packet):");
                pyPacketFactory.AppendLine($"    KEY = {packetKeyCode}");
                pyPacketFactory.AppendLine("    def __init__(self, key_code, data):");
                pyPacketFactory.AppendLine("        super().__init__(key_code, data)");
                pyPacketFactory.AppendLine("        data = self.data");
                
                for (int j = 0; j < fieldNames.Count; ++j)
                {
                    var fieldType = fields[fieldNames[j]];
                    var parser = fieldTypeToPYDataParser[fieldType];
                    pyPacketFactory.AppendLine($"        self.{fieldNames[j]}, data = {parser}(data)");
                }

                pyPacketFactory.AppendLine();
                
                StringBuilder csFieldWrite = new StringBuilder();

                csFieldWrite.AppendLine("        List<byte> ___buffer = new List<byte>();");
                for (int j = 0; j < fieldNames.Count; ++j)
                {
                    var fieldType = fields[fieldNames[j]];
                    var writer = fieldTypeToCSDataWriter[fieldType];
                    csFieldWrite.AppendLine($"        {writer}(___buffer, {fieldNames[j]});");
                }
                csFieldWrite.AppendLine("        Data = ___buffer.ToArray();");
                
                csPacketFactory.AppendLine($"public class {packetName} : Packet");
                csPacketFactory.AppendLine("{");
                csPacketFactory.AppendLine($"    public {packetName}({csConstInput}) : base({packetKeyCode})");
                csPacketFactory.AppendLine("    {");
                csPacketFactory.Append(csFieldWrite.ToString());
                csPacketFactory.AppendLine("    }");
                csPacketFactory.AppendLine("}");
            }
        }
        
        csPacketFactory.AppendLine(
@"public partial class Packet
{
    public static void InitFactory()
    {
"
+
$"{csPacketFactoryInit.ToString()}"
+
@"    }
}");
        pyPacketFactory.AppendLine("def init_factory():");
        pyPacketFactory.Append(pyPacketFactoryInit.ToString());
        
        using (StreamWriter writer = new StreamWriter(pyPacketFactoryFullpath))
            writer.WriteLine(pyPacketFactory.ToString());
        using (StreamWriter writer = new StreamWriter(csPacketFactoryFullpath))
            writer.WriteLine(csPacketFactory.ToString());
    }
}