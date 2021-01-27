using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

// 수정하려는 Map 변수를 기억하고 있다가 쓰도록 하자
[CustomEditor(typeof(Map))]
public class MapEditor : Editor
{
    Map map;

    // 그리드를 그릴지 말지 여부
    bool isDrawGrid = true;

    private void OnEnable()
    {
        // Map 인스턴스를 할당하기 위해 target 변수를 넣어주자
        // target 은 [CustomEditor(typeof(Map))] 로 부터 정보를 찾는다.
        map = target as Map;
    }

    // Inspector 창을 확장해보자
    public override void OnInspectorGUI()
    {
        map.tileX = EditorGUILayout.IntField("가로 타일 개수", map.tileX);
        map.tileY = EditorGUILayout.IntField("세로 타일 개수", map.tileY);
        // 가로세로 타일의 최소 최대 값을 지정해 주고 싶다.
        map.tileX = Mathf.Clamp(map.tileX, 1, 200);
        map.tileY = Mathf.Clamp(map.tileY, 1, 200);

        // 바닥타일 객체 인스펙터에 표시하자
        var options = new[] { GUILayout.Width(64), GUILayout.Height(64) };

        map.floorTile = (GameObject)EditorGUILayout.ObjectField("바닥 타일", map.floorTile, typeof(GameObject), false);

        // 선택 타일 그리기
        //map.selectedTile = (GameObject)EditorGUILayout.ObjectField("선택타일", map.selectedTile, typeof(GameObject), true);

        // tile 목록 그리기
        DrawTileList();

        // 그리드 그릴지 말지 여부
        isDrawGrid = EditorGUILayout.Toggle("그리드 그릴까요?", isDrawGrid);

        // 인스펙터의 값이 변하면 씬 정보도 갱신하고 싶다.
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }


