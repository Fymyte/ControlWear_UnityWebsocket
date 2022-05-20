using System.Collections;
using System.Collections.Generic;
using ControlWear;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Connection connection = new Connection();
        connection.StartSever("54321");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
