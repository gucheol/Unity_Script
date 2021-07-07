using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using System;
using Segmentation_Script;
using System.Linq;
using BoundingBox_Script;
//using Randomize_Custom;

namespace Main {
    public class SceneManager : MonoBehaviour
    {
        public static Vector2 image_size = new Vector2(883, 625);     // (width, height) 여권 이미지의 해상도
        public static Vector2Int mesh_tess = new Vector2Int(16, 24);  // (x, z) 분할 갯수, Model Shape 생성 시 작성한 파라미터

        //should Setting
        public string nation = "Korea";

        List<string> feature_ExtractName = new List<string>
        {
            "GivenName", "KoreanName"
        };
        List<float[]> feature_color = new List<float[]>
        {
            PresetColor.blue, PresetColor.green
        };

        public List<string> feature_Image = new List<string>()// Not Text
        {
            "PassportCoverSynth", "PassportPhotoSynth"
        };
        public List<float[]> feature_rect_color = new List<float[]>
        {
            PresetColor.gray, PresetColor.brown
        };
        private string passportCoverSynth = "Korea_PassportCoverSynth";
        //

        public static List<string> feature_names = new List<string>();      
        public static List<string> feature_rect = new List<string>();
        public string root_savepath;

        public static GameObject passport_Object;

        public struct DataSet
        {
            public Texture[] personImage;
            public Texture[] background;
            public List<string> type;
            public List<string> surName;
            public List<string> givenName;
            public List<string> korName;
        };
        DataSet dataSet = new DataSet();
        //
        [Serializable]
        public struct Quad
        {
            public Vector2 left_top;
            public Vector2 right_top;
            public Vector2 left_bottom;
            public Vector2 right_bottom;
        };

        [Serializable]
        public struct Segment
        {
            public string name;
            public string text;
            public Quad quad;
        };

        [Serializable]
        public class SegmentationInfo
        {
            public string image_filename;
            public List<Segment> segment_list = new List<Segment>();
        };

        [Serializable]
        public class SegmentationInfoList
        {
            public List<SegmentationInfo> info_list = new List<SegmentationInfo>();
        };

        public static SegmentationInfoList segmentation_info_list = new SegmentationInfoList();

        public  List<string> cover_filenames;
        public  List<string> inner_filenames;
        public  List<string> face_filenames = new List<string>();
        
        public static GameObject passport_obj = null;
        public static GameObject passport_cover_synth_obj = null;
        GameObject passport_photo_synth_obj = null;

        public Material cover_mat = null;
        public Material inner_mat = null;
        public Material face_mat = null;

        public Mesh cover_mesh;
        public Mesh inner_mesh = null;
        
        public static List<Vector3> inner_vertices = new List<Vector3>();

        int frame_index = 0;

        // Start is called before the first frame update
        void Start()
        {
            passport_Object = GameObject.Find(nation + "_PassportCoverSynth");

            CreateSaveFolder();
            MakeExtractFeatureList();

            CollectCoverMeshFilenames();
            CollectInnerMeshFilenames();
            CollectFaceTextureFilenames();
            

            cover_mat = Resources.Load<Material>("Materials/KoreaPassportCoverMat");
            inner_mat = Resources.Load<Material>("Materials/PassportRenderTextureMaterial");
            face_mat = Resources.Load<Material>("Materials/FaceMat");

            //
            passport_cover_synth_obj = GameObject.Find(passportCoverSynth);
            passport_photo_synth_obj = GameObject.Find("PassportPhotoSynth");


            dataSet.personImage = Resources.LoadAll<Texture>("Faces");
            dataSet.background = Resources.LoadAll<Texture>("Background");
            dataSet.type = load_TextFile("type");
            dataSet.surName = load_TextFile("surName");
            dataSet.givenName = load_TextFile("givenName");
            dataSet.korName = load_TextFile("korName");
            
        }
        
        void CollectCoverMeshFilenames()
        {
            string cover_path = Application.dataPath + "/Resources/Meshes/cover_pages/";
            string[] cover_paths = Directory.GetFiles(cover_path);
            foreach (string path in cover_paths)
            {
                string ext = Path.GetExtension(path);
                if (ext == ".obj")
                {
                    string filename = Path.GetFileNameWithoutExtension(path);
                    cover_filenames.Add(filename);
                }
            }
        }

        void CollectInnerMeshFilenames()
        {
            string inner_path = Application.dataPath + "/Resources/Meshes/inner_pages/";
            string[] inner_paths = Directory.GetFiles(inner_path);
            foreach (string path in inner_paths)
            {
                string ext = Path.GetExtension(path);
                if (ext == ".obj")
                {
                    string filename = Path.GetFileNameWithoutExtension(path);
                    inner_filenames.Add(filename);
                }
            }
        }

