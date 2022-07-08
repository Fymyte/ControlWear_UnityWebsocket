using System;
using System.Collections;
using System.Collections.Generic;
using Ohrizon.ControlWear.Gesture;
using Ohrizon.ControlWear.Network;
using UnityEngine;
using UnityEngine.Serialization;

public class MenuManager : MonoBehaviour
{
    public List<GameObject> menuItems;
    public int nbShowedRow;
    public int nbShowedColumn;
    public int nbRow;
    public int nbColumn;
    public GameObject menuItemPrefab;
    public float lerpTime = .1f;
    public Vector3 unFocusedScale;
    public Vector3 focusedScale;
    public float distanceBetweenItems;
    
    private const string SOCKET_PORT = "54321";
    private GestureManager _gestureManager;
    private IListener _messageListener;

    private int _row;
    private int _col;
    private float startX;
    private float startY;


    // Start is called before the first frame update
    void Start()
    {
        _row = 0;
        _col = 0;

        var offsetX = (nbShowedColumn - 1f) / 2f;
        var offsetY = (nbShowedRow - 1f) / 2f;

        startX = -offsetX;
        startY = offsetY;

        InitializeMenuItems();

        // _messageListener = new TcpListener(SOCKET_PORT);
        _messageListener = new BluetoothListener();
        _messageListener.ClientConnected += client => Debug.Log($"Client {client} connected");
        _messageListener.ClientDisconnected += client =>
        {
            Debug.Log($"Client {client} disconnected");
            _messageListener.Listen();
        };
        _messageListener.MessageReceived += message => _gestureManager.Recognize(message);

        _gestureManager = new GestureManager();
        _gestureManager.SingleTap += OnSingleTap;// () => Debug.Log($"[{DateTime.Now.}] Single Tap");
        _gestureManager.DoubleTap += () => Debug.Log("Double Tap");
        _gestureManager.HorizontalLeft += OnLeft;
        _gestureManager.HorizontalRight += OnRight;
        _gestureManager.VerticalDown += OnDown;
        _gestureManager.VerticalUp += OnUp;

        _messageListener.Listen();
    }

    private void OnLeft()
    {
        Debug.Log("TRACE - OnLeft");
        if (_col >= nbColumn - 1) return;
        
        UnFocusItem(_row, _col);
        FocusItem(_row, _col + 1);
        // foreach (var item in menuItems)
        // {
        //     StartCoroutine(MoveSLerp(item.transform, Vector3.left));
        // }
        _col += 1;
    }
    private void OnRight()
    {
        Debug.Log("TRACE - OnRight");
        if (_col <= 0) return;
        
        // UnFocusCurrentItem();
        UnFocusItem(_row, _col);
        FocusItem(_row, _col - 1);
        // foreach (var item in menuItems)
        // {   
        //     StartCoroutine(MoveSLerp(item.transform, Vector3.right));
        // }

        _col -= 1;
    }
    private void OnDown()
    {
        Debug.Log("TRACE - OnDown");
        if (_row <= 0) return;
        
        UnFocusItem(_row, _col);
        FocusItem(_row - 1, _col);
        // foreach (var item in menuItems)
        // {
        //     StartCoroutine(MoveSLerp(item.transform, Vector3.down));
        // }

        _row -= 1;
    }
    private void OnUp()
    {
        Debug.Log("TRACE - OnUp");
        if (_row >= nbRow - 1) return;
        UnFocusItem(_row, _col);
        FocusItem(_row + 1, _col);

        // foreach (var item in menuItems)
        // {
        //     StartCoroutine(MoveSLerp(item.transform, Vector3.up));
        // }

        _row += 1;
    }

    private GameObject GetItem(int row, int col)
    {
        return menuItems[col * nbRow + row];
    }

    private void FocusItem(int row, int col)
    {
        GameObject item =  GetItem(row, col);
        item.gameObject.GetComponent<Renderer>().material.color = Color.red;
        StartCoroutine(ScaleSLerp(item.transform, focusedScale));
    }
    private void UnFocusItem(int row, int col)
    {
        GameObject item = GetItem(row, col);
        item.gameObject.GetComponent<Renderer>().material.color = Color.white;
        StartCoroutine(ScaleSLerp(item.transform, unFocusedScale));
    }
    private IEnumerator MoveSLerp(Transform item, Vector3 movement)
    {
        float t = 0;
        Vector3 initialPos = item.position;
        Vector3 finalPos = initialPos + (movement * distanceBetweenItems);
        while (t < 1)
        {
            t += Time.deltaTime / lerpTime;
            item.position = Vector3.Slerp(initialPos, finalPos, t);
            yield return null;
        }
    }

    private IEnumerator ScaleSLerp(Transform item, Vector3 movement)
    {
        
        float t = 0;
        Vector3 initialPos = item.localScale;
        Vector3 finalPos = movement;
        while (t < 1)
        {
            t += Time.deltaTime / lerpTime;
            item.localScale = Vector3.Slerp(initialPos, finalPos, t);
            yield return null;
        }
    }

    private void PrintRowCol()
    {
        Debug.Log($"position: [{_row}:{_col}]");
    }

    private void InitializeMenuItems()
    {
        menuItems = new List<GameObject>();
        for (float i = 0; i < nbColumn; ++i)
        {
            for (float j = 0; j < nbRow; ++j)
            {
                // Debug.Log($"Item[{i}][{j}]: {(i + startX) * distanceBetweenItems}, {(-j + startY) * distanceBetweenItems}");
                GameObject newItem;
                try
                {
                    newItem = Instantiate(menuItemPrefab,
                        new Vector3((i + startX) * distanceBetweenItems, (-j + startY) * distanceBetweenItems, 0),
                        Quaternion.identity, transform);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to instantiate: {e.Message}");
                    return;
                }

                ;
                newItem.transform.localScale = unFocusedScale;
                menuItems.Add(newItem);
            }
        }
        
        // GetItem(_row, _col).transform.Translate(0, 0, -.5f);
        FocusItem(_row, _col);
        PrintRowCol();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnSingleTap()
    {
        DateTime now = DateTime.Now;
        Debug.Log($"{now.Hour}:{now.Minute}:{now.Second}:{now.Millisecond} Single tap");
    }
}
