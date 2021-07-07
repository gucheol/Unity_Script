using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombinedCardScript : MonoBehaviour
{
    Vector2 image_size = new Vector2(883, 625);     // (width, height) 여권 이미지의 해상도
    Vector2Int mesh_tess = new Vector2Int(16, 24);  // (x, z) 분할 갯수, Model Shape 생성 시 작성한 파라미터

    List<string> boundingBoxList = new List<string> { "Number" };
    List<string> SegmentationList = new List<string> { " " };
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
