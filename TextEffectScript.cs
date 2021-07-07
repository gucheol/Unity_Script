using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;


namespace TextProperty_Script
{
    public class TextProperty
    {
        public static void Ink_smudge_On(List<string> feature_ExtractName)
        {
            foreach (string feature in feature_ExtractName)
            {
                if (GameObject.Find(feature).GetComponent<MeshRenderer>().enabled == false)
                    continue;
                Material material = GameObject.Find(feature).GetComponent<MeshRenderer>().material;
                material.EnableKeyword("UNDERLAY_ON");

                float ran_dilate = UnityEngine.Random.Range(0, 1f);
                material.SetFloat("_UnderlayDilate", ran_dilate);
                material.SetFloat("_UnderlaySoftness", 1);
            }
        }
        public static void Ink_smudge_Off(List<string> feature_ExtractName)
        {
            foreach (string feature in feature_ExtractName)
            {
                if (GameObject.Find(feature).GetComponent<MeshRenderer>().enabled == false)
                    continue;
                Material material = GameObject.Find(feature).GetComponent<MeshRenderer>().material;
                material.DisableKeyword("UNDERLAY_ON");         
            }
        }
        public static void TextSpacingOption(List<string> feature_ExtractName)
        {
            string current_scene = SceneManager.GetActiveScene().name;
            if (current_scene == "Alien_Card" || current_scene == "Overseas_Card")
            {
                foreach (string feature in feature_ExtractName)
                {
                    if (feature == "Number")
                        TMPRandomSpacingRange(-5f, 5f, feature);
                    else
                        TMPRandomSpacingRange(0f, 20f, feature);
                }
            }
            if(current_scene == "Driver_Card")
            {
                foreach(string feature in feature_ExtractName)
                {
                    if(feature == "CardNumber")
                        TMPRandomSpacingRange(-15f, -10f, feature);
                    else if(feature == "Name")
                        TMPRandomSpacingRange(-10f, 30f, feature);
                    else if(feature == "PersonNumber")
                        TMPRandomSpacingRange(-15f, 0f, feature);
                    else
                        TMPRandomSpacingRange(-20f, -10f, feature);
                }
            }
            if(current_scene == "ID_Card")
            {
                foreach(string feature in feature_ExtractName)
                {
                    if(feature == "Name")
                        TMPRandomSpacingRange(-5f, 10f, feature);
                    else if(feature == "IDNumber")
                        TMPRandomSpacingRange(-10f, 20f, feature);
                    else
                        TMPRandomSpacingRange(5f, 50f, feature);
                }
            }
        }
        public static void TMPRandomSpacingRange(float min_spacing, float max_spacing, string tmp_name)
        {
            TMP_Text feature_text = GameObject.Find(tmp_name).GetComponent<TMP_Text>();
            feature_text.characterSpacing = UnityEngine.Random.Range(min_spacing, max_spacing); ;
        }
    }
}

