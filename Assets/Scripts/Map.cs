using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// 바닥 타일 정보
[System.Serializable]
public struct MapInfo
{
    // 타일 크기
    public int width;
    public int height;
    // 로드할 프리팹 이름
    public string prefabName;
    // 위치
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public struct Tile
{
    // 로드할 프리팹 이름
    public string prefabName;
    // 위치
    public float x;
    public float y;
    public float z;
}

// 단순 정보만을 갖고있는 클레스
// -> 타일 크기(X, Y)
public class Map : MonoBehaviour
{
    // -> 타일 크기(X, Y)
    public int tileX;
    public int tileY;
    // 바닥타일
    public GameObject floorTile;

    // 바닥위에 그릴 타일(선택된타일 - prefab)
    public GameObject selectedTile;

    // 여러 타일 등록 할 수 있도록 하는 변수
    public List<GameObject> tileList = new List<GameObject>();

    public float tileCreateTime = 0.2f;
    // 그리드를 그릴지 말지 여부
    public bool isDrawGrid = true;

    [MenuItem("MyMenu/PrintHello")]
    static void PrintHello()
    {
        Debug.Log("Hello!!!!");
    }
    [MenuItem("MyMenu/Set ProductName")]
    static void SetProductName()
    {
        PlayerSettings.productName = "205";
    }
    [MenuItem("MyMenu/New Window")]
    static void NewWindnow()
    {
        SampleWindow window = new SampleWindow();
        window.minSize = new Vector2(100, 100);
        window.maxSize = new Vector2(400, 400);
        window.Show();
    }
}
