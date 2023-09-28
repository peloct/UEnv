using System;
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
        env.RegisterPacketHandler(P2UTest.KEY, packet =>
        {
            var p2uTest = packet as P2UTest;
            Debug.Log(p2uTest.text);
        });
        
        env.Run();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            env.SendPacket(new U2PTest("U2PTest"));
        }
    }
}
