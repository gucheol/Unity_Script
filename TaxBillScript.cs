﻿using System.Collections;
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

using LoadResource_Script;
using Object_Script;
using Effect_Script;
using CaptureTool_Script;
using JsonData_Script;
using TextProperty_Script;
using CalcCoord_Script;
using ReceiptTag_Script;
using TaxBillTextGen_Script;

public struct FileDataSet
{
    public List<string> company_name;
    public List<string> kor_name;
    public List<string> address1;
    public List<string> address2;
    public List<string> address3;
    public List<string> business_category;
    public List<string> business_type;
    public List<string> goods_name;
};
public class func_collect
{
    public static void LoadResources()
    {
        TaxBill.dataset.address1 = LoadResouce.TextFile("Address1");
        TaxBill.dataset.address2 = LoadResouce.TextFile("Address2");
        TaxBill.dataset.address3 = LoadResouce.TextFile("Address3");
        TaxBill.dataset.kor_name = LoadResouce.TextFile("korName");
        TaxBill.dataset.company_name = LoadResouce.TextFile("Receipt/company_name");
        TaxBill.dataset.business_category = LoadResouce.TextFile("Receipt/business_category");
        TaxBill.dataset.business_type = LoadResouce.TextFile("Receipt/business_type");
        TaxBill.dataset.goods_name = LoadResouce.TextFile("Receipt/goods_name");
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
    public static void SaveOcrInfoToJson(List<string> ocr_obj_list, string root_path, int stage_cnt)
    {
        string filename = "seg_" + stage_cnt.ToString() + ".png";
        string ocr_info = JsonCharacter.MakeBoundingBoxData(ocr_obj_list, filename);
        filename = String.Format("BoundingBox_{0}.json", stage_cnt);
        string path = root_path + "/json/" + filename;
        System.IO.File.WriteAllText(path, ocr_info);
    }
    public static GameObject CreateModelFromTemplete(string model_folder_path, string mat_path)
    {
        GameObject model_package = CreateObj("Model_Package", GameObject.Find("SceneManager").transform);
        GameObject main_model = CreateObj("Model", model_package.transform);
        string model_path = SelectOneModelPath(model_folder_path);
        AddModelAttr(main_model, model_path, mat_path);
        List<Vector3> main_model_vertices = main_model.GetComponent<MeshFilter>().mesh.vertices.ToList();

        // 나중에 수정해야됨
        DataNeeds.inner_vertices = main_model_vertices;
        DataNeeds.passport_obj = model_package.transform.Find("Model").gameObject;
        DataNeeds.image_size = func_collect.TempleteImageSize();
        DataNeeds.passport_inner_object = GameObject.Find(TaxBill.templete_name);
        DataNeeds.mesh_tess = TaxBill.mesh_tess;

        return main_model;
    }
    public static string SelectOneModelPath(string folder_path)
    {
        List<string> model_paths = new List<string>();
        string full_folder_path = Application.dataPath + "/Resources/" + folder_path;
        List<string> file_paths = Directory.GetFiles(full_folder_path).ToList();
        foreach (string path in file_paths)
        {
            string ext = Path.GetExtension(path);
            if (ext == ".obj" || ext == ".fbx")
            {
                string filename = Path.GetFileNameWithoutExtension(path);
                model_paths.Add(filename);
            }
        }
        int index = UnityEngine.Random.Range(0, model_paths.Count);
        string model_path = folder_path + "/" + model_paths[index];
        return model_path;
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
        main_model_mf.sharedMesh = Resources.Load<Mesh>(model_path); //model_path //"Meshes/real_card"
        main_model_mr.material = Resources.Load<Material>(mat_path);
    }
    public static Vector2 TempleteImageSize()
    {
        Texture image = GameObject.Find("Templete").GetComponent<MeshRenderer>().material.mainTexture;
        return new Vector2(image.width, image.height);
    }
    public static void CreateDefaultModel()
    {
        TaxBill.model = func_collect.CreateModelFromTemplete(TaxBill.model_path, TaxBill.mat_path);
        
    }
    public static void CreateTaggingModel()
    {
        Material templete_mat = GameObject.Find(TaxBill.templete_name).GetComponent<MeshRenderer>().material;
        string file_path = LoadImageFromSystemFolder(templete_mat, TaxBill.tagging_folder);
        TaxBill.tagging_file_path = file_path;
        TaxBill.model = func_collect.CreateModelFromTemplete(TaxBill.model_path, TaxBill.mat_path);
    }
    public static Texture2D LoadImage(string img_path)
    {
        byte[] byteTexture = System.IO.File.ReadAllBytes(img_path);
        Texture2D texture = new Texture2D(0, 0);
        texture.LoadImage(byteTexture);
        return texture;
    }
    public static string LoadImageFromSystemFolder(Material templete_mat, string img_folder_path)
    {
        List<string> paths = GetFilePathsInFolder(img_folder_path, ".jpg");
        int index = UnityEngine.Random.Range(0, paths.Count);
        Texture2D texture = LoadImage(paths[index]);
        templete_mat.mainTexture = texture;
        return paths[index];
    }
    public static List<string> GetFilePathsInFolder(string img_folder_path, string ext)
    {
        return Directory.GetFiles(img_folder_path, "*" + ext).ToList();
    }
    public static void SaveTransFormTextBounds(string root_path, int stage_cnt)
    {
        string filename = "seg_" + stage_cnt.ToString() + ".png";
        // json string을 반환해야 함. 
        string ocr_info = ReceiptTagDataScript.LoadAndTransFormBoundingBoxData(filename);
        filename = String.Format("BoundingBox_{0}.json", stage_cnt);
        string path = root_path + "/json/" + filename;
        System.IO.File.WriteAllText(path, ocr_info);
    }
    public static void TurnOffMeshRenderer(string templete_obj_name)
    {
        GameObject templete = GameObject.Find(templete_obj_name);
        List<string> text_list = GetChildNameList(templete);
        for(int i=0; i < text_list.Count; i++)
        {
            GameObject tmp_obj = GameObject.Find(text_list[i]);
            tmp_obj.GetComponent<MeshRenderer>().enabled = false;
        }
    }
    public static void TaxBill_TextChange()
    {
        RandomTextTaxBill.Gen_BookID("책번호-권-내용");
        RandomTextTaxBill.Gen_BookID("책번호-호-내용");
        RandomTextTaxBill.Gen_SN("일련번호-내용");
        RandomTextTaxBill.Gen_CompanyID("공급자-등록번호-내용");
        RandomTextTaxBill.Gen_CompanyName("공급자-상호-내용", TaxBill.dataset.company_name);
        RandomTextTaxBill.Gen_PersonName("공급자-성명-내용", TaxBill.dataset.kor_name);
        RandomTextTaxBill.Gen_CompanyAddress("공급자-사업장주소-내용", TaxBill.dataset.address1, TaxBill.dataset.address2, TaxBill.dataset.address3);
        RandomTextTaxBill.Gen_Category("공급자-업태-내용", TaxBill.dataset.business_category);
        RandomTextTaxBill.Gen_type("공급자-종목-내용", TaxBill.dataset.business_type);
        RandomTextTaxBill.Gen_CompanyID("공급받는자-등록번호-내용");
        RandomTextTaxBill.Gen_CompanyName("공급받는자-상호-내용", TaxBill.dataset.company_name);
        RandomTextTaxBill.Gen_PersonName("공급받는자-성명-내용", TaxBill.dataset.kor_name);
        RandomTextTaxBill.Gen_CompanyAddress("공급받는자-사업장주소-내용", TaxBill.dataset.address1, TaxBill.dataset.address2, TaxBill.dataset.address3);
        RandomTextTaxBill.Gen_Category("공급받는자-업태-내용", TaxBill.dataset.business_category);
        RandomTextTaxBill.Gen_type("공급받는자-종목-내용", TaxBill.dataset.business_type);
        RandomTextTaxBill.Gen_Date("작성-년월일-내용_part1", "작성-년월일-내용_part2", "작성-년월일-내용_part3");
        RandomTextTaxBill.Gen_SupplyPrice("공급가액-금액-공란수-내용", "공급가액-금액-내용", "세액-내용", "합계금액-내용");
        RandomTextTaxBill.Gen_ListContents(TaxBill.dataset.goods_name);

    }
    public static void RunTaggingModel(int stage_cnt, bool DebugCameraFix)
    {
        // 카메라 세팅 나중에 분리해야함
        float CameraMinDistance = 8.9f;
        float CameraMaxDistance = 9.0f;
        float CameraMaxAngle = Mathf.PI;

        func_collect.CreateTaggingModel();
        Background.ChangeBackground(TaxBill.background);
        CaptureTool.RandomizeCamera(TaxBill.model.name, CameraMinDistance, CameraMaxDistance, CameraMaxAngle, DebugCameraFix);
        Util.AntiAliasingFunc(true);
        func_collect.CaptureScreenshot_Original(TaxBill.root_path, stage_cnt);
        SaveTransFormTextBounds(TaxBill.root_path, stage_cnt);
    }
    public static void RunDefaultModel(int stage_cnt, bool DebugCameraFix)
    {
        //파란색 텍스트 rgb: (9,104,214)
        // 카메라 세팅 나중에 분리해야함
        float CameraMinDistance = 6.0f;
        float CameraMaxDistance = 13.0f;
        float CameraMaxAngle = Mathf.PI * 6.36f;

        func_collect.CreateDefaultModel();
        Background.ChangeBackground(TaxBill.background);
        CaptureTool.RandomizeCamera(TaxBill.model.name, CameraMinDistance, CameraMaxDistance, CameraMaxAngle, DebugCameraFix);
        TaxBill_TextChange();

        Util.AntiAliasingFunc(true);
        func_collect.CaptureScreenshot_Original(TaxBill.root_path, stage_cnt);
        TaxBill.ocr_obj_list = func_collect.GetOcrObjName(TaxBill.templete_name);
        //func_collect.SaveOcrInfoToJson(TaxBill.ocr_obj_list, TaxBill.root_path, stage_cnt);
    }
}
public static class TaxBill
{
    public static GameObject model;
    public static string templete_name;
    public static Texture[] background;
    public static Dictionary<string, Color> seg_obj_list = new Dictionary<string, Color>() { };
    public static List<string> ocr_obj_list = new List<string>() { };
    public static Vector2Int mesh_tess;
    public static string root_path;
    public static string model_path;
    public static string mat_path;
    public static string tagging_folder;
    public static string tagging_file_path;
    public static FileDataSet dataset;
}
public class TaxBillScript : MonoBehaviour
{
    public bool isTagData = false;
    public int iter = 10;
    public bool DebugCameraFix = false;
    int frame_index = 0;
    int frame_rate = 3;
    const int PREPARE_STAGE = 0;
    const int ORIGINAL_STAGE = 1;
    const int SEGMENTATION_STAGE = 2;