        GUILayout.Space(15);
        // 사용자가 생성 버튼을 누르면 타일을 생성하고 싶다.
        if(GUILayout.Button("맵 생성"))
        {
            // 만약 바닥타일 프리팹을 등록하지 않으면 오류 팝업을 띄우자
            if(map.floorTile == null)
            {
                //오류 팝업을 띄우자
                if (EditorUtility.DisplayDialog("Error", "Please insert Floor Tile", "OK"))
                {
                    return;
                }
            }
            CreateMap();
        }
    }

    void DrawTileList()
    {
        // 추가할 타일 입력 받기
        GameObject tile = (GameObject)EditorGUILayout.ObjectField("타일 추가", null, typeof(GameObject), false);
        // 사용자가 타일을 추가했으면 목록에 추가하자
        if(tile)
        {
            map.tileList.Add(tile);
        }

        // 타일 목록 그리기
        for(int i=0;i<map.tileList.Count;i++)
        {
            // 현재 인덱스 번째 타일의 이름을 지정
            GUI.SetNextControlName("tile" + i);
            map.tileList[i] = (GameObject)EditorGUILayout.ObjectField("타일 " + i, map.tileList[i], typeof(GameObject), false);
            if(map.tileList[i] == null)
            {
                map.tileList.RemoveAt(i);
                i--;
            }
        }

        // 현재 선택 녀석의 컨트롤 이름 가져오기
        string name = GUI.GetNameOfFocusedControl();
        //Debug.Log("현재 선택된 타일 이름 : " + name);
        // 만약 타일이 선택되면
        if (name.Contains("tile"))
        {
            // 해당 타일의 프리팹을 가져와서 SelectedTile 에 넣어주자
            int idx = int.Parse(name.Substring(4).Trim());

            map.selectedTile = map.tileList[idx];
        }

    }

    // Scene 뷰에 바닥타일을 이용하여 타일크기 만큼 그리자
    void CreateMap()
    {
        // 만약 씬에 Tiles 게임오브젝트가 없으면 만들고 바닥 타일을 자식으로 추가하자
        if(GameObject.Find("Tiles"))
        {
            // 있으면 없애주자
            DestroyImmediate(GameObject.Find("Tiles"));
        }

        GameObject tiles = new GameObject("Tiles");
        tiles.transform.position = Vector3.zero;

        // 시작위치
        Vector3 center = Vector3.zero;
        // 1. 바닥타일 생성
        GameObject floor = (GameObject)PrefabUtility.InstantiatePrefab(map.floorTile);

        floor.name = "Floor";

        // 2. 바닥타일 크기를 tileX, tileY 크기로 설정
        floor.transform.localScale = new Vector3(map.tileX, 1, map.tileY);
        // 3. 위치지정
        floor.transform.position = center + Vector3.right * map.tileX * 0.5f + Vector3.forward * map.tileY * 0.5f;

        floor.transform.parent = tiles.transform;
    }



    // 씬 창을 확장하는 함수
    private void OnSceneGUI()
    {
        // 씬에서 오브젝트를 클릭했을 때 다른 객체가 선택되지 않게 하고 싶다.
        // 현재 (Focus)선택된 녀석의 ID 계속 기억 되도록 하고 싶다.
        int id = GUIUtility.GetControlID(FocusType.Passive);
        // 씬에 해당 id 녀석이 계속 선택되어 있도록 처리
        HandleUtility.AddDefaultControl(id);

        // 타일 기반의 맵을 그리고 싶다. 그리드
        DrawGrid();

        // 만약 선택된 타일이 있다면
        if(map.selectedTile)
        {
            DrawTile();
        }

        // 선택된 타일 프리뷰 이미지 그려보자
        DrawSelectedTilePreviewOnScene();
    }

    void DrawSelectedTilePreviewOnScene()
    {
        // 1. 프리뷰이미지 가져오기
        Texture2D preview = AssetPreview.GetAssetPreview(map.selectedTile);
        if(preview == null)
        {
            preview = AssetPreview.GetMiniThumbnail(map.selectedTile);
        }
        // 2. 씬에 이미지 그리기
        if(preview)
        {
            Handles.BeginGUI();
            GUI.Button(new Rect(10, 10, 100, 100), preview);
            Handles.EndGUI();
        }

        
    }

    // 1. 사용자가 타일을 그릴 수 있도록 하자
    // 2. 마우스를 누르고 있으면 그려지게 하고 싶다.
    // 필요속성 : 누르고 있는 중인지 상태 기억 변수
    bool isMouseDownState = false;
    // 3. shift 키를 눌렀을 때는 타일을 지우게 하고싶다.
    // 4. 바닥을 터치하면 타일을 해당 위치에 그리고
    //    타일을 터치하면 타일 위에 타일을 새로 위치시키자
    void DrawTile()
    {
        // 사용자가 마우스를 클릭하면 그려지게 하고 싶다.
        Event e = Event.current;


        // Alt 키를 눌렀을 때는 종료하자
        if(e.alt)
        {
            return;
        }

        if(e.type == EventType.MouseDown)
        {
            isMouseDownState = true;
        }
        else if (e.type == EventType.MouseUp)
        {
            isMouseDownState = false;
        }

        if(isMouseDownState == false)
        {
            return;
        }

        // Ray 를 쏴서 닿은 그 지점에 타일을 만들어서 위치 시키자
        // 1. Ray 가 필요
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        // 2. RaycastHit 필요
        RaycastHit hitInfo = new RaycastHit();
        // 3. Ray 를 쏜다.
        if (Physics.Raycast(ray, out hitInfo))
        {
            bool isShift = CheckShiftInput(e, hitInfo);
            if(isShift)
            {
                return;
            }

            MakeAndLocateTile(hitInfo);
            
        }
    }

    private bool CheckShiftInput(Event e, RaycastHit hitInfo)
    {
        // 만약 shift 키를 눌렀을 때 부딪힌 녀석이 바닥이면 지우지 않는다.
        if (e.shift && hitInfo.transform.name == "Floor")
        {
            return true;
        }
        //// 만약 shift 키 누르고 부딪힌 녀석이 Tile 이면 지운다.
        //if (e.shift && hitInfo.transform.name == "Tile")
        //{
        //    DestroyImmediate(hitInfo.transform.gameObject);
        //    return true;
        //}

        return false;
    }

    private void MakeAndLocateTile(RaycastHit hitInfo)
    {
        // 4. 닿은 지점에 타일 만들어서 위치 시킨다.
        
        // 부딪힌 녀석이 바닥이면 그냥 그지점에 만들고
        if(hitInfo.transform.name == "Floor")
        {
            GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(map.selectedTile);
            tile.name = "Tile";

            // 타일 배치
            // index 찾아오자
            int x = Mathf.FloorToInt(hitInfo.point.x % map.tileX);
            int z = Mathf.FloorToInt(hitInfo.point.z % map.tileY);
            tile.transform.position = new Vector3(x + 0.5f, 0.5f, z + 0.5f);
            // Tiles 자식으로 등록하자
            tile.transform.parent = GameObject.Find("Tiles").transform;
        }
        // 그렇지 않고 타일이면 타일 위쪽에 만들자
        else
        {
            RaycastHit hit;
            while (true)
            {
                Ray ray = new Ray(hitInfo.transform.position, Vector3.up);
                if(Physics.Raycast(ray, out hit))
                {
                    hitInfo = hit;
                }
                else
                {
                    break;
                }
            }
            // 만약 shift 키를 누르면 타일 제거하고
            if(Event.current.shift)
            {
                DestroyImmediate(hitInfo.transform.gameObject);
                return;
            }
            // 그렇지 않으면 만들자

            GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(map.selectedTile);
            tile.name = "Tile";
            // 배치
            tile.transform.position = hitInfo.transform.position + Vector3.up;
            // Tiles 자식으로 등록하자
            tile.transform.parent = GameObject.Find("Tiles").transform;
        }
        
        
    }

    private void DrawGrid()
    {
        if(isDrawGrid == false)
        {
            return;
        }
        
        // 맵을 화면의 어디에서 부터 그릴지 위치 설정
        Vector3 center = Vector3.zero;
        // 그리드의 색상 지정
        Handles.color = Color.green;

        // map 의 Y 가 Z 축 방향을 가리킨다.
        for(int i=0;i <= map.tileY;i++)
        {
            Vector3 p1 = center + Vector3.forward * i;

            Vector3 p2 = center + Vector3.forward * i + Vector3.right * map.tileX;

            Handles.DrawLine(p1, p2);
        }

        for (int j = 0; j <= map.tileX; j++)
        {
            Vector3 p1 = center + Vector3.right * j;

            Vector3 p2 = center + Vector3.right * j + Vector3.forward * map.tileY;

            Handles.DrawLine(p1, p2);
        }
    }
}
