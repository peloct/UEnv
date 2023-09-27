using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UEnv))]
public class Main : MonoBehaviour
{
    private UEnv env;
    
    void Start()
    {
        env = GetComponent<UEnv>();
        env.RegisterPacketHandler(AddButton.KEY, packet =>
        {
            var addButton = packet as AddButton;
            Debug.Log(addButton.id);
        });
        
        env.Run();
    }
}
