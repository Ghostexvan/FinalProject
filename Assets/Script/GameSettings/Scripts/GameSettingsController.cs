using UnityEngine;
using UnityEngine.Audio;

public class GameSettingsController : MonoBehaviour
{
    private static GameSettingsController _Instance;

    public GameSettings gameSettings;
    public AudioMixer mixer;

    private void Awake() {
        if (_Instance == null){
            _Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(this.gameObject);
        
        LoadSettings();
        ApplySettings();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (gameSettings.isSet)
            ApplySettings();
    }

    void ApplySettings(){
        FullScreenMode screenMode = gameSettings.resolution.isFullscreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed;
        Screen.SetResolution(gameSettings.resolution.width, gameSettings.resolution.height, screenMode);

        Application.targetFrameRate = gameSettings.frameRate;

        //Debug.LogWarning("Master Volume: " + PlayerPrefs.GetFloat("JSAM_MASTER_VOL"));
        //Debug.LogWarning("Music Volume: " + PlayerPrefs.GetFloat("JSAM_MUSIC_VOL"));
        //Debug.LogWarning("Sound Volume: " + PlayerPrefs.GetFloat("JSAM_SOUND_VOL"));

        //AudioMixer mixer = Resources.Load<AudioMixer>(audioPath);
        mixer.SetFloat("masterVolume", Mathf.Log10(gameSettings.volume.master) * 20f);
        mixer.SetFloat("musicVolume", Mathf.Log10(gameSettings.volume.music) * 20f);
        mixer.SetFloat("sfxVolume", Mathf.Log10(gameSettings.volume.sound) * 20f);
        //PlayerPrefs.SetFloat("JSAM_MASTER_VOL", gameSettings.volume.master);
        //PlayerPrefs.SetFloat("JSAM_MUSIC_VOL", gameSettings.volume.music);
        //PlayerPrefs.SetFloat("JSAM_SOUND_VOL", gameSettings.volume.sound);

        gameSettings.isSet = false;

        SaveSettings();
    }

    public void LoadSettings(){
        if (PlayerPrefs.GetInt("width") != 0){
            gameSettings.resolution.width = PlayerPrefs.GetInt("width");
            gameSettings.resolution.height = PlayerPrefs.GetInt("height");
            gameSettings.resolution.isFullscreen = PlayerPrefs.GetInt("fullscreen") == 1;
            gameSettings.volume.master = PlayerPrefs.GetFloat("volumeMaster");
            gameSettings.volume.music = PlayerPrefs.GetFloat("volumeMusic");
            gameSettings.volume.sound = PlayerPrefs.GetFloat("volumeSound");
            //gameSettings.volume.master = PlayerPrefs.GetFloat("JSAM_MASTER_VOL");
            //gameSettings.volume.music = PlayerPrefs.GetFloat("JSAM_MUSIC_VOL");
            //gameSettings.volume.sound = PlayerPrefs.GetFloat("JSAM_SOUND_VOL");
            gameSettings.frameRate =  PlayerPrefs.GetInt("framerate");
        } 
    }

    private void OnApplicationQuit() {
        SaveSettings();
    }

    public void SaveSettings(){
        PlayerPrefs.SetInt("width", gameSettings.resolution.width);
        PlayerPrefs.SetInt("height", gameSettings.resolution.height);
        PlayerPrefs.SetInt("fullscreen", gameSettings.resolution.isFullscreen ? 1 : 0);
        PlayerPrefs.SetFloat("volumeMaster", gameSettings.volume.master);
        PlayerPrefs.SetFloat("volumeMusic", gameSettings.volume.music);
        PlayerPrefs.SetFloat("volumeSound", gameSettings.volume.sound);
        //PlayerPrefs.SetFloat("JSAM_MASTER_VOL", gameSettings.volume.master);
        //PlayerPrefs.SetFloat("JSAM_MUSIC_VOL", gameSettings.volume.music);
        //PlayerPrefs.SetFloat("JSAM_SOUND_VOL", gameSettings.volume.sound);
        PlayerPrefs.SetInt("framerate", gameSettings.frameRate);

        PlayerPrefs.Save();
    }
}
