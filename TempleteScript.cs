using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Segmentation_Script;
using BoundingBox_Script;
using TMPro;
using UnityEngine.SceneManagement;

namespace Object_Script
{
    public static class Templete_Property
    {
        public static class PersonImage
        {
            public static void ChangePerson(Texture[] personImages)
            {
                Material personImage_Material = GameObject.Find("PhotoSynth").GetComponent<MeshRenderer>().material;
                int random_index = UnityEngine.Random.Range(0, personImages.Length);
                Texture personImage = personImages[random_index];
                personImage_Material.SetTexture("_MainTex", personImage);
                // 투명한 사진이 같이 있을 경우
                if (GameObject.Find("SidePhotoSynth") != null)
                {
                    Material waterMarkImage_Material = GameObject.Find("SidePhotoSynth").GetComponent<MeshRenderer>().material;
                    waterMarkImage_Material.SetTexture("_MainTex", personImage);
                }
            }
            public static void ChangeGrayScale(float random_personImage_grayscale_freq)
            {
                Material personImage_Material = GameObject.Find("PhotoSynth").GetComponent<MeshRenderer>().material;
                float random_grayscale = UnityEngine.Random.Range(0, 1.0f);
                if (random_grayscale < random_personImage_grayscale_freq)
                {
                    personImage_Material.SetFloat("_Saturation", 0);
                    if (GameObject.Find("SidePhotoSynth") != null)
                    {
                        Material waterMarkImage_Material = GameObject.Find("SidePhotoSynth").GetComponent<MeshRenderer>().material;
                        waterMarkImage_Material.SetFloat("_Saturation", 0);
                    }               
                }
                else
                {
                    personImage_Material.SetFloat("_Saturation", 1);
                    if (GameObject.Find("SidePhotoSynth") != null)
                    {
                        Material waterMarkImage_Material = GameObject.Find("SidePhotoSynth").GetComponent<MeshRenderer>().material;
                        waterMarkImage_Material.SetFloat("_Saturation", 1);
                    }         
                }
            }
            public static void ChagneTransparent()
            {
                Material personImage_Material = GameObject.Find("PhotoSynth").GetComponent<MeshRenderer>().material;
                float photo_transparent = UnityEngine.Random.Range(0.6f, 1.0f);
                Color photo_color = personImage_Material.GetColor("_Color");
                photo_color = new Color(photo_color.r, photo_color.g, photo_color.b, photo_transparent);
                personImage_Material.SetColor("_Color", photo_color);
            }
        }
        public static void ChangeColorSaturation()
        {
            Material objectBackground_Material;
            if (GameObject.Find("PassportInnerSynth") == null)
                objectBackground_Material = GameObject.Find("CardSynth").GetComponent<MeshRenderer>().material;
            else
                objectBackground_Material = GameObject.Find("PassportInnerSynth").GetComponent<MeshRenderer>().material;
            float saturation_value = UnityEngine.Random.Range(1.4f, 2.0f);
            objectBackground_Material.SetFloat("_Saturation", saturation_value);
        }
        public static void ChangeKorColor()
        {
            Material kor_Material = GameObject.Find("KOR").GetComponent<MeshRenderer>().material;
            Color[] kor_color = new Color[] { PresetColor.kor_red, PresetColor.kor_yellow, PresetColor.kor_brown };
            int random_index = UnityEngine.Random.Range(0, kor_color.Length);
            kor_Material.SetColor("_BaseColor", kor_color[random_index]);
        }
        public static void ChangeStamp(Texture[] stamps)
        {
            int random_index = UnityEngine.Random.Range(0, stamps.Length);
            Texture stamp_texture = stamps[random_index];
            MeshRenderer stamp_MeshRenderer = GameObject.Find("Stamp").GetComponent<MeshRenderer>();
            stamp_MeshRenderer.material.SetTexture("_MainTex", stamp_texture);
        }
        public static void ChangeStampTransparent()
        {
            float stamp_transparent = UnityEngine.Random.Range(0.4f, 1.0f);
            MeshRenderer stamp_MeshRenderer = GameObject.Find("Stamp").GetComponent<MeshRenderer>();
            Color stamp_color = stamp_MeshRenderer.material.GetColor("_Color");
            stamp_color = new Color(stamp_color.r, stamp_color.g, stamp_color.b, stamp_transparent);
            stamp_MeshRenderer.material.SetColor("_Color", stamp_color);
        }
        public static void ChangeTempleteMetalic()
        {
            Material combinedObject_material = GameObject.Find("Inner").GetComponent<MeshRenderer>().material;
            float random_metalic = UnityEngine.Random.Range(0, 0.2f);
            combinedObject_material.SetFloat("_Metallic", random_metalic);
        }
        public static void ChangeToLitShader()
        {
            Shader lit = Shader.Find("Universal Render Pipeline/Lit");
            Material Inner_material = GameObject.Find("Inner").GetComponent<MeshRenderer>().material;
            Inner_material.shader = lit;
        }
        public static void ChangeToSegShader()
        {
            Shader unlit = Shader.Find("Universal Render Pipeline/Unlit");
            Material Inner_material = GameObject.Find("Inner").GetComponent<MeshRenderer>().material;
            Inner_material.shader = unlit;
        }

    }
    public static class Background
    {
        public static void ChangeBackground(Texture[] backgrounds)
        {
            int random_index = UnityEngine.Random.Range(0, backgrounds.Length);
            Texture background = backgrounds[random_index];
            MeshRenderer background_MeshRenderer = GameObject.Find("Background").GetComponent<MeshRenderer>();
            background_MeshRenderer.material.SetTexture("_BaseMap", background);
        }
        public static void NomalPosition()
        {
            GameObject background = GameObject.Find("Background");
            if (background != null)
            {
                Vector3 background_pos = background.transform.localPosition;
                //background.transform.localPosition = new Vector3(background_pos.x, background_pos.y, 0f); //-0.0423f
            }
        }
        public static void HandPosition()
        {
            GameObject background = GameObject.Find("Background");
            if (background != null)
            {
                Vector3 background_pos = background.transform.localPosition;
                //background.transform.localPosition = new Vector3(background_pos.x, background_pos.y, 0.0714f);
            }
        }
    }
    public static class SubCard
    {
        public static void CreateSubObject(Texture[] image_set, float freq)
        {
            GameObject old_object = GameObject.Find("sub_card");
            if (old_object != null)
                Object.Destroy(old_object);

            float random_gen = UnityEngine.Random.Range(0, 1.0f);
            if (random_gen < freq)
            {
                // create mesh
                GameObject sub_card = GameObject.CreatePrimitive(PrimitiveType.Plane);
                sub_card.name = "sub_card";
                // change texture
                MeshRenderer meshRenderer = sub_card.GetComponent<MeshRenderer>();
                meshRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                int random_index = UnityEngine.Random.Range(0, image_set.Length);
                meshRenderer.material.SetTexture("_BaseMap", image_set[random_index]);
                // scale, position, rotation
                float width = meshRenderer.material.mainTexture.width;
                float height = meshRenderer.material.mainTexture.height;
                float w_ratio = width / height;
                float h_ratio = 1;

                float[] scale_range = new float[] { 0.2f, 1.0f };
                float[] pos_rangex = new float[] { -20.0f, 10.0f };
                float[] pos_rangez = new float[] { -15.0f, 25.0f };
                float pos_y = -0.1f;
                float[] rot_rangey = new float[] { 0f, 360f };

                //float scale_coefficient = UnityEngine.Random.Range(scale_range[0], scale_range[1]);
                float scale_coefficient = 1;
                float scale_x = w_ratio * scale_coefficient;
                float scale_z = h_ratio * scale_coefficient;
                Vector3 scale = new Vector3(scale_x, 1.0f, scale_z);

                float pos_x = UnityEngine.Random.Range(pos_rangex[0], pos_rangex[1]);
                float pos_z = UnityEngine.Random.Range(pos_rangez[0], pos_rangez[1]);
                Vector3 position = new Vector3(pos_x, pos_y, pos_z);

                float rot_y = UnityEngine.Random.Range(rot_rangey[0], rot_rangey[1]);

                sub_card.transform.rotation = Quaternion.Euler(0, rot_y, 0);
                sub_card.transform.localScale = scale;
                sub_card.transform.position = position;
                //
            }
        }
        public static void DelSubObject()
        {
            GameObject sub_card = GameObject.Find("sub_card");
            Object.Destroy(sub_card);
        }
    }
    public static class Hologram
    {
        public static bool CreateHologram( float random_hologram_freq, List<string> holo_filenames, List<GameObject> instanceList, List<string> feature_list)
        {
            bool useHologram = false;
            float random_hologram = UnityEngine.Random.Range(0, 1.0f);
            if (random_hologram < random_hologram_freq)
            {
                useHologram = true;
                GameObject old_object = GameObject.Find("Hologram");
                if (old_object != null)
                    UnityEngine.Object.Destroy(old_object);

                GameObject holo_obj = new GameObject();
                holo_obj.name = "Hologram";
                CreateHoloComponent(holo_obj, holo_filenames);
                // position
                List<Vector3> Bounds;
                if (GameObject.Find("PassportInnerSynth") == null)
                    Bounds = Seg.CalcRectObjectBounds("CardSynth");
                else
                    Bounds = Seg.CalcRectObjectBounds("PassportInnerSynth");
                //make instance
                CreateHoloInstances(holo_obj, Bounds, instanceList);
                CreateHoloInstancesOnTheChar(holo_obj, instanceList, feature_list);
            }
            return useHologram;
        }
        public static void CreateHoloComponent(GameObject holo_obj, List<string> holo_filenames)
        {
            MeshFilter holo_mesh_filter = holo_obj.AddComponent<MeshFilter>();
            MeshRenderer holo_mesh_renderer = holo_obj.AddComponent<MeshRenderer>();
            // load mesh file   
            int holo_index = UnityEngine.Random.Range(0, holo_filenames.Count);
            string holo_path = "Meshes/holo_shape/" + holo_filenames[holo_index];
            Mesh holo_mesh = Resources.Load<Mesh>(holo_path);
            holo_mesh_filter.sharedMesh = holo_mesh;
            // material
            holo_mesh_renderer.material = Resources.Load<Material>("Materials/Etc/HologramMat");
            // holo alpha
            string scene_name = SceneManager.GetActiveScene().name.Split(new char[] { '_' })[0];
            Dictionary<string, float> holo_alpha = new Dictionary<string, float>() { { "Alien", 0.05f }, { "Driver", 0.2f }, { "ID", 0.3f }, { "Overseas", 0.1f } };
            holo_mesh_renderer.material.SetFloat("Alpha", holo_alpha[scene_name]);
            // transform
            float scale_size = UnityEngine.Random.Range(0.2f, 0.4f);
            Vector3 holo_scale = new Vector3(scale_size, scale_size, scale_size);
            holo_obj.transform.localScale = holo_scale;
            holo_obj.transform.position = new Vector3(1000, 1000, 1000);
        }
        public static void CreateHoloInstances(GameObject holo_obj, List<Vector3> Bounds, List<GameObject> instanceList)
        {
            float min_x = Bounds[0].x;
            float min_y = Bounds[0].y;
            float max_x = Bounds[3].x;
            float max_y = Bounds[3].y;
            float x_offset = 0f;
            float y_offset = 0f;
            float z_offset = -0.02f;
            // create Instances
            int Instance_num = UnityEngine.Random.Range(3, 6);
            for (int i = 0; i < Instance_num; i++)
            {
                float pos_x = UnityEngine.Random.Range(min_x + x_offset, max_x - x_offset);
                float pos_y = UnityEngine.Random.Range(min_y + y_offset, max_y - y_offset);
                float pos_z = Bounds[0].z + z_offset;
                Vector3 pos = new Vector3(pos_x, pos_y, pos_z);
                Vector3 rot = new Vector3(-180, 0, 0);
                GameObject instance = UnityEngine.Object.Instantiate(holo_obj, pos, Quaternion.Euler(rot));
                instanceList.Add(instance);
            }
        }
        public static void CreateHoloInstancesOnTheChar(GameObject holo_obj, List<GameObject> instanceList, List<string> feature_name_list)
        {
            // 각각의 텍스트 위에 홀로그램 생성
            foreach(string feature_name in feature_name_list)
            {
                List<Vector3> feature_texture_coord = BoundingBox.CalcTMPBounds(feature_name);
                Vector3 bot_left = feature_texture_coord[0];
                Vector3 top_right = feature_texture_coord[3];

                float x_offset = 0f;
                float y_offset = 0f;
                float z_offset = -0.02f;
                // create Instances
                int Instance_num = UnityEngine.Random.Range(0, 3);
                for (int i = 0; i < Instance_num; i++)
                {
                    float pos_x = UnityEngine.Random.Range(bot_left.x + x_offset, top_right.x - x_offset);
                    float pos_y = UnityEngine.Random.Range(bot_left.y + y_offset, top_right.y - y_offset);
                    float pos_z = bot_left.z + z_offset;
                    Vector3 pos = new Vector3(pos_x, pos_y, pos_z);
                    Vector3 rot = new Vector3(-180, 0, 0);
                    GameObject instance = UnityEngine.Object.Instantiate(holo_obj, pos, Quaternion.Euler(rot));
                    instanceList.Add(instance);
                }
            }         
        }
        public static void DestroyHologram(List<GameObject> instanceList)
        {
            GameObject holo_object = GameObject.Find("Hologram");
            if (holo_object != null)
            {
                UnityEngine.Object.Destroy(holo_object);
                foreach (GameObject instance in instanceList)
                    UnityEngine.Object.Destroy(instance);
                instanceList.Clear();
            }
        }
        
    }
}

