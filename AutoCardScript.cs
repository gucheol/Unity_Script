using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoCardScript : MonoBehaviour
{
    string[] SceneList = new string[]
    {
        //"ID", "Overseas"
        "Alien","ID","Overseas","Driver"
    };
    public class SceneController
    {
        public static int count = 0;
        public static bool isRunning = false;

        public static int num_CaptureData = 10000;
        public static float random_light_intensity_freq = 0.1f;
        public static float random_color_freq = 0.05f;
        public static float random_personImage_grayscale_freq = 0.3f;
        public static float random_objectBackground_saturation_freq = 0.3f;
        public static float random_hand_show_freq = 0.0f; //0.15f
        public static float random_subcard_show_freq = 0.4f;
        public static float random_inner_unlit_freq = 0f; //0.3f
        public static float random_card_shadowAndLight_freq = 0f; //0.3f
        public static float random_hologram_freq = 0.2f; //0.4f

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
            UnityEditor.EditorApplication.isPlaying = false;
        else
        {
            if (!SceneController.isRunning)
            {
                SceneController.isRunning = true;
                Scene = SceneList[SceneController.count] + "_Card";
                SceneManager.LoadScene(Scene);
            }
        }
        
    }
}