    void Start()
    {
        TaxBill.templete_name = "Templete";
        TaxBill.root_path = Application.persistentDataPath + "/" + SceneManager.GetActiveScene().name;
        TaxBill.background = Resources.LoadAll<Texture>("Background/Debug"); //"Background"
        TaxBill.model_path = "Meshes/Paper";
        TaxBill.mat_path = "Materials/Receipt/TaxbillRenderTextureMat";
        TaxBill.mesh_tess = new Vector2Int(16, 24); //mesh info
        // tagging
        TaxBill.tagging_folder = "C:/Workspace/Unity/AppTestSynthID/Assets/Resources/Textures/TempleteTexture/ReceiptTexture/Demo"; // Z:\\Workspace\\data\\nullee_invoice\\태깅_아르바이트\\작업물_권현지\\완료\\세금계산서
        func_collect.CreateSaveFolder();
        func_collect.LoadResources();
        if (isTagData)
            func_collect.TurnOffMeshRenderer(TaxBill.templete_name);
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
                if (isTagData)
                    func_collect.RunTaggingModel(stage_cnt, DebugCameraFix);
                else
                    func_collect.RunDefaultModel(stage_cnt, DebugCameraFix);
            }
            else if (stage_level == SEGMENTATION_STAGE)
                if (!isTagData)
                    func_collect.SaveOcrInfoToJson(TaxBill.ocr_obj_list, TaxBill.root_path, stage_cnt);
        }
        frame_index++;
    }
}
