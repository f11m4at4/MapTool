using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RectTest : MonoBehaviour
{
    RectTransform rectOri;
    public RectTransform rectTarg;
    // Start is called before the first frame update
    void Start()
    {
        rectOri = GetComponent<RectTransform>();

        rectOri.pivot = rectTarg.pivot;
        rectOri.anchorMin = rectTarg.anchorMin;
        rectOri.anchorMax = rectTarg.anchorMax;

        rectOri.position = rectTarg.position;

        rectOri.sizeDelta = rectTarg.sizeDelta;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
