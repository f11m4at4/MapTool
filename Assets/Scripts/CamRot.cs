using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamRot : MonoBehaviour
{
    Vector3 angles;
    public float rotSpeed = 200;
    // Start is called before the first frame update
    void Start()
    {
        // 처음 카메라 각도 적용 해주기위해 저장
        angles = transform.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        float x = -Input.GetAxis("Mouse Y");
        float y = Input.GetAxis("Mouse X");

        angles.x += x * rotSpeed * Time.deltaTime;
        angles.y += y * rotSpeed * Time.deltaTime;

        angles.x = Mathf.Clamp(angles.x, -90, 90);

        transform.eulerAngles = angles;
    }
}
