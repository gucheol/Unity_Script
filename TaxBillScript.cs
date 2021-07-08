using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using System;
using System.Linq;
using UnityEngine.SceneManagement;

using Segmentation_Script;
using BoundingBox_Script;
using Hand_Script;
using ImageUtil_Script;
using RandomText_Script;
using LoadResource_Script;
using Object_Script;
using Effect_Script;
using CaptureTool_Script;
using JsonData_Script;
using TextProperty_Script;
using CalcCoord_Script;

public class func_collect
{
    class CameraAngle
    {
        public Vector3 camera_angle;
    }
    //util
    public static void CreateSaveFolder()
    {
        string scene_name = SceneManager.GetActiveScene().name;

        string root_path = Application.persistentDataPath + "/" + scene_name;
        CreateDir(root_path);
        CreateDir(root_path + "/rgb");
        CreateDir(root_path + "/seg");
        CreateDir(root_path + "/json");
    }
    public static void CreateDir(string path)
    {
        DirectoryInfo dir = new DirectoryInfo(path);
        if (dir.Exists == false)
            dir.Create();
    }
    public static List<string> GetChildNameList(GameObject obj)
    {
        List<string> child_list = new List<string>();
        int child_num = obj.transform.childCount;
        for (int i = 0; i < child_num; i++)
        {
            Transform child_obj = obj.transform.GetChild(i);
            child_list.Add(child_obj.name);
        }
        return child_list;
    }
    public static List<string> GetOcrObjName(string templete_name)
    {
        GameObject tempele_obj = GameObject.Find(templete_name);
        List<string> ocr_list = GetChildNameList(tempele_obj);
        return ocr_list;
    }
    public static void CaptureScreenshot_Original(string root_path, int stage_cnt)
    {
        string filename = "rgb_" + stage_cnt.ToString() + ".png";
        string path = root_path + "/rgb/" + filename;
        ScreenCapture.CaptureScreenshot(path);
    }
    public static void SaveOcrInfoToJson(GameObject model, string root_path, int stage_cnt)
    {
        string filename = "seg_" + stage_cnt.ToString() + ".png";
        string ocr_info = JsonCharacter.MakeBoundingBoxData(TaxBill.ocr_obj_list, filename);
        filename = String.Format("BoundingBox_{0}.json", stage_cnt);
        string path = root_path + "/json/" + filename;
        System.IO.File.WriteAllText(path, ocr_info);
    }
    public static GameObject CreateModelFromTemplete(string model_path, string mat_path)
    {
        GameObject model_package = CreateObj("Model_Package", GameObject.Find("SceneManager").transform);
        GameObject main_model = CreateObj("Model", model_package.transform);
        AddModelAttr(main_model, model_path, mat_path);
        List<Vector3> main_model_vertices = main_model.GetComponent<MeshFilter>().mesh.vertices.ToList();

        // 나중에 수정해야됨
        DataNeeds.inner_vertices = main_model_vertices;
        DataNeeds.passport_obj = model_package.transform.Find("Model").gameObject;
        DataNeeds.image_size = TaxBill.image_size;
        DataNeeds.passport_inner_object = GameObject.Find(TaxBill.templete_name);
        DataNeeds.mesh_tess = TaxBill.mesh_tess;

        return main_model;
    }
    public static GameObject CreateObj(string obj_name, Transform transform)
    {
        GameObject obj = GameObject.Find(obj_name);
        if (obj)
            UnityEngine.Object.Destroy(obj);

        GameObject new_obj = new GameObject();
        new_obj.name = obj_name;
        new_obj.transform.parent = transform;
        return new_obj;
    }
    public static void AddModelAttr(GameObject model_obj, string model_path, string mat_path)
    {
        MeshFilter main_model_mf = model_obj.AddComponent<MeshFilter>();
        MeshRenderer main_model_mr = model_obj.AddComponent<MeshRenderer>();
        main_model_mf.sharedMesh = Resources.Load<Mesh>(model_path);
        main_model_mr.material = Resources.Load<Material>(mat_path);
    }
    public static Vector2 TempleteImageSize()
    {
        Texture image = GameObject.Find("Templete").GetComponent<MeshRenderer>().material.mainTexture;
        return new Vector2(image.width, image.height);
    }
}
public static class TaxBill
{
    public static GameObject model;
    public static string scene_name;
    public static Vector2 image_size;
    public static Vector2Int mesh_tess;
    public static string templete_name = "Templete";
    public static string root_path;
    public static Dictionary<string, Color> seg_obj_list = new Dictionary<string, Color>() { };
    public static List<string> ocr_obj_list = new List<string>() { };
    public static Texture[] background;
    public static List<Vector3> main_model_vertices;
    public static string model_path;
    public static string mat_path;
}
public class TaxBillScript : MonoBehaviour
{
    public int iter = 10;

    GameObject model_package;
    int frame_index = 0;
    int frame_rate = 100;
    const int PREPARE_STAGE = 0;
    const int ORIGINAL_STAGE = 1;
    const int SEGMENTATION_STAGE = 2;
    void Start()
    {
        TaxBill.scene_name = SceneManager.GetActiveScene().name;
        TaxBill.root_path = Application.persistentDataPath + "/" + TaxBill.scene_name;
        TaxBill.background = Resources.LoadAll<Texture>("Background");
        TaxBill.ocr_obj_list = func_collect.GetOcrObjName(TaxBill.templete_name); //new List<string>() { "FixedText11_part1", "FixedText11_part2", "FixedText11_part3" };
        TaxBill.model_path = "Meshes/real_card"; //flat_card_32x48
        TaxBill.mat_path = "Materials/Receipt/TaxbillRenderTextureMat"; //매터리얼 바꿔야됨
        TaxBill.image_size = func_collect.TempleteImageSize();
        TaxBill.mesh_tess = new Vector2Int(16, 24); //new Vector2Int(32, 48)
        func_collect.CreateSaveFolder();
    }
    void Update()
    {
        int stage_level = frame_index % frame_rate;
        int stage_cnt = frame_index / frame_rate;
        int end_frame = iter * frame_rate;

        if (frame_index == end_frame)
        {
             UnityEditor.EditorApplication.isPlaying = false;
        }
        else
        {
            if (stage_level == ORIGINAL_STAGE)
            {
                TaxBill.model = func_collect.CreateModelFromTemplete(TaxBill.model_path, TaxBill.mat_path);

                Background.ChangeBackground(TaxBill.background);
                CaptureTool.RandomizeCamera("Model",true);
                //Effect_Env.TempleteReflectLight_Point(TaxBill.ocr_obj_list);
                //Effect_Env.CharShadowOrLight_On(TaxBill.ocr_obj_list);
                Util.AntiAliasingFunc(true);
                func_collect.CaptureScreenshot_Original(TaxBill.root_path, stage_cnt);
                func_collect.SaveOcrInfoToJson(TaxBill.model, TaxBill.root_path, stage_cnt);
            }
        }
        frame_index++;
    }
}
