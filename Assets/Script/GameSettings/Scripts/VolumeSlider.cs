using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public GameSettings gameSettings;
    public VolumeType type;

    private void Awake() {
        GetComponent<Slider>().onValueChanged.AddListener(delegate {
            OnValueChange();
        });
    }

    private void Start() {
        switch(type){
            case VolumeType.Master:
                gameObject.GetComponent<Slider>().value = gameSettings.volume.master;
                break;
            case VolumeType.Music:
                gameObject.GetComponent<Slider>().value = gameSettings.volume.music;
                break;
            case VolumeType.Sound:
                gameObject.GetComponent<Slider>().value = gameSettings.volume.sound;
                break;
        }
    }

    public void OnValueChange(){
        switch(type){
            case VolumeType.Master:
                gameSettings.volume.master = gameObject.GetComponent<Slider>().value;
                break;
            case VolumeType.Music:
                gameSettings.volume.music = gameObject.GetComponent<Slider>().value;
                break;    
            case VolumeType.Sound:
                gameSettings.volume.sound = gameObject.GetComponent<Slider>().value;
                break;
        }
        gameSettings.isSet = true;
    }
}

public enum VolumeType{
    Master,
    Music,
    Sound
}