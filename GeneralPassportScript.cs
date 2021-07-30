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

public class GeneralPassportScript : MonoBehaviour
{
    public int num_CaptureData = 5000;
    public float CameraMinDistance = 6.0f;
    public float CameraMaxDistance = 15.0f;
    public float CameraMaxAngle = Mathf.PI * 9.8f;
    public float random_objectBackground_saturation_freq = 0.3f;
    public float random_hand_show_freq = 0.15f;
    public float random_subcard_show_freq = 0.3f;
    public float random_hologram_freq = 0.3f;
    public bool isFormattedRandomMR = true;
    public bool debug_mode = false;

    Vector2 image_size;     // (width, height) 여권 이미지의 해상도
    Vector2Int mesh_tess = new Vector2Int(16, 24);  // (x, z) 분할 갯수, Model Shape 생성 시 작성한 파라미터

    string passportInnerSynth = "PassportInnerSynth";
    //should Set
    List<string> feature_ExtractName = new List<string>() { "Mr1", "Mr2", "PassportNo" };
    List<string> feature_Image = new List<string>();
    List<float[]> feature_rect_color = new List<float[]>();

    string[] seg_list = new string[] { "PassportCoverSynth", "PassportInnerSynth", "PhotoSynth", "Mr" };
    float[][] seg_color_list = new float[][] { PresetColor.black, PresetColor.object_background, PresetColor.person_image, PresetColor.mr };
    //
    List<string> feature_names = new List<string>();
    List<string> feature_rect = new List<string>();
    string root_savepath;


    struct DataSet
    {
        public Texture[] personImage;
        public Texture[] background;
        public Texture[] cardImage;
        public List<string> type;
        public List<string> surName;
        public List<string> givenName;
        public List<string> korName;
    };
    DataSet dataSet = new DataSet();

    public List<string> cover_filenames;
    public List<string> inner_filenames;
    public List<string> holo_filenames;
    public List<string> face_filenames;

    GameObject passport_obj;
    GameObject passport_inner_synth_obj;
    GameObject passport_photo_synth_obj;
    Material cover_mat;
    Material inner_mat_lit;
    Material inner_mat_unlit;
    List<GameObject> instanceList = new List<GameObject>();

