using System.Collections;
using System.Collections.Generic;
using ControlWear;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        new ConnectionTcpListener("54123").Listen();
        new ConnectionBluetoothListener().Listen();
    }

}
