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
            Vector2 a = new Vector2(p2uTest.ndarray_data.GetValue(0, 0), p2uTest.ndarray_data.GetValue(0, 1));
            Vector2 b = new Vector2(p2uTest.ndarray_data.GetValue(1, 0), p2uTest.ndarray_data.GetValue(1, 1));
            Debug.Log($"ndarray_data : {a}, {b}");
            Debug.Log(p2uTest.float_data);
            Debug.Log(p2uTest.int_data);
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
