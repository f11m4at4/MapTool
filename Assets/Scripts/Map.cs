using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
