using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object_Script;


namespace Hand_Script
{
    public static class Hand
    {
        public static void Animation_Hand_PickUp()
        {
            GameObject.Find("hand").GetComponent<Animator>().Play("Armature|PickUp", 0, 1.0f);
        }
        public static void Animation_Hand_ThumbDown()
        {
            GameObject.Find("hand").GetComponent<Animator>().Play("Armature|ThumbPressDown", 0, 1.0f);
        }
        public static void Adjust_Hand_Pose_location(string current_pose)
        {
            Transform hand_transform = GameObject.Find("hand_pivot").GetComponent<Transform>();
            string[] location;

            if (GameObject.Find("CardSynth") != null)
                location = new string[] { "right", "bottom", "left", "top" };
            else
                location = new string[] { "right", "left", "top"};

            int random = UnityEngine.Random.Range(0, location.Length);
            string current_location = location[random];

            if (current_pose == "PickUp")
            {
                if (current_location == "right")
                {
                    float x = UnityEngine.Random.Range(-8.14f, -1.68f);
                    float y = -0.29f;
                    float z = UnityEngine.Random.Range(11.78f, 12.4f);
                    hand_transform.position = new Vector3(x, y, z);

                    float rotate_y = UnityEngine.Random.Range(-13.8f, 38.79f);
                    hand_transform.rotation = Quaternion.Euler(-18.786f, rotate_y, 196.805f);
                }
                else if (current_location == "bottom")
                {
                    float x = UnityEngine.Random.Range(-0.7f, 0.04f);
                    float y = -0.29f;
                    float z = UnityEngine.Random.Range(0.9f, 11.41f);
                    hand_transform.position = new Vector3(x, y, z);

                    float rotate_y = UnityEngine.Random.Range(90.66f, 144.9f);
                    hand_transform.rotation = Quaternion.Euler(-18.786f, rotate_y, 196.805f);
                }
                else if (current_location == "left")
                {
                    float x = UnityEngine.Random.Range(-7.47f, -0.58f);
                    float y = -0.29f;
                    float z = UnityEngine.Random.Range(-0.36f, 0.84f);
                    hand_transform.position = new Vector3(x, y, z);

                    float rotate_y = UnityEngine.Random.Range(188.7f, 236.56f);
                    hand_transform.rotation = Quaternion.Euler(-18.786f, rotate_y, 196.805f);
                }
                else if (current_location == "top")
                {
                    float x = UnityEngine.Random.Range(-9.59f, -8.94f);
                    float y = -0.29f;
                    float z = UnityEngine.Random.Range(1.26f, 12.08f);
                    hand_transform.position = new Vector3(x, y, z);

                    float rotate_y = UnityEngine.Random.Range(268.5f, 309.51f);
                    hand_transform.rotation = Quaternion.Euler(-18.786f, rotate_y, 196.805f);
                }
            }
            else if (current_pose == "ThumbDown")
            {
                if (current_location == "right")
                {
                    float x = UnityEngine.Random.Range(-0.97f, -7.92f);
                    float y = -0.303f;
                    float z = UnityEngine.Random.Range(10.4f, 12.21f);
                    hand_transform.position = new Vector3(x, y, z);

                    float rotate_y = UnityEngine.Random.Range(-29.78f, 21.1f);
                    hand_transform.rotation = Quaternion.Euler(0.05f, rotate_y, 182.647f);
                }
                else if (current_location == "bottom")
                {
                    float x = UnityEngine.Random.Range(-2.1f, -0.32f);
                    float y = -0.303f;
                    float z = UnityEngine.Random.Range(1.15f, 11.71f);
                    hand_transform.position = new Vector3(x, y, z);

                    float rotate_y = UnityEngine.Random.Range(69.64f, 116f);
                    hand_transform.rotation = Quaternion.Euler(0.05f, rotate_y, 182.647f);
                }
                else if (current_location == "left")
                {
                    float x = UnityEngine.Random.Range(-7.72f, -0.82f);
                    float y = -0.38f;
                    float z = UnityEngine.Random.Range(-0.11f, 2.2f);
                    hand_transform.position = new Vector3(x, y, z);

                    float rotate_y = UnityEngine.Random.Range(154.1f, 199.68f);
                    hand_transform.rotation = Quaternion.Euler(0.05f, rotate_y, 182.647f);
                }
                else if (current_location == "top")
                {
                    float x = UnityEngine.Random.Range(-8.49f, -6.2f);
                    float y = -0.38f;//2.15f;
                    float z = UnityEngine.Random.Range(0.77f, 11.55f);
                    hand_transform.position = new Vector3(x, y, z);

                    float rotate_y = UnityEngine.Random.Range(255.1f, 301.8f);
                    hand_transform.rotation = Quaternion.Euler(0.05f, rotate_y, 182.647f);
                }
            }

        }
        public static void Random_Hand_Pose()
        {
            string[] pose = new string[] { "PickUp", "ThumbDown" };
            int random = UnityEngine.Random.Range(0, pose.Length);
            string current_pose = pose[random];
            if (current_pose == "PickUp") Animation_Hand_PickUp();
            else if (current_pose == "ThumbDown") Animation_Hand_ThumbDown();

            Adjust_Hand_Pose_location(current_pose);
        }

        public static void Seg_Hand_Color()
        {
            Material hand_material = GameObject.Find("ZBrushPolyMesh3D.001").GetComponent<SkinnedMeshRenderer>().material;
            Color black = new Color(0, 0, 0, 1);
            Color black_transparent = new Color(0, 0, 0, 0);
            if (hand_material.shader.name == "Universal Render Pipeline/Unlit")
                hand_material.SetColor("_BaseColor", black);
            else
                hand_material.SetColor("_Color", black_transparent);

        }
        public static void Lit_Hand_Color()
        {
            Material hand_material = GameObject.Find("ZBrushPolyMesh3D.001").GetComponent<SkinnedMeshRenderer>().material;
            Color apricot = new Color(1f, 0.922f, 0.8632076f, 1);
            Color apricot_transparent = new Color(1f, 0.922f, 0.8632076f, 0);
            if (hand_material.shader.name == "Universal Render Pipeline/Unlit")
                hand_material.SetColor("_BaseColor", apricot);
            else
                hand_material.SetColor("_Color", apricot_transparent);
        }
        public static void Show_Hand(float show_freq)
        {      
            Shader hand_shader;
            Color hand_color;
            string variable;
            float random_hand = UnityEngine.Random.Range(0, 1.0f);
            // change transpareent and adjust background position
            if (random_hand < show_freq) //show
            {
                hand_shader = Shader.Find("Universal Render Pipeline/Unlit");
                Color apricot = new Color(1f, 0.922f, 0.8632076f, 1);
                hand_color = apricot;
                variable = "_BaseColor";
                Background.HandPosition();
                
            }
            else //hide
            {
                hand_shader = Shader.Find("Unlit/ColorAdjust");
                Color apricot_transparent = new Color(1f, 0.922f, 0.8632076f, 0);
                hand_color = apricot_transparent;
                variable = "_MainTex";
                Background.NomalPosition();
            }

            Material hand_material = GameObject.Find("ZBrushPolyMesh3D.001").GetComponent<SkinnedMeshRenderer>().material;
            hand_material.shader = hand_shader;
            hand_material.SetColor(variable, hand_color);
        }
    }
}

