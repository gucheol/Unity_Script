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

public class GeneralCardScript : MonoBehaviour
{
    Vector2 image_size;   // (width, height) 여권 이미지의 해상도
    Vector2Int mesh_tess = new Vector2Int(16, 24);  // (x, z) 분할 갯수, Model Shape 생성 시 작성한 파라미터

    string cardSynth = "CardSynth";
    //
    List<string> feature_ExtractName = new List<string>();
    List<string> feature_Image = new List<string>();
    List<float[]> feature_rect_color = new List<float[]>();

    //Segmentation
    string[] kor_segList = new string[] { "CardSynth", "PhotoSynth", "KOR" };
    float[][] kor_segList_color = new float[][] { PresetColor.object_background, PresetColor.person_image, PresetColor.kor };
    string[] stamp_segList = new string[] { "CardSynth", "PhotoSynth", "Stamp" };
    float[][] stamp_segList_color = new float[][] { PresetColor.object_background, PresetColor.person_image, PresetColor.stamp };

    public int num_CaptureData = 2000;
    public float random_personImage_grayscale_freq = 0.3f;
    public float random_objectBackground_saturation_freq = 0.3f;
    public float random_hand_show_freq = 0.15f;
    public float random_subcard_show_freq = 0.3f;
    public float random_hologram_freq = 0.3f;
    public bool debug_mode = false;
    public float CameraMinDistance = 6.0f;
    public float CameraMaxDistance = 15.0f;
    public float CameraMaxAngle = Mathf.PI * 9.8f;

    int frame_rate = 3;
    //
    List<string> feature_names = new List<string>();
    List<string> feature_rect = new List<string>();
    string root_savepath;

    struct DataSet
    {
        public Texture[] personImage;
        public Texture[] background;
        public Texture[] cardImage;
        public Texture[] stamp;
        public List<string> korName;
        public List<string> Driver_Type;
        public List<string> Address1;
        public List<string> Address2;
        public List<string> Address3;
        public List<string> Driver_Admin;
        public List<string> Alien_Type;
        public List<string> surName;
        public List<string> givenName;
        public List<string> Nation;
        public List<string> AdminKor;
        public List<string> AdminEng;
    };
    DataSet dataSet = new DataSet();

    public List<string> holo_filenames;
    public List<string> face_filenames;

    GameObject passport_obj = null;
    Material inner_mat_lit = null;
    Material inner_mat_unlit = null;
    List<GameObject> instanceList = new List<GameObject>();

    //Mesh cover_mesh;
    Mesh inner_mesh = null;

    List<Vector3> inner_vertices = new List<Vector3>();

    int frame_index = 0;
    bool useHologram = false;

    // Start is called before the first frame update
    void Start()
    {
        var image_pixel = TempleteImageSize();
        image_size = new Vector2(image_pixel.Item1, image_pixel.Item2);

        if (GameObject.Find("AutoSceneManager") != null)
            AutoSceneSetting();
        MakeSegList();
        MakejsonTextList(feature_ExtractName);
        CreateSaveFolder();
        MakeExtractFeatureList();
        ResourceLoad();
    }

