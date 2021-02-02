using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

// 수정하려는 Map 변수를 기억하고 있다가 쓰도록 하자
[CustomEditor(typeof(Map))]
public class MapEditor : Editor
{
    Map map;

    

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

        // 타일 생성 간격
        map.tileCreateTime = EditorGUILayout.FloatField("타일 생성 간격", map.tileCreateTime);

        // 그리드 그릴지 말지 여부
        map.isDrawGrid = EditorGUILayout.Toggle("그리드 그릴까요?", map.isDrawGrid);

        // 인스펙터의 값이 변하면 씬 정보도 갱신하고 싶다.
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
            
        }

        // 속성 저장 되도록 해보자
        //SerializedProperty tileX

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

        // 저장 버튼 그리기
        GUILayout.Space(5);
        if(GUILayout.Button("맵 저장"))
        {
            string message = SaveMap();
            // success 가 true 면 저장 성공, 그렇지 않으면 저장 실패 메시지 띄우고 싶다.
            // 저장 성공여부 다이얼로그 띄우기
            EditorUtility.DisplayDialog("Save Info", message, "확인");
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
    // 5. 일정 시간에 한번씩 타일을 만들고 싶다.
    // 필요속성 : 딜레이시간, 경과시간
    float currentTime;

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
            currentTime = map.tileCreateTime;
        }
        else if (e.type == EventType.MouseUp)
        {
            isMouseDownState = false;
        }

        if(isMouseDownState == false)
        {
            return;
        }

        // 일정 시간에 한번씩 타일을 만들고 싶다.
        // 1. 시간이 흘렀으니까
        currentTime += Time.fixedDeltaTime;
        // 2. 일정시간이 됐으니까
        if(currentTime < map.tileCreateTime)
        {
            return;
        }
        currentTime = 0;
        // 3. 타일을 만들고 싶다.

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
        if(map.isDrawGrid == false)
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

    // map data 저장하기
    string SaveMap()
    {

        // 1. MapInfo 저장 (바닥타일)
        // 바닥타일 정보를 저장하고 싶다.
        // - 바닥타일이 있는지 조사 해서 없으면 함수 종료
        GameObject floor = GameObject.Find("Floor");
        if(floor == null)
        {
            return "저장할 데이터가 없습니다.\nFloor 객체가 있는지 확인 하세요.";
        }

        MapInfo info = new MapInfo();
        info.width = map.tileX;
        info.height = map.tileY;
        info.prefabName = "Prefabs/" + map.floorTile.name;
        info.x = floor.transform.position.x;
        info.y = floor.transform.position.y;
        info.z = floor.transform.position.z;

        // 2. Tile 들 저장
        List<Tile> tiles = new List<Tile>();
        // Tiles 밑에 있는 타일
        // floor 부모(Tiles)가 갖고 있는 모든 자식들의 Transform 을 하나씩 꺼내온다.
        foreach(Transform tile in floor.transform.parent)
        {
            // Tile 만 검출해서 데이터로 만들고 싶다.
            if(tile.name == "Tile")
            {
                // 게임 오브젝트가 참조 하고 있는 원본프리팹 정보가 필요하다.
                GameObject obj = PrefabUtility.GetCorrespondingObjectFromSource<GameObject>(tile.gameObject);
                // Tiel 구조체 만들기
                Tile t = new Tile();
                t.prefabName = "Prefabs/" + obj.name;
                t.x = tile.position.x;
                t.y = tile.position.y;
                t.z = tile.position.z;

                tiles.Add(t);
            }
        }

        // 3. 파일이름 만들기 -> 파일 저장 다이얼로그 띄워서 저장할 파일이름 경로 가져오기
        string fileName = EditorUtility.SaveFilePanel("Save Map", Application.dataPath, "map", "dat");

        // 데이터를 저장하자
        // Exception 처리
        try
        {
            // 코드작성
            BinaryFormatter bf = new BinaryFormatter();
            // 파일을 열고 통로(스트림) 만들기
            FileStream file = File.Create(fileName);
            // 데이터를 파일로 쓰기
            bf.Serialize(file, info);
            bf.Serialize(file, tiles);
            // 파일 닫아 주기
            file.Close();
        }
        // 만약 오류가 발생하면
        catch(System.Exception e)
        {
            return "오류 발생 : " + e.Message;
        }

        return "Save Success!!";
    }

    // 저장된 데이터 불러오기
    [MenuItem("MyMenu/Load Map")]
    static void LoadMap()
    {
        try
        {
            // 1. 저장된 파일이름 필요
            string fileName = EditorUtility.OpenFilePanel("Open Map", Application.dataPath, "dat");
            if(fileName == null)
            {
                EditorUtility.DisplayDialog("오류", "선택된 파일이 없습니다.", "OK");
                return;
            }
            // 2. 파일 열기
            // 3. 스트림 연결
            FileStream file = File.Open(fileName, FileMode.Open);
            if(file == null || file.Length < 1)
            {
                EditorUtility.DisplayDialog("오류", "잘못된 파일입니다.", "OK");
                return;
            }
            // 4. 데이터 불러오기
            BinaryFormatter bf = new BinaryFormatter();
            MapInfo info = (MapInfo)bf.Deserialize(file);
            List<Tile> tiles = (List<Tile>)bf.Deserialize(file);
            file.Close();

            // 5. 씬 재구성하기
            // - 씬에 Tiles 가 있으면 제거 하고 새로 만들자
            GameObject tilesParent = GameObject.Find("Tiles");
            if(tilesParent)
            {
                DestroyImmediate(tilesParent);
            }
            tilesParent = new GameObject("Tiles");

            // 5.1 바닥타일(맵정보) 재구성
            // - 바닥타일 프리팹 로드
            GameObject floorPrefab = (GameObject)Resources.Load(info.prefabName);
            // - 객체 생성
            GameObject floor = (GameObject)PrefabUtility.InstantiatePrefab(floorPrefab);
            // - 크기 및 위치
            floor.transform.localScale = new Vector3(info.width, 1, info.height);
            floor.transform.position = new Vector3(info.x, info.y, info.z);
            // - 이름 지정
            floor.name = "Floor";
            // - 부모를 Tiles 로 지정
            floor.transform.parent = tilesParent.transform;
            // 5.2 일반타일들 재구성
            foreach(Tile tile in tiles)
            {
                // 1. 프리팹 로드
                GameObject tilePrefab = Resources.Load(tile.prefabName) as GameObject;
                // 2. 객체 생성
                GameObject t = PrefabUtility.InstantiatePrefab(tilePrefab) as GameObject;
                // 3. 위치지정
                t.transform.position = new Vector3(tile.x, tile.y, tile.z);
                // 4. 이름 지정
                t.name = "Tile";
                // 5. Tiles 의 자식으로 등록
                t.transform.parent = tilesParent.transform;
            }

        }
        catch(System.Exception e)
        {
            EditorUtility.DisplayDialog("오류", e.Message, "확인");
        }
    }
}