        void CollectFaceTextureFilenames()
        {
            string face_path = Application.dataPath + "/Resources/Faces/";
            string[] face_paths = Directory.GetFiles(face_path);
            foreach (string path in face_paths)
            {
                string ext = Path.GetExtension(path);
                if (ext == ".png")
                {
                    string filename = Path.GetFileNameWithoutExtension(path);
                    face_filenames.Add(filename);
                }
            }
        }
        
        void Update()
        {
            if(frame_index % 3 ==0 )
                Seg.BackgroundToNomal();

            if (frame_index % 3 == 1)
            {
                Passport_Kor();

                RandomizePassport();
                RandomizeCamera();
                RandomizeLight();

                Destroy_All_AttachQuad(feature_names);
                Destroy_All_AttachQuad(feature_rect);
                //Destroy_AttachQuad("PassportPhotoSynth");

                string filename = "rgb_" + (frame_index / 3).ToString() + ".png";
                string path = root_savepath + "/rgb/" + filename;
                ScreenCapture.CaptureScreenshot(path); //모든 렌더링이 끝난 후, 캡처

                /*
                Seg.CreateAttachQuad("SurName", PresetColor.red);
                Seg.CreateAttachQuad("GivenName", PresetColor.blue);
                Seg.CreateAttachQuad("KoreanName", PresetColor.green);
                */
                //List<Vector2> feature_points = CalcCoord.RecalcAllFeaturePoints();
                
            }

            if (frame_index % 3 == 2)
            {
                for (int index = 0; index < feature_names.Count; index++)
                {
                    Seg.CreateAttachQuad(feature_names[index], feature_color[index]);
                }

                for (int index = 0; index < feature_rect.Count; index++)
                {
                    Seg.CreateAttachQuad(feature_rect[index], feature_rect_color[index]);
                }

                //Seg.CreateAttachQuad("PassportPhotoSynth", PresetColor.brown);
                
                //Seg.BackgroundToBlack("Background");

                string filename = "seg_" + (frame_index / 3).ToString() + ".png";
                //string path = "Assets/TrainingData/" + filename;
                string path = root_savepath + "/seg/" + filename;
                ScreenCapture.CaptureScreenshot(path);

                // Fill the segmentation info with the captured image filename and the quad region for each text field
                SegmentationInfo seg_info = segmentation_info_list.info_list[segmentation_info_list.info_list.Count - 1];
                seg_info.image_filename = filename;

                List<int> removeIndex = new List<int>();
                for (int i = 0; i < seg_info.segment_list.Count; i++)
                {
                    Segment segment = seg_info.segment_list[i];
                    string feature_name = segment.name;
                    //segment.quad = CalcCoord.CalcQuadForFeature(feature_name);


                    if (segment.quad.left_bottom == segment.quad.right_top) //feature가 화면 밖으로 나갔을때
                        removeIndex.Add(i);
                    else
                        seg_info.segment_list[i] = segment;
                }
                removeIndex.Reverse();
                foreach (int index in removeIndex)
                {
                    seg_info.segment_list.RemoveAt(index);
                }

                segmentation_info_list.info_list[segmentation_info_list.info_list.Count - 1] = seg_info;

                Seg.BackgroundToBlack();      

            }
            frame_index++;
        }