    void Update()
    {
        if (frame_index == num_CaptureData * frame_rate)
        {
            if (GameObject.Find("AutoSceneManager") == null)
                UnityEditor.EditorApplication.isPlaying = false;
            else
            {
                OnSceneQuit();
                AutoCardScript.SceneController.isRunning = false;
            }
        }
        else
        {
            if (frame_index % frame_rate == 0)
            {
                Seg.BackgroundToNomal();
                Seg.Destroy_All_AttachQuad(feature_names);
                Seg.Destroy_All_AttachQuad(feature_rect);
                Seg.SegLightOff();
                Seg.TempleteFilm_On();
                Util.LodingNextFrame();
            }
            // excute a lot of func and capture rgb image
            if (frame_index % frame_rate == 1)
            {
                Util.LodeComplete();
                RandomizePassport();
                Background.ChangeBackground(dataSet.background);

                CaptureTool.RandomizeCamera("Inner", CameraMinDistance, CameraMaxDistance, CameraMaxAngle, debug_mode);
                SubCard.CreateSubObject(dataSet.cardImage, random_subcard_show_freq);

                Hand.Random_Hand_Pose();
                Hand.Show_Hand(random_hand_show_freq);
                Hand.Lit_Hand_Color();
                
                Card_TextChange(debug_mode);
                TextProperty.Ink_smudge_On(feature_ExtractName);
                //TextProperty.TextSpacingOption(feature_ExtractName);
                      
                Effect_Env.TempleteReflectLight_Point(feature_ExtractName);
                //Effect_Env.EachChar_TempleteReflectLight_Point_On(feature_ExtractName);
                Effect_Env.CharShadowOrLight_On(feature_ExtractName, "Inner");             
                Effect_Env.StudioBrightness_On();

                useHologram = Hologram.CreateHologram(random_hologram_freq, holo_filenames, instanceList, feature_ExtractName);
                
                Util.AntiAliasingFunc(true); // 유니티에서 자동으로 안티얼라이어싱 필터 적용 
                //모든 렌더링이 끝난 후, 캡처
                string filename = "rgb_" + (frame_index / frame_rate).ToString() + ".png";
                string path = root_savepath + "/rgb/" + filename;
                ScreenCapture.CaptureScreenshot(path); 
            }
            // turn off func and capture seg image, save character json data
            if (frame_index % frame_rate == 2)
            {              
                Seg.SegLightOn();
                Seg.BackgroundToBlack();
                Seg.TempleteFilm_Off();

                Util.AntiAliasingFunc(false);
                SubCard.DelSubObject();
                Hand.Seg_Hand_Color();
                TextProperty.Ink_smudge_Off(feature_ExtractName);

                //Effect_Env.EachChar_TempleteReflectRight_Point_Off(feature_ExtractName); //
                Effect_Env.CharShadowOrLight_Off();
                Effect_Env.StudioBrightness_Off();
                // hologram destroy, before take a picture
                if (useHologram == true)
                    Hologram.DestroyHologram(instanceList);
                // Attach Segmentation Object
                for (int index = 0; index < feature_rect.Count; index++)
                    Seg.CreateAttachQuad(feature_rect[index], feature_rect_color[index]);
                // Take a segmentation picture
                string filename = "seg_" + (frame_index / frame_rate).ToString() + ".png";
                string path = root_savepath + "/seg/" + filename;
                ScreenCapture.CaptureScreenshot(path);
                // Json Text
                string json_seg_info_list = JsonCharacter.MakeBoundingBoxData(feature_names, filename);
                filename = String.Format("BoundingBox_{0}.json", frame_index / frame_rate);
                path = root_savepath + "/json/" + filename;
                System.IO.File.WriteAllText(path, json_seg_info_list);
            }
        }
        frame_index++;
    }

    void OnSceneQuit()
    {
        AutoCardScript.SceneController.count++;
        AutoCardScript.SceneController.isRunning = false;
    }
    void AutoSceneSetting()
    {
        num_CaptureData = AutoCardScript.SceneController.num_CaptureData;
        random_personImage_grayscale_freq = AutoCardScript.SceneController.random_personImage_grayscale_freq;
        random_hand_show_freq = AutoCardScript.SceneController.random_hand_show_freq;
        random_subcard_show_freq = AutoCardScript.SceneController.random_subcard_show_freq;
        random_hologram_freq = AutoCardScript.SceneController.random_hologram_freq;
    }

