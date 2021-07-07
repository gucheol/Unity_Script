using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoPassportScript : MonoBehaviour
{
    string[] SceneList = new string[] { 
        //"Bangladesh", "China", "Egypt", "India", "Indonesia", "Korea", "Kazakhstan", "Kyrgyzstan", "Mongol",
        //"Myanmar", "Nepal", "Pakistan", "Philippines", "Russia", "Srilanka", "Thailand", "Ukraine", "Uzbekistan", "Vietnam"
    "Australia", "Bangladesh", "Bhutan", "Cambodia", "China","Egypt", "Ethiopia", "France", 
    "India", "Indonesia", "Kazakhstan", "Korea", "Kyrgyzstan", "Libya", "Malaysia",
    "Mali", "Mongol", "Myanmar", "Nepal", "Newzealand", "Pakistan", "Philippines", "Russia", "Southafrica", "Srilanka",
    "Tagikistan", "Thailand", "Tiwan", "Ukraine", "USA", "Uzbekistan", "Vietnam"
    };

    public class SceneController
    {
        public static bool withCardDataGen = false;
        public static int count = 0;
        public static bool isRunning = false;

        public static int num_CaptureData = 10;
        public static float random_light_intensity_freq = 0.1f;
        public static float random_color_freq = 0.05f;
        public static float random_objectBackground_saturation_freq = 0.3f;
        public static float random_hand_show_freq = 0.1f; //0.15f
        public static float random_subcard_show_freq = 0f; //0.3f
        public static float random_inner_unlit_freq = 0.3f; //0.5f
        public static float random_card_shadowAndLight_freq = 0.3f; //0.3f
        public static float random_hologram_freq = 0.2f;
        public static bool isFormattedRandomMR = true; //false

    }
    string Scene;
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneController.count == SceneList.Length)
        {
            if (SceneController.withCardDataGen)
            {
                SceneManager.LoadScene("AutoCard");
                Destroy(gameObject);
            }     
            else
            {  
                UnityEditor.EditorApplication.isPlaying = false;
            }
                
        }         
        else
        {
            if (!SceneController.isRunning)
            {
                SceneController.isRunning = true;
                Scene = SceneList[SceneController.count] + "_Passport";
                SceneManager.LoadScene(Scene);
            }
        }
        
    }
}