        void OnApplicationQuit()
        {

            string json_seg_info_list = UnityEngine.JsonUtility.ToJson(segmentation_info_list);
            string filename = "BoundingBox.json";
            //string path = "Assets/TrainingData/" + filename;
            string path = root_savepath + "/" + filename;

            System.IO.File.WriteAllText(path, json_seg_info_list);
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

            CreatePassportShape(cover_path, inner_path);

            // (2) Photo info
            int random_index = UnityEngine.Random.Range(0, dataSet.personImage.Length);
            Texture personImage = dataSet.personImage[random_index];
            MeshRenderer personImage_MeshRenderer = GameObject.Find(nation + "_PassportPhotoSynth").GetComponent<MeshRenderer>();
            personImage_MeshRenderer.material.SetTexture("_BaseMap", personImage);

            random_index = UnityEngine.Random.Range(0, dataSet.personImage.Length);
            Texture background = dataSet.background[random_index];
            MeshRenderer background_MeshRenderer = GameObject.Find("Background").GetComponent<MeshRenderer>();
            background_MeshRenderer.material.SetTexture("_BaseMap", background);
            /*
            int num_face_files = face_filenames.Count;
            int face_file_index = UnityEngine.Random.Range(0, num_face_files);

            string face_path = "Faces/" + face_filenames[face_file_index];

            Texture face_tex = Resources.Load<Texture>(face_path);
            
            face_mat.SetTexture("_BaseMap", face_tex);
            */
            // (3) Text info
            SegmentationInfo seg_info = new SegmentationInfo();
            segmentation_info_list.info_list.Add(seg_info);

            foreach (string feature_name in feature_names)
            {
                Segment segment = new Segment();
                segment.name = feature_name;
                seg_info.segment_list.Add(segment);
                /*
                bool is_feature_available = feature_names_to_texts.ContainsKey(feature_name);
                if (is_feature_available)
                {
                    List<string> example_texts = feature_names_to_texts[feature_name];
                    int num_example_texts = example_texts.Count;

                    int example_index = UnityEngine.Random.Range(0, num_example_texts);
                    string feature_text = example_texts[example_index];

                    GameObject feature_obj = passport_cover_synth_obj.transform.Find(feature_name).gameObject;
                    feature_obj.GetComponent<TMP_Text>().SetText(feature_text);


                    Segment segment = new Segment();
                    segment.name = feature_name;
                    segment.text = feature_text;

                    seg_info.segment_list.Add(segment);

                }
                */
            }
        }

        Vector3 RandomPointInBounds(Bounds bounds)
        {
            return new Vector3(
                UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                UnityEngine.Random.Range(bounds.min.y, bounds.max.y),
                UnityEngine.Random.Range(bounds.min.z, bounds.max.z)
            );
        }

        Vector3 RandomPointInHemisphere(Vector3 center, float min_radius, float max_radius)
        {
            Vector3 V = UnityEngine.Random.insideUnitSphere;
            V.Set(V.x, Mathf.Abs(V.y), V.z);

            float r = UnityEngine.Random.Range(min_radius, max_radius);
            Vector3 p = center + r * V;

            return p;
        }

        Vector3 RandomPointInCone(Vector3 center, Vector3 dir, float max_angle, float min_radius, float max_radius)
        {
            Vector3 U = RandomDirectionInCone(dir, max_angle);

            float r = UnityEngine.Random.Range(min_radius, max_radius);
            Vector3 p = center + r * U;

            return p;
        }

        Vector3 RandomDirectionInCone(Vector3 dir, float max_angle)
        {
            Vector3 V = UnityEngine.Random.insideUnitSphere;
            Vector3 N = dir.normalized;

            Vector3 axis = Vector3.ProjectOnPlane(V, N);
            float angle = UnityEngine.Random.Range(0.0f, max_angle);

            Quaternion rot_quat = Quaternion.AngleAxis(angle, axis);
            Vector3 U = rot_quat * N;

            return U;
        }

        Vector3 RandomUnitVectorPerpendicularTo(Vector3 n)
        {
            Vector3 N = n.normalized;
            Vector3 V = UnityEngine.Random.insideUnitSphere;
            Vector3 P = Vector3.ProjectOnPlane(V, N).normalized;

            return P;
        }

        void RandomizeCamera()
        {
            GameObject inner_obj = passport_obj.transform.Find("Inner").gameObject;
            MeshRenderer inner_renderer = inner_obj.GetComponent<MeshRenderer>();
            Bounds inner_bounds = inner_renderer.bounds;
            Bounds target_bounds = new Bounds(inner_bounds.center, inner_bounds.size * 0.5f);

            float camera_min_dist = 5.0f;
            float camera_max_dist = 20.0f;
            float camera_max_angle = Mathf.PI / 6.0f;

            // (1) Target position
            Vector3 target_position = RandomPointInBounds(target_bounds);

            // (2) Camera position
            Vector3 camera_position = RandomPointInCone(target_position, Vector3.up, camera_max_angle, camera_min_dist, camera_max_dist);

            // (3) Up vector
            Vector3 camera_up = RandomUnitVectorPerpendicularTo(target_position - camera_position);

            // (4) Set
            Camera.main.transform.position = camera_position;
            Camera.main.transform.LookAt(target_position, camera_up);
        }

        void RandomizeLight()
        {
            GameObject point_light = GameObject.Find("Point Light");
            GameObject directional_light = GameObject.Find("Directional Light");

            GameObject lightbox_obj = GameObject.Find("LightBox");
            BoxCollider lightbox_collider = lightbox_obj.GetComponent<BoxCollider>();
            Bounds lightbox_bounds = lightbox_collider.bounds;

            // (1) Point light position
            Vector3 p = RandomPointInBounds(lightbox_bounds);
            point_light.transform.position = p;

            // (2) Directional light direction
            Vector3 d = RandomDirectionInCone(Vector3.up, Mathf.PI / 4.0f);
            directional_light.transform.rotation = Quaternion.FromToRotation(Vector3.back, d);
        }

        GameObject CreatePassportShape(string cover_path, string inner_path)
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
            inner_mesh_renderer.material = inner_mat;

            //
            inner_vertices.Clear();
            inner_mesh.GetVertices(inner_vertices);

            //
            return passport_obj;
        }
        List<string> load_TextFile(string fileName)
        {
            TextAsset dataSet = Resources.Load<TextAsset>($"TextDataSet/{fileName}");
            StringReader type_sr = new StringReader(dataSet.text);
            string line = type_sr.ReadLine();
            List<string> textData = new List<string>();
            while (line != null)
            {
                textData.Add(line);
                line = type_sr.ReadLine();
            }
            type_sr.Close();
            return textData;
        }
        
        void Passport_Kor()
        {
            TMP_Text Korea_Surname = passport_Object.gameObject.transform.Find("SurName").gameObject.GetComponent<TMP_Text>();
            //TMP_Text Korea_Surname = GameObject.Find("Korea_SurName").GetComponent<TMP_Text>();
            TMP_Text Korea_GivenName = GameObject.Find("Korea_GivenName").GetComponent<TMP_Text>();
            TMP_Text Korea_Birth = GameObject.Find("Korea_DateOfBirth").GetComponent<TMP_Text>();
            TMP_Text Korea_Sex = GameObject.Find("Korea_Sex").GetComponent<TMP_Text>();
            TMP_Text Korea_Issue = GameObject.Find("Korea_DateOfIssue").GetComponent<TMP_Text>();
            TMP_Text Korea_Expiry = GameObject.Find("Korea_DateOfExpiry").GetComponent<TMP_Text>();
            TMP_Text Korea_Type = GameObject.Find("Korea_Type").GetComponent<TMP_Text>();
            TMP_Text Korea_PassportNo = GameObject.Find("Korea_PassportNo").GetComponent<TMP_Text>();
            TMP_Text Korea_PersonalNo = GameObject.Find("Korea_PersonalNo").GetComponent<TMP_Text>();
            TMP_Text Korea_KoreaName = GameObject.Find("Korea_KoreanName").GetComponent<TMP_Text>();
            TMP_Text Korea_Mr1 = GameObject.Find("Korea_Mr1").GetComponent<TMP_Text>();
            TMP_Text Korea_Mr2 = GameObject.Find("Korea_Mr2").GetComponent<TMP_Text>();


            string surname = Random_Name(dataSet.surName, Korea_Surname);
            string givenname = Random_Name(dataSet.givenName, Korea_GivenName);
            string birth = Random_Birth(Korea_Birth);
            string sex = Random_Sex(Korea_Sex);
            string expiry = Random_Issue_And_Expiry(Korea_Issue, Korea_Expiry);
            string type = Random_Type(dataSet.type, Korea_Type);
            string passport_no = Random_PassportNo(type, Korea_PassportNo);
            string personal_no = Random_PersonalNo(Korea_PersonalNo);
            Random_Name(dataSet.korName, Korea_KoreaName);
            Random_Mr1(type, surname, givenname, "KOR", Korea_Mr1);
            Random_Mr2(passport_no, birth, sex, expiry, personal_no, Korea_Mr2);
        }

        string Random_Name(List<string> dataSet, TMP_Text textObject)
        {
            int random_index = UnityEngine.Random.Range(0, dataSet.Count);
            string name_string = dataSet[random_index];
            textObject.SetText(name_string);
            return name_string;
        }
        string Random_Birth(TMP_Text textObject) //birth
        {
            int year = UnityEngine.Random.Range(40, 100);
            int month = UnityEngine.Random.Range(1, 13);
            int day;
            string[] month_DataSet = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

            if (month == 2)
                day = UnityEngine.Random.Range(1, 30);
            else if (month == 2 || month == 4 || month == 6 || month == 9 || month == 11)
                day = UnityEngine.Random.Range(1, 31);
            else
                day = UnityEngine.Random.Range(1, 32);

            string birth_string = string.Format("{0:00}. {1}. {2:0000}", day, month_DataSet[month - 1], 1900 + year);
            textObject.SetText(birth_string);

            birth_string = string.Format("{0:00}{1:00}{2:00}", year, month, day);
            return birth_string;
        }
        string Random_Sex(TMP_Text textObject)
        {
            int random_index = UnityEngine.Random.Range(0, 2);
            string sex;
            if (random_index == 1)
                sex = "F";
            else
                sex = "M";
            textObject.SetText(sex);
            return sex;
        }
        string Random_Issue_And_Expiry(TMP_Text textObject1, TMP_Text textObject2) //issue, expiry
        {
            int year = UnityEngine.Random.Range(0, 20);
            int month = UnityEngine.Random.Range(1, 13);
            int day;
            string[] month_DataSet = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

            if (month == 2)
                day = UnityEngine.Random.Range(1, 30);
            else if (month == 2 || month == 4 || month == 6 || month == 9 || month == 11)
                day = UnityEngine.Random.Range(1, 31);
            else
                day = UnityEngine.Random.Range(1, 32);

            string issue_string = string.Format("{0:00}. {1}. {2:00}", day, month_DataSet[month - 1], 2000 + year);
            textObject1.SetText(issue_string);
            year += 10;
            string expiry_string = string.Format("{0:00}. {1}. {2:00}", day, month_DataSet[month - 1], 2000 + year);
            textObject2.SetText(expiry_string);

            expiry_string = string.Format("{0:00}{1:00}{2:00}", year, month, day);
            return expiry_string;
        }
        string Random_Type(List<string> Texts, TMP_Text textObject)
        {
            int random_index = UnityEngine.Random.Range(0, Texts.Count);
            string type = dataSet.type[random_index];
            textObject.SetText(type);
            return type;
        }

        string Random_PassportNo(string type, TMP_Text textObject)
        {
            int random_num = UnityEngine.Random.Range(0, 100000000);
            string passportNo_string = string.Format("{0}{1:00000000}", type.Substring(1), random_num);
            textObject.SetText(passportNo_string);
            return passportNo_string;
        }
        string Random_PersonalNo(TMP_Text textObject)
        {
            int random_num = UnityEngine.Random.Range(0, 9999999);
            string personalNo_string = string.Format("{0:0000000}", random_num);
            textObject.SetText(personalNo_string);
            return personalNo_string;
        }
        void Random_Mr1(string type, string surname, string given, string nation, TMP_Text mr1)
        {
            string name = surname + "<<" + given;
            name = name.Replace("-", "<");
            if (name.Length > 40)
            {
                name = name.Substring(0, 40);
            }
            else
            {
                for (int i = name.Length; i < 39; i++)
                {
                    name = name.Insert(name.Length, "<");
                }
            }
            string tmp_string = string.Format("{0:<<}{1:<<<}{2:<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<}", type, nation, name);
            mr1.SetText(tmp_string);
        }
        void Random_Mr2(string Passport_no, string birth, string sex, string expiry, string Personal_no, TMP_Text mr2)
        {
            string tmp_string = string.Format("{0:<<<<<<<<<}{1}{2:<<<}{3:<<<<<<}{4}{5}{6:<<<<<<}{7}{8:<<<<<<<<}{9}{10:000000}{11}{12}",
                Passport_no, UnityEngine.Random.Range(0, 10), "KOR", birth, UnityEngine.Random.Range(0, 10), sex, expiry, UnityEngine.Random.Range(0, 10), Personal_no, "V", UnityEngine.Random.Range(0, 1000000), UnityEngine.Random.Range(0, 10), UnityEngine.Random.Range(0, 10));
            mr2.SetText(tmp_string);
        }

        void CreateSaveFolder()
        {
            string folder_guid = Guid.NewGuid().ToString();
            root_savepath = Application.persistentDataPath + "/" + folder_guid;

            DirectoryInfo root_di = new DirectoryInfo(root_savepath);
            root_di.Create();

            string rgb_savepath = root_savepath + "/rgb";
            DirectoryInfo rgb_di = new DirectoryInfo(rgb_savepath);
            rgb_di.Create();

            string seg_savepath = root_savepath + "/seg";
            DirectoryInfo seg_di = new DirectoryInfo(seg_savepath);
            seg_di.Create();

        }
        void Destroy_All_AttachQuad(List<string> feature_names)
        {
            foreach(string feature_name in feature_names)
            {
                Destroy_AttachQuad(feature_name);
            }
            
        }
        void Destroy_AttachQuad(string feature_name)
        {
            string attachQuad_name = feature_name + "_attachQuad";
            GameObject attachObject = GameObject.Find(attachQuad_name);
            if (attachObject != null)
                UnityEngine.Object.Destroy(attachObject);
        }
        void MakeExtractFeatureList()
        {
            foreach(string feature in feature_ExtractName)
            {
                string feature_name = nation + "_" + feature;
                feature_names.Add(feature_name);
            }
            foreach(string feature in feature_Image)
            {
                string feature_name = nation + "_" + feature;
                feature_rect.Add(feature_name);
            }
        }
    }
    
}