    void RandomizePassport()
    {
        string cover_path = "";
        string inner_path = "Meshes/real_card";

        CreatePassportShape(cover_path, inner_path);

        Templete_Property.PersonImage.ChangePerson(dataSet.personImage);
        Templete_Property.PersonImage.ChangeGrayScale(random_personImage_grayscale_freq);
        Templete_Property.PersonImage.ChagneTransparent();
        Templete_Property.ChangeColorSaturation();

        if (GameObject.Find("KOR") != null)
            Templete_Property.ChangeKorColor();
        if (GameObject.Find("Stamp") != null)
        {
            Templete_Property.ChangeStamp(dataSet.stamp);
            Templete_Property.ChangeStampTransparent();
        }
        Templete_Property.ChangeTempleteMetalic();
    }
    void CreatePassportShape(string cover_path, string inner_path)
    {
        if (passport_obj)
        {
            UnityEngine.Object.Destroy(passport_obj);
        }

        passport_obj = new GameObject();
        passport_obj.name = "Passport";
        passport_obj.transform.parent = transform;

        GameObject inner_obj = new GameObject();
        inner_obj.name = "Inner";
        inner_obj.transform.parent = passport_obj.transform;

        MeshFilter inner_mesh_filter = inner_obj.AddComponent<MeshFilter>();
        MeshRenderer inner_mesh_renderer = inner_obj.AddComponent<MeshRenderer>();

        inner_mesh = Resources.Load<Mesh>(inner_path);
        inner_mesh_filter.sharedMesh = inner_mesh;
        inner_mesh_renderer.material = inner_mat_lit;

        inner_vertices.Clear();
        inner_mesh.GetVertices(inner_vertices);
        DataNeeds.inner_vertices = inner_vertices;
        DataNeeds.passport_obj = passport_obj.transform.Find("Inner").gameObject;
        SendDataToBoundingBox();
    }
    void CreateSaveFolder()
    {
        //string folder_guid = Guid.NewGuid().ToString();

        string sceneName = GetSceneType();

        root_savepath = Application.persistentDataPath + "/" + sceneName;

        DirectoryInfo root_di = new DirectoryInfo(root_savepath);
        root_di.Create();

        string rgb_savepath = root_savepath + "/rgb";
        DirectoryInfo rgb_di = new DirectoryInfo(rgb_savepath);
        rgb_di.Create();

        string seg_savepath = root_savepath + "/seg";
        DirectoryInfo seg_di = new DirectoryInfo(seg_savepath);
        seg_di.Create();

        string json_savepath = root_savepath + "/json";
        DirectoryInfo json_di = new DirectoryInfo(json_savepath);
        json_di.Create();

    }
    void MakeExtractFeatureList()
    {
        foreach (string feature in feature_ExtractName)
        {
            string feature_name = feature;
            feature_names.Add(feature_name);
        }
        foreach (string feature in feature_Image)
        {
            string feature_name = feature;
            feature_rect.Add(feature_name);
        }
    }
    void SendDataToBoundingBox()
    {
        DataNeeds.image_size = image_size;
        DataNeeds.passport_inner_object = GameObject.Find(cardSynth);
        DataNeeds.mesh_tess = mesh_tess;
    }
    // 각 여권 종류
    void Card_TextChange(bool debug_mode)
    {
        if (!debug_mode)
        {
            string current_scene = GetSceneType();
            if (current_scene == "Alien")
            {
                TMP_Text Name = GameObject.Find("Name").GetComponent<TMP_Text>();
                TMP_Text Sex = GameObject.Find("Sex").GetComponent<TMP_Text>();
                TMP_Text Nation = GameObject.Find("Nation").GetComponent<TMP_Text>();
                TMP_Text Number = GameObject.Find("Number").GetComponent<TMP_Text>();
                TMP_Text WaterMark_Number = GameObject.Find("WaterMark_Number").GetComponent<TMP_Text>();
                TMP_Text Date = GameObject.Find("Date").GetComponent<TMP_Text>();
                TMP_Text OfficeKor = GameObject.Find("OfficeKor").GetComponent<TMP_Text>();
                TMP_Text OfficeEng = GameObject.Find("OfficeEng").GetComponent<TMP_Text>();
                TMP_Text Type = GameObject.Find("Type").GetComponent<TMP_Text>();

                RandomText_Card.Random_AlienType(dataSet.Alien_Type, Type);
                RandomText_Card.Random_AlienName(dataSet.surName, dataSet.givenName, Name);
                RandomText_Card.Random_Sex(Sex);
                RandomText_Card.Random_Nation(dataSet.Nation, Nation);
                RandomText_Card.Random_Number(Number, WaterMark_Number);
                RandomText_Card.Random_AlienDate(Date);
                RandomText_Card.Random_OfficeKor(dataSet.AdminKor, OfficeKor);
                RandomText_Card.Random_OfficeEng(dataSet.AdminEng, OfficeEng);
            }
            else if (current_scene == "Driver")
            {
                TMP_Text Name = GameObject.Find("Name").GetComponent<TMP_Text>();
                TMP_Text type = GameObject.Find("Type").GetComponent<TMP_Text>();
                TMP_Text CardNumber = GameObject.Find("CardNumber").GetComponent<TMP_Text>();
                TMP_Text PersonNumber = GameObject.Find("PersonNumber").GetComponent<TMP_Text>();
                TMP_Text Address = GameObject.Find("Address").GetComponent<TMP_Text>();
                TMP_Text Date1 = GameObject.Find("Date1").GetComponent<TMP_Text>();
                TMP_Text Date2 = GameObject.Find("Date2").GetComponent<TMP_Text>();
                TMP_Text PublishDate = GameObject.Find("PublishDate").GetComponent<TMP_Text>();
                //TMP_Text Require = GameObject.Find("Require").GetComponent<TMP_Text>();
                TMP_Text SerialNumber = GameObject.Find("SerialNumber").GetComponent<TMP_Text>();
                TMP_Text Admin = GameObject.Find("Admin").GetComponent<TMP_Text>();

                RandomText_Card.Random_KorName(dataSet.korName, Name);
                RandomText_Card.Random_DriverType(dataSet.Driver_Type, type);
                RandomText_Card.Random_CardNumber(CardNumber);
                RandomText_Card.Random_PersonNumber(PersonNumber);
                RandomText_Card.Random_DriverAddress(dataSet.Address1, dataSet.Address2, dataSet.Address3, Address);
                RandomText_Card.Random_DriverDate(Date1, Date2, PublishDate);
                //RandomText_Card.Random_Require(Require);
                RandomText_Card.Random_SerialNumber(SerialNumber);
                RandomText_Card.Random_Admin(dataSet.Driver_Admin, Admin);
            }
            else if (current_scene == "ID")
            {
                TMP_Text Name = GameObject.Find("Name").GetComponent<TMP_Text>();
                TMP_Text IDNumber = GameObject.Find("IDNumber").GetComponent<TMP_Text>();
                TMP_Text Address = GameObject.Find("Address").GetComponent<TMP_Text>();
                TMP_Text Date = GameObject.Find("Date").GetComponent<TMP_Text>();
                TMP_Text Admin = GameObject.Find("Admin").GetComponent<TMP_Text>();

                RandomText_Card.Random_KorName(dataSet.korName, Name);
                RandomText_Card.Random_PersonNumber(IDNumber);
                RandomText_Card.Random_IDAddress(dataSet.Address1, dataSet.Address2, dataSet.Address3, Address, Admin);
                RandomText_Card.Random_IDDate(Date);
            }
            else
            {
                TMP_Text Name = GameObject.Find("Name").GetComponent<TMP_Text>();
                TMP_Text Sex = GameObject.Find("Sex").GetComponent<TMP_Text>();
                TMP_Text Nation = GameObject.Find("Nation").GetComponent<TMP_Text>();
                TMP_Text Number = GameObject.Find("Number").GetComponent<TMP_Text>();
                TMP_Text WaterMark_Number = GameObject.Find("WaterMark_Number").GetComponent<TMP_Text>();
                TMP_Text Date = GameObject.Find("Date").GetComponent<TMP_Text>();
                TMP_Text OfficeKor = GameObject.Find("OfficeKor").GetComponent<TMP_Text>();
                TMP_Text OfficeEng = GameObject.Find("OfficeEng").GetComponent<TMP_Text>();

                RandomText_Card.Random_AlienName(dataSet.surName, dataSet.givenName, Name);
                RandomText_Card.Random_Sex(Sex);
                RandomText_Card.Random_Nation(dataSet.Nation, Nation);
                RandomText_Card.Random_Number(Number, WaterMark_Number);
                RandomText_Card.Random_AlienDate(Date);
                RandomText_Card.Random_OfficeKor(dataSet.AdminKor, OfficeKor);
                RandomText_Card.Random_OfficeEng(dataSet.AdminEng, OfficeEng);
            }
        }      
    }
    // json 글자 뽑기
    void MakejsonTextList(List<string> extractNames)
    {
        string current_scene = GetSceneType();
        //feature_ExtractName
        List<string> addNames;
        if (current_scene == "Alien")
        {
            addNames = new List<string>()
            //{ "FixedText_2"};
            { "FixedText1", "FixedText2", "FixedText3", "FixedText4", "FixedText5","FixedText6","FixedText7","FixedText8","FixedText9","FixedText10","FixedText11",
            "FixedText12","FixedText13_part1","FixedText13_part2","FixedText13_part3","FixedText14","FixedText15","FixedText16","FixedText17",
            "WaterMark_Number", "Name", "Sex", "Nation", "Number", "Type", "Date", "OfficeKor", "OfficeEng"};
        }
        else if(current_scene == "Driver")
        {
            addNames = new List<string>()
            //{ "PersonNumber"};
            {"FixedText1", "FixedText2_part1","FixedText2_part2","FixedText3_part1","FixedText3_part2","FixedText4","FixedText5","FixedText6","FixedText7",
            "FixedText8","FixedText9","FixedText10","FixedText11","FixedText12",
            "CardNumber", "Name", "PersonNumber", "Address", "Date1", "Date2", "Require", "Type", "SerialNumber", "PublishDate",
            "Admin", "CardNumber",};
        }
        else if(current_scene == "ID")
        {
            addNames = new List<string>()
            //{ "Name", "IDNumber", "Date"};
            { "Name", "IDNumber", "Date", "Address", "Admin", "FixedText1"};
        }
        else
        {
            addNames = new List<string>()
            //{"Number", "Name", "Date"};
            { "FixedText1", "FixedText2", "FixedText3", "FixedText4", "FixedText5", "FixedText6", "FixedText7", "FixedText8", "FixedText9", "FixedText10",
            "FixedText11", "FixedText12", "FixedText13", "FixedText14", "FixedText15", "FixedText16_part1", "FixedText16_part2", "FixedText16_part3",
            "Name", "Sex", "Nation", "Number", "Date", "OfficeKor", "OfficeEng", "Type"};
        }

        foreach(string name in addNames)
        {
            extractNames.Add(name);
        }
    }
    void MakeSegList()
    {
        if(GameObject.Find("KOR") != null)
        {
            for (int i = 0; i < kor_segList.Length; i++)
            {
                feature_Image.Add(kor_segList[i]);
                feature_rect_color.Add(kor_segList_color[i]);
            }
        }
        else
        {
            for (int i = 0; i < stamp_segList.Length; i++)
            {
                feature_Image.Add(stamp_segList[i]);
                feature_rect_color.Add(stamp_segList_color[i]);
            }            
        }   
    }
    (int, int) TempleteImageSize()
    {
        Texture image = GameObject.Find("CardSynth").GetComponent<MeshRenderer>().material.mainTexture;
        return (image.width, image.height);
    }
    void ResourceLoad()
    {
        face_filenames = LoadResouce.CollectFaceTextureFilenames();
        holo_filenames = LoadResouce.CollectHoloMeshFilenames();

        //inner_mat_lit = Resources.Load<Material>("Materials/CardRenderTextureMaterial");
        inner_mat_lit = Resources.Load<Material>("Materials/Card/CardRenderTextureMaterial");
        inner_mat_unlit = Resources.Load<Material>("Materials/Card/CardRenderTextureMaterialUnlit");

        dataSet.personImage = Resources.LoadAll<Texture>("Faces");
        if (!debug_mode)
            dataSet.background = Resources.LoadAll<Texture>("Background");
        else
            dataSet.background = Resources.LoadAll<Texture>("Background/Debug");

        dataSet.stamp = Resources.LoadAll<Texture>("Stamp");
        dataSet.cardImage = Resources.LoadAll<Texture>("Subcard");
        dataSet.Alien_Type = LoadResouce.TextFile("Alien_Type");
        dataSet.surName = LoadResouce.TextFile("surName");
        dataSet.givenName = LoadResouce.TextFile("givenName");
        dataSet.Nation = LoadResouce.TextFile("Nation");
        dataSet.AdminKor = LoadResouce.TextFile("AdminKor");
        dataSet.AdminEng = LoadResouce.TextFile("AdminEng");
        dataSet.Driver_Type = LoadResouce.TextFile("Driver_Type");
        dataSet.korName = LoadResouce.TextFile("korName");
        dataSet.Address1 = LoadResouce.TextFile("Address1");
        dataSet.Address2 = LoadResouce.TextFile("Address2");
        dataSet.Address3 = LoadResouce.TextFile("Address3");
        dataSet.Driver_Admin = LoadResouce.TextFile("Driver_Admin");
    }
    string GetSceneType()
    {
        string scene_fullName = SceneManager.GetActiveScene().name;
        int underbar_index = scene_fullName.IndexOf("_");
        string scene_type = scene_fullName.Substring(0, underbar_index);
        return scene_type;
    }
}