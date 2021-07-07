using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using System;
using Segmentation_Script;

public class SceneManagerCharacterBased : MonoBehaviour
{
    Vector2 image_size = new Vector2(883, 625);     // (width, height) 여권 이미지의 해상도
    Vector2Int mesh_tess = new Vector2Int(16, 24);  // (x, z) 분할 갯수, Model Shape 생성 시 작성한 파라미터

    //
    [Serializable]
    struct Quad
    {
        public Vector2 left_top;
        public Vector2 right_top;
        public Vector2 left_bottom;
        public Vector2 right_bottom;
    };

    [Serializable]
    struct Segment
    {
        public string name;
        public string text;
        public Quad quad;
    };

    [Serializable]
    class SegmentationInfo
    {
        public string image_filename;

        public List<Segment> segment_list = new List<Segment>();
        /*
        public Quad sur_name_quad;
        public Quad given_name_quad;

        public string sur_name;
        public string given_name;
        */
    };

    [Serializable]
    class SegmentationInfoList
    {
        public List<SegmentationInfo> info_list = new List<SegmentationInfo>();
    };

    SegmentationInfoList segmentation_info_list = new SegmentationInfoList();

    List<string> feature_names = new List<string>
    {
        "SurName", "GivenName", "KoreanName"
    };

    Dictionary<string, List<string>> feature_names_to_texts = new Dictionary<string, List<string>>
    {
        {"SurName", new List<string>{"LEE", "KIM", "PARK", "JUNG","MY NAME IS A"} },
        {"GivenName", new List<string>{"KANG HOON", "HYUN JIN", "SO YOUNG", "ROCK YOU", "JOON HO", "WHERE MYTIME"} },
        {"KoreanName", new List<string>{"이강훈", "박현진", "김소영", "박낙규", "김준호" } }
    };

    List<GameObject> feature_markers = new List<GameObject>();
    List<GameObject> feature_polygons = new List<GameObject>();


    List<string> cover_filenames = new List<string>();
    List<string> inner_filenames = new List<string>();
    List<string> face_filenames = new List<string>();

    GameObject passport_obj = null;
    GameObject passport_cover_synth_obj = null;
    GameObject passport_photo_synth_obj = null;

    Material cover_mat = null;
    Material inner_mat = null;
    Material face_mat = null;

    Mesh cover_mesh = null;
    Mesh inner_mesh = null;

    List<Vector3> inner_vertices = new List<Vector3>();

    int frame_index = 0;

    // Start is called before the first frame update
    void Start()
    {
        CollectCoverMeshFilenames();
        CollectInnerMeshFilenames();
        CollectFaceTextureFilenames();

        //
        cover_mat = Resources.Load<Material>("Materials/KoreaPassportCoverMat");
        inner_mat = Resources.Load<Material>("Materials/PassportRenderTextureMaterial");
        face_mat = Resources.Load<Material>("Materials/FaceMat");

        //
        passport_cover_synth_obj = GameObject.Find("PassportCoverSynth");
        passport_photo_synth_obj = GameObject.Find("PassportPhotoSynth");
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
            if (ext == ".jpg")
            {
                string filename = Path.GetFileNameWithoutExtension(path);
                face_filenames.Add(filename);
            }
        }
    }

    Vector2 CalcImageCoordsInPassport(Vector3 world_coords)
    {
        Bounds passport_cover_bounds = passport_cover_synth_obj.GetComponent<MeshRenderer>().bounds;
        float passport_left = passport_cover_bounds.min.x;
        float passport_right = passport_cover_bounds.max.x;
        float passport_bottom = passport_cover_bounds.min.y;
        float passport_top = passport_cover_bounds.max.y;

        float passport_width = passport_right - passport_left;
        float passport_height = passport_top - passport_bottom;
        float image_width = image_size.x;
        float image_height = image_size.y;

        float scale_x = image_width / passport_width;
        float scale_y = image_height / passport_height;

        float world_x = world_coords.x;
        float world_y = world_coords.y;

        float image_x = (world_x - passport_left) * scale_x;
        float image_y = (passport_top - world_y) * scale_y;

        return new Vector2(image_x, image_y);
    }

    List<Vector2> CalcImageCoordsInPassport(List<Vector3> world_coords_list)
    {
        List<Vector2> image_coords_list = new List<Vector2>();

        foreach (Vector2 world_coords in world_coords_list)
        {
            Vector2 image_coords = CalcImageCoordsInPassport(world_coords);
            image_coords_list.Add(image_coords);
        }

        return image_coords_list;
    }

    Bounds NormalizeFeatureBoundsInPassportBounds(Bounds feature_bounds)
    {
        Bounds passport_cover_bounds = passport_cover_synth_obj.GetComponent<MeshRenderer>().bounds;
        float passport_left = passport_cover_bounds.min.x;
        float passport_right = passport_cover_bounds.max.x;
        float passport_bottom = passport_cover_bounds.min.y;
        float passport_top = passport_cover_bounds.max.y;

        // 
        float passport_width = passport_right - passport_left;
        float passport_height = passport_top - passport_bottom;
        float image_width = image_size.x;
        float image_height = image_size.y;

        float scale_x = image_width / passport_width;
        float scale_y = image_height / passport_height;

        //
        float feature_left = feature_bounds.min.x;
        float feature_right = feature_bounds.max.x;
        float feature_bottom = feature_bounds.min.y;
        float feature_top = feature_bounds.max.y;

        //
        float normalized_left = (feature_left - passport_left) * scale_x;
        float normalized_right = (feature_right - passport_left) * scale_x;
        float normalized_bottom = (feature_bottom - passport_bottom) * scale_y;
        float normalized_top = (feature_top - passport_bottom) * scale_y;

        //
        Vector3 normalized_min = new Vector3(normalized_left, normalized_bottom, 0);
        Vector3 normalized_max = new Vector3(normalized_right, normalized_top, 0);

        Bounds normalized_bounds = new Bounds();
        normalized_bounds.SetMinMax(normalized_min, normalized_max);

        return normalized_bounds;
    }

    List<Vector2> RecalcAllFeaturePoints()
    {
        List<Vector2> all_feature_points = new List<Vector2>();

        foreach (string feature_name in feature_names)
        {
            List<Vector2> feature_points = RecalcFeaturePoints(feature_name);
            all_feature_points.AddRange(feature_points);
        }

        return all_feature_points;
    }

    List<Vector2> RecalcFeaturePoints(string feature_name)
    {
        //        List<Vector3> world_features = CalculateTMPBounds(feature_name);
        List<Vector3> world_features = CalculateTMPBoundsForEachCharacter(feature_name);
        List<Vector2> image_features = CalcImageCoordsInPassport(world_features);

        return image_features;
    }

    // Update is called once per frame
    void Update()
    {
        if (frame_index % 300 == 0)
        {
            RandomizePassport();
            RandomizeCamera();
            RandomizeLight();

//            Seg.CreateAttachQuad("SurName", PresetColor.red);
//            Seg.CreateAttachQuad("GivenName", PresetColor.blue);
//            Seg.CreateAttachQuad("KoreanName", PresetColor.green);

            List<Vector2> feature_points = RecalcAllFeaturePoints();
            //StartCoroutine(Create2DPolygons(feature_points));
            StartCoroutine(Create2DCircles(feature_points));
        }

        if (frame_index % 300 == 1)
        {
            string filename = "passport_synth_" + (frame_index / 300).ToString() + ".png";
            string path = "Assets/TrainingData/" + filename;
            ScreenCapture.CaptureScreenshot(path);

            // Fill the segmentation info with the captured image filename and the quad region for each text field
            SegmentationInfo seg_info = segmentation_info_list.info_list[segmentation_info_list.info_list.Count - 1];
            seg_info.image_filename = filename;

            for (int i = 0; i < seg_info.segment_list.Count; i++)
            {
                Segment segment = seg_info.segment_list[i];
                string feature_name = segment.name;
                segment.quad = CalcQuadForFeature(feature_name);
                seg_info.segment_list[i] = segment;
            }
            /*
            foreach(string feature_name in feature_names)
            {
                Segment segment = seg_info.feature_segment[feature_name];
                segment.quad = CalcQuadForFeature(feature_name);

                seg_info.feature_segment[feature_name] = segment;
            }
            */

            segmentation_info_list.info_list[segmentation_info_list.info_list.Count - 1] = seg_info;
        }

        frame_index++;
    }

    void OnApplicationQuit()
    {
        string json_seg_info_list = UnityEngine.JsonUtility.ToJson(segmentation_info_list,true);

        string filename = "segmention_info_list.json";
        string path = "Assets/TrainingData/" + filename;

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
        int num_face_files = face_filenames.Count;
        int face_file_index = UnityEngine.Random.Range(0, num_face_files);

        string face_path = "Faces/" + face_filenames[face_file_index];

        Texture face_tex = Resources.Load<Texture>(face_path);
        face_mat.SetTexture("_BaseMap", face_tex);

        // (3) Text info
        SegmentationInfo seg_info = new SegmentationInfo();
        segmentation_info_list.info_list.Add(seg_info);

        foreach (string feature_name in feature_names)
        {
            bool is_feature_available = feature_names_to_texts.ContainsKey(feature_name);
            if (is_feature_available)
            {
                List<string> example_texts = feature_names_to_texts[feature_name];
                int num_example_texts = example_texts.Count;

                int example_index = UnityEngine.Random.Range(0, num_example_texts);
                string feature_text = example_texts[example_index];

                GameObject feature_obj = passport_cover_synth_obj.transform.Find(feature_name).gameObject;
                feature_obj.GetComponent<TMP_Text>().SetText(feature_text);

                //
                Segment segment = new Segment();
                segment.name = feature_name;
                segment.text = feature_text;

                seg_info.segment_list.Add(segment);
            }
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

    IEnumerator Create2DPolygons(List<Vector2> feature_points)
    {
        foreach (GameObject polygon in feature_polygons)
        {
            UnityEngine.Object.Destroy(polygon);
        }
        feature_polygons.Clear();

        //
        int num_quads = feature_points.Count / 4;

        for (int q = 0; q < num_quads; q++)
        {
            List<Vector3> poly_verts = new List<Vector3>();
            List<int> indices = new List<int> { 0, 1, 3, 2, 0 };

            Vector3 center = Vector3.zero;
            for (int i = 0; i < 5; i++)
            {
                Vector2 point = feature_points[q * 4 + indices[i]];

                Vector2 tex_coords = CalcTextureCoordsFromImageCoords(point);
                Vector3 model_coords = CalcModelCoordsFromTextureCoords(tex_coords);
                Vector3 world_coords = CalcWorldCoordsFromModelCoords(model_coords);
                Vector2 screen_coords = CalcScreenCoordsFromWorldCoords(world_coords);

                Vector3 vert = new Vector3(screen_coords.x, screen_coords.y, 0f);
                poly_verts.Add(vert);
            }

            GameObject canvas = GameObject.Find("Canvas");
            GameObject quad_prefab = Resources.Load("Prefabs/Quad") as GameObject;
            GameObject quad_instance = GameObject.Instantiate(quad_prefab, canvas.transform);
            feature_polygons.Add(quad_instance);
        }

        yield return null;

        for(int q=0; q < num_quads; q++)
        {
            List<Vector3> poly_verts = new List<Vector3>();
            List<int> indices = new List<int> { 0, 1, 3, 2, 0 };

            Vector3 center = Vector3.zero;
            for (int i = 0; i < 5; i++)
            {
                Vector2 point = feature_points[q * 4 + indices[i]];

                Vector2 tex_coords = CalcTextureCoordsFromImageCoords(point);
                Vector3 model_coords = CalcModelCoordsFromTextureCoords(tex_coords);
                Vector3 world_coords = CalcWorldCoordsFromModelCoords(model_coords);
                Vector2 screen_coords = CalcScreenCoordsFromWorldCoords(world_coords);

                Vector3 vert = new Vector3(screen_coords.x, screen_coords.y, 0f);
                poly_verts.Add(vert);
            }

            GameObject quad_instance = feature_polygons[q];
            Shapes2D.Shape poly = quad_instance.GetComponent<Shapes2D.Shape>();
            poly.settings.shapeType = Shapes2D.ShapeType.Polygon;
            //            poly.settings.fillColor = new Color(230.0f / 255.0f, 126.0f / 255.0f, 34.0f / 255.0f, 0.5f);
            poly.SetPolygonWorldVertices(poly_verts.ToArray());
        }

        yield return null;
    }

    IEnumerator Create2DCircles(List<Vector2> feature_points)
    {
        foreach (GameObject polygon in feature_polygons)
        {
            UnityEngine.Object.Destroy(polygon);
        }
        feature_polygons.Clear();

        //
        List<Vector3> quad_centers = new List<Vector3>();
        List<float> quad_radii = new List<float>();
        float quad_radius = 0.0f;

        int num_quads = feature_points.Count / 4;
        for (int q = 0; q < num_quads; q++)
        {
            List<Vector3> quad_verts = new List<Vector3>();

            Vector3 center = Vector3.zero;
            for (int i = 0; i < 4; i++)
            {
                Vector2 point = feature_points[q * 4 + i];

                Vector2 tex_coords = CalcTextureCoordsFromImageCoords(point);
                Vector3 model_coords = CalcModelCoordsFromTextureCoords(tex_coords);
                Vector3 world_coords = CalcWorldCoordsFromModelCoords(model_coords);
                Vector2 screen_coords = CalcScreenCoordsFromWorldCoords(world_coords);

                Vector3 vert = new Vector3(screen_coords.x, screen_coords.y, 0f);
                center += vert;

                quad_verts.Add(vert);
            }
            center /= 4.0f;
            quad_centers.Add(center);

            float max_radius = 0.0f;
            for (int i = 0; i < 4; i++)
            {
                float radius = (quad_verts[i] - center).magnitude;
                if (radius > max_radius)
                {
                    max_radius = radius;
                }
            }
            quad_radii.Add(max_radius);

            if (max_radius > quad_radius)
            {
                quad_radius = max_radius;
            }
        }

        yield return null;

        //
        quad_radius /= Camera.main.pixelWidth;
        quad_radius *= 1.25f;

        GameObject canvas = GameObject.Find("Canvas");
        GameObject circle_prefab = Resources.Load("Prefabs/Circle") as GameObject;

        for (int q = 0; q < num_quads; q++)
        {
            if(quad_radii[q] < 1e-04f)
            {
                continue;
            }

            GameObject circle_instance = GameObject.Instantiate(circle_prefab, canvas.transform);
            feature_polygons.Add(circle_instance);

            Shapes2D.Shape poly = circle_instance.GetComponent<Shapes2D.Shape>();
            poly.settings.shapeType = Shapes2D.ShapeType.Ellipse;
            poly.transform.position = quad_centers[q];
            poly.transform.localScale = new Vector3(quad_radius, Camera.main.aspect * quad_radius);
        }

        yield return null;
    }

    int GetVertexIndex(int i, int j)
    {
        return i * (mesh_tess.y + 1) + j;
    }

    Vector3 GetVertex(int i, int j)
    {
        int index = GetVertexIndex(i, j);
        return GetVertex(index);
    }

    Vector3 GetVertex(int index)
    {
        if (inner_vertices != null)
        {
            return inner_vertices[index];
        }
        else
        {
            return Vector3.zero;
        }
    }

    Vector2 CalcTextureCoordsFromImageCoords(Vector2 image_coords)
    {
        float tu = image_coords.x / image_size.x;
        float tv = image_coords.y / image_size.y;

        //        return new Vector2(tu, tv);
        return new Vector2(tv, 1 - tu);
    }

    Vector3 CalcModelCoordsFromTextureCoords(Vector2 texture_coords)
    {
        float du = 1.0f / (float)mesh_tess.x;
        float dv = 1.0f / (float)mesh_tess.y;

        float tu = texture_coords.x;
        float tv = texture_coords.y;

        int i0 = (int)(tu / du);
        int j0 = (int)(tv / dv);

        if (i0 > mesh_tess.x - 1) i0 = mesh_tess.x - 1;
        if (j0 > mesh_tess.y - 1) j0 = mesh_tess.y - 1;

        int i1 = (i0 < mesh_tess.x - 1 ? i0 + 1 : i0);
        int j1 = (j0 < mesh_tess.y - 1 ? j0 + 1 : j0);

        //
        Vector3 p_O = GetVertex(i0, j0);
        Vector3 p_U = GetVertex(i1, j0);
        Vector3 p_V = GetVertex(i0, j1);

        //
        float u = (tu - i0 * du) / du;
        float v = (tv - j0 * dv) / dv;

        Vector3 p_u = Vector3.Lerp(p_O, p_U, u);
        Vector3 p_v = Vector3.Lerp(p_O, p_V, v);

        //
        Vector3 p_M = p_O + (p_u - p_O) + (p_v - p_O);
        return p_M;
    }

    Vector3 CalcWorldCoordsFromModelCoords(Vector3 model_coords)
    {
        return passport_obj.transform.TransformPoint(model_coords);
    }

    Vector2 CalcScreenCoordsFromWorldCoords(Vector3 world_coords)
    {
        Vector3 screen_coords = Camera.main.WorldToScreenPoint(world_coords);
        return new Vector2(screen_coords.x, screen_coords.y);
    }

    Vector2 CalcImageCoordsFromScreenCoords(Vector2 screen_coords)
    {
        Vector2 image_coords = new Vector2(screen_coords.x, Camera.main.pixelHeight - screen_coords.y);
        return image_coords;
    }

    Vector2 CalcScreenCoordsFromImageCoords(Vector2 image_coords)
    {
        Vector2 tex_coords = CalcTextureCoordsFromImageCoords(image_coords);
        Vector3 model_coords = CalcModelCoordsFromTextureCoords(tex_coords);
        Vector3 world_coords = CalcWorldCoordsFromModelCoords(model_coords);
        Vector2 screen_coords = CalcScreenCoordsFromWorldCoords(world_coords);

        return screen_coords;
    }

    Vector2 CalcImageCoordsFromImageCoords(Vector2 input_image_coords)
    {
        Vector2 screen_coords = CalcScreenCoordsFromImageCoords(input_image_coords);
        Vector2 output_image_coords = CalcImageCoordsFromScreenCoords(screen_coords);

        return output_image_coords;
    }

    Vector2 CalcImageCoordsFromTextureCoords(Vector2 input_tex_coords)
    {
        Vector3 model_coords = CalcModelCoordsFromTextureCoords(input_tex_coords);
        Vector3 world_coords = CalcWorldCoordsFromModelCoords(model_coords);
        Vector2 screen_coords = CalcScreenCoordsFromWorldCoords(world_coords);
        Vector2 output_image_coords = CalcImageCoordsFromScreenCoords(screen_coords);

        return output_image_coords;
    }

    Quad CalcQuadForIthField(int i)
    {
        string feature_name = feature_names[i];
        Quad quad = CalcQuadForFeature(feature_name);

        return quad;
    }

    Quad CalcQuadForFeature(string feature_name)
    {
        List<Vector2> feature_points = RecalcFeaturePoints(feature_name);

        Vector2 left_top = CalcImageCoordsFromImageCoords(feature_points[0]);
        Vector2 right_top = CalcImageCoordsFromImageCoords(feature_points[1]);
        Vector2 left_bottom = CalcImageCoordsFromImageCoords(feature_points[2]);
        Vector2 right_bottom = CalcImageCoordsFromImageCoords(feature_points[3]);

        Quad quad = new Quad();
        quad.left_top = left_top;
        quad.right_top = right_top;
        quad.left_bottom = left_bottom;
        quad.right_bottom = right_bottom;

        return quad;
    }

    public List<Vector3> CalculateTMPBounds(string TMPObjectName)
    {
        TMP_Text textComponent = GameObject.Find(TMPObjectName).GetComponent<TMP_Text>();

        Vector3 passportScale;
        if (textComponent.transform.parent == null)
            passportScale = new Vector3(1, 1, 1);
        else
            passportScale = textComponent.transform.parent.localScale;

        Vector3 textScale = textComponent.rectTransform.localScale;
        Vector3 scale = new Vector3(passportScale.x * textScale.x, passportScale.y * textScale.y, passportScale.z * textScale.z);

        textComponent.ForceMeshUpdate();
        var textInfo = textComponent.textInfo;
        var charInfos = textInfo.characterInfo;

        float max_y = 0;
        float min_y = 0;
        float min_x = textInfo.characterInfo[0].vertex_BL.position.x;
        float max_x = textInfo.characterInfo[textInfo.characterCount - 1].vertex_BR.position.x;

        foreach (var charinfo in charInfos)
        {
            if (min_y > charinfo.vertex_BL.position.y)
                min_y = charinfo.vertex_BL.position.y;
            if (max_y < charinfo.vertex_TL.position.y)
                max_y = charinfo.vertex_TL.position.y;
        }

        Vector3 bot_left = new Vector3(min_x * scale.x, min_y * scale.y, 0) + textComponent.transform.position;
        Vector3 top_left = new Vector3(min_x * scale.x, max_y * scale.y, 0) + textComponent.transform.position;
        Vector3 top_right = new Vector3(max_x * scale.x, max_y * scale.y, 0) + textComponent.transform.position;
        Vector3 bot_right = new Vector3(max_x * scale.x, min_y * scale.y, 0) + textComponent.transform.position;

        List<Vector3> tmp_bounds = new List<Vector3> { bot_left, bot_right, top_left, top_right };
        return tmp_bounds;
    }

    public List<Vector3> CalculateTMPBoundsForEachCharacter(string TMPObjectName)
    {
        TMP_Text textComponent = GameObject.Find(TMPObjectName).GetComponent<TMP_Text>();

        textComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = textComponent.textInfo;
        TMP_CharacterInfo[] charInfos = textInfo.characterInfo;

        List<Vector3> tmp_bounds_for_each_char = new List<Vector3>();

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = charInfos[i];

            float left = charInfo.vertex_TL.position.x;
            float top = charInfo.vertex_TL.position.y;

            float right = charInfo.vertex_BR.position.x;
            float bottom = charInfo.vertex_BR.position.y;

            List<Vector3> tmp_bounds = new List<Vector3> {
                new Vector3(left, bottom, 0.0f),
                new Vector3(right, bottom, 0.0f),
                new Vector3(left, top, 0.0f),
                new Vector3(right, top, 0.0f),
            };

            foreach (Vector3 local_p in tmp_bounds)
            {
                Vector3 global_p = textComponent.transform.TransformPoint(local_p);
                tmp_bounds_for_each_char.Add(global_p);
            }
        }

        return tmp_bounds_for_each_char;
    }
}