    Mesh cover_mesh = null;
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
        //MakejsonTextList(feature_ExtractName);
        CreateSaveFolder();
        MakeExtractFeatureList();
        ResourceLoad();
    }
    void Update()
    {
        if (frame_index == num_CaptureData * 3)
        {
            if (GameObject.Find("AutoSceneManager") == null)
                UnityEditor.EditorApplication.isPlaying = false;
            else
            {
                OnSceneQuit();
            }

        }
        else
        {
            if (frame_index % 3 == 0)
            {
                Seg.BackgroundToNomal();
                Seg.Destroy_All_AttachQuad(feature_names);
                Seg.Destroy_All_AttachQuad(feature_rect);
                Seg.SegLightOff();
                Util.LodingNextFrame();
            }    
            if (frame_index % 3 == 1)
            {
                Util.LodeComplete();
                RandomizePassport();
                //Templete_Property.ChangeToLitShader();
                Background.ChangeBackground(dataSet.background);

                CaptureTool.RandomizeCamera("Inner", CameraMinDistance, CameraMaxDistance, CameraMaxAngle, debug_mode); ;
                SubCard.CreateSubObject(dataSet.cardImage, random_subcard_show_freq);

                Hand.Random_Hand_Pose();
                Hand.Show_Hand(random_hand_show_freq);
                Hand.Lit_Hand_Color();

                Passport_TextChange();

                Effect_Env.TempleteReflectLight_Point(feature_ExtractName);
                Effect_Env.CharShadowOrLight_On(feature_ExtractName, "Inner");
                Effect_Env.StudioBrightness_On();

                useHologram = Hologram.CreateHologram(random_hologram_freq, holo_filenames, instanceList, feature_ExtractName);

                Util.AntiAliasingFunc(true);

                string filename = "rgb_" + (frame_index / 3).ToString() + ".png";
                string path = root_savepath + "/rgb/" + filename;
                ScreenCapture.CaptureScreenshot(path); //모든 렌더링이 끝난 후, 캡처
            }

            if (frame_index % 3 == 2)
            {
                //for Segmentation
                Seg.SegLightOn();
                Seg.BackgroundToBlack();
                Templete_Property.ChangeToSegShader();

                Util.AntiAliasingFunc(false);
                SubCard.DelSubObject();
                Hand.Seg_Hand_Color();

                TextProperty.Ink_smudge_Off(feature_ExtractName);

                Effect_Env.CharShadowOrLight_Off();
                Effect_Env.StudioBrightness_Off();

                if (useHologram == true)
                    Hologram.DestroyHologram(instanceList);

                for (int index = 0; index < feature_rect.Count; index++)
                    Seg.CreateAttachQuad(feature_rect[index], feature_rect_color[index]);

                string filename = "seg_" + (frame_index / 3).ToString() + ".png";
                string path = root_savepath + "/seg/" + filename;
                ScreenCapture.CaptureScreenshot(path);
                Seg.BackgroundToBlack();
            }
        }
        
        frame_index++;
    }
    void OnSceneQuit()
    {
        AutoPassportScript.SceneController.count++;
        AutoPassportScript.SceneController.isRunning = false;
    }
    void AutoSceneSetting()
    {
        num_CaptureData = AutoPassportScript.SceneController.num_CaptureData;
        random_hand_show_freq = AutoPassportScript.SceneController.random_hand_show_freq;
        random_subcard_show_freq = AutoPassportScript.SceneController.random_subcard_show_freq;
        random_hologram_freq = AutoPassportScript.SceneController.random_hologram_freq;
        isFormattedRandomMR = AutoPassportScript.SceneController.isFormattedRandomMR;
    }

    void RandomizePassport()
    {
        // (1) Shape
        int num_cover_files = cover_filenames.Count;
        int cover_file_index = UnityEngine.Random.Range(0, num_cover_files);

        int num_inner_files = inner_filenames.Count;
        int inner_file_index = UnityEngine.Random.Range(0, num_inner_files);

        string cover_path = "Meshes/cover_pages/" + cover_filenames[cover_file_index];
        string inner_path = "Meshes/inner_pages/" + inner_filenames[inner_file_index];
        //string inner_path = "Meshes/inner_pages/" + "passport_inner_0078"; //0024

        CreatePassportShape(cover_path, inner_path);

        Templete_Property.PersonImage.ChangePerson(dataSet.personImage);
        Templete_Property.PersonImage.ChagneTransparent();
        Templete_Property.ChangeColorSaturation();
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

        GameObject cover_obj = new GameObject();
        cover_obj.name = "Cover";
        cover_obj.transform.parent = passport_obj.transform;

        GameObject inner_obj = new GameObject();
        inner_obj.name = "Inner";
        inner_obj.transform.parent = passport_obj.transform;

        //
        MeshFilter cover_mesh_filter = cover_obj.AddComponent<MeshFilter>();
        MeshRenderer cover_mesh_renderer = cover_obj.AddComponent<MeshRenderer>();

        cover_mesh = Resources.Load<Mesh>(cover_path);
        cover_mesh_filter.sharedMesh = cover_mesh;
        cover_mesh_renderer.material = cover_mat;

        //
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
        //
    }
    void CreateSaveFolder()
    {
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
        DataNeeds.passport_inner_object = GameObject.Find(passportInnerSynth);
        DataNeeds.mesh_tess = mesh_tess;
    }
    // 각 여권 종류
    void Passport_TextChange()
    {
        if (!debug_mode)
        {
            string surname;
            string givenname;
            string personal_no;
            // name
            if (GameObject.Find("Name") == null)
            {
                TMP_Text Surname = GameObject.Find("SurName").GetComponent<TMP_Text>();
                TMP_Text GivenName = GameObject.Find("GivenName").GetComponent<TMP_Text>();
                surname = RandomText_Passport.Random_Name(dataSet.surName, Surname);
                givenname = RandomText_Passport.Random_Name(dataSet.givenName, GivenName);
            }
            else
            {
                TMP_Text Name = GameObject.Find("Name").GetComponent<TMP_Text>();
                var name = RandomText_Passport.Random_FullName(dataSet.surName, dataSet.givenName, Name);
                surname = name.Item1;
                givenname = name.Item2;
            }
            // personal number
            if (GameObject.Find("PersonalNo") == null)
            {
                personal_no = "<<<<<<<";
            }
            else
            {
                TMP_Text PersonalNo = GameObject.Find("PersonalNo").GetComponent<TMP_Text>();
                personal_no = RandomText_Passport.Random_PersonalNo(PersonalNo);
            }
            // special case
            if (GameObject.Find("KoreanName") != null)
            {
                TMP_Text KoreaName = GameObject.Find("KoreanName").GetComponent<TMP_Text>();
                RandomText_Passport.Random_Name(dataSet.korName, KoreaName);
            }

            TMP_Text Birth = GameObject.Find("Birth").GetComponent<TMP_Text>();
            TMP_Text Sex = GameObject.Find("Sex").GetComponent<TMP_Text>();
            TMP_Text Issue = GameObject.Find("Issue").GetComponent<TMP_Text>();
            TMP_Text Expiry = GameObject.Find("Expiry").GetComponent<TMP_Text>();
            TMP_Text Type = GameObject.Find("Type").GetComponent<TMP_Text>();
            TMP_Text PassportNo = GameObject.Find("PassportNo").GetComponent<TMP_Text>();
            TMP_Text Mr1 = GameObject.Find("Mr1").GetComponent<TMP_Text>();
            TMP_Text Mr2 = GameObject.Find("Mr2").GetComponent<TMP_Text>();

            string birth = RandomText_Passport.Random_Birth(Birth);
            string sex = RandomText_Passport.Random_Sex(Sex);
            string expiry = RandomText_Passport.Random_Issue_And_Expiry(Issue, Expiry);
            string type = RandomText_Passport.Random_Type(dataSet.type, dataSet.type, Type);
            string passport_no = RandomText_Passport.Random_PassportNo(type, PassportNo);
            string nation = GetSceneType();
            string nationCode = RandomText_Passport.Extract_NationCode(nation);

            //MR - Machine Readable
            if (isFormattedRandomMR)
            {
                RandomText_Passport.Random_Mr1(type, surname, givenname, nationCode, Mr1);
                RandomText_Passport.Random_Mr2(passport_no, nationCode, birth, sex, expiry, personal_no, Mr2);
            }
            else
            {
                RandomText_Passport.All_Random_Mr(Mr1, Mr2);
            }
        }     
    }
    (int, int) TempleteImageSize()
    {
        Texture image = GameObject.Find("PassportInnerSynth").GetComponent<MeshRenderer>().material.mainTexture;
        return (image.width, image.height);
    }
    void MakeSegList()
    {
        for (int i = 0; i < seg_list.Length; i++)
        {
            feature_Image.Add(seg_list[i]);
            feature_rect_color.Add(seg_color_list[i]);
        }
    }
    string GetSceneType()
    {
        string scene_fullName = SceneManager.GetActiveScene().name;
        int underbar_index = scene_fullName.IndexOf("_");
        string scene_type = scene_fullName.Substring(0, underbar_index);
        return scene_type;
    }
    void ResourceLoad()
    {
        cover_filenames = LoadResouce.CollectCoverMeshFilenames();
        inner_filenames = LoadResouce.CollectInnerMeshFilenames();
        face_filenames = LoadResouce.CollectFaceTextureFilenames();
        holo_filenames = LoadResouce.CollectHoloMeshFilenames();

        cover_mat = Resources.Load<Material>("Materials/PassportCoverRenderTextureMaterial");
        inner_mat_lit = Resources.Load<Material>("Materials/PassportRenderTextureMaterial");
        inner_mat_unlit = Resources.Load<Material>("Materials/PassportRenderTextureMaterialUnlit");

        passport_inner_synth_obj = GameObject.Find(passportInnerSynth);
        passport_photo_synth_obj = GameObject.Find("PassportPhotoSynth");

        dataSet.personImage = Resources.LoadAll<Texture>("Faces");
        if(!debug_mode)
            dataSet.background = Resources.LoadAll<Texture>("Background");
        else
            dataSet.background = Resources.LoadAll<Texture>("Background/desk_sample");
        dataSet.cardImage = Resources.LoadAll<Texture>("Subcard");
        dataSet.type = LoadResouce.TextFile("type");
        dataSet.surName = LoadResouce.TextFile("surName");
        dataSet.givenName = LoadResouce.TextFile("givenName");
        dataSet.korName = LoadResouce.TextFile("korName");
    }
}