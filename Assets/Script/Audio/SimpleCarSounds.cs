using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// While I actually have JSAM, I also wanted to be able to use the same sounds for different objects.
/// Sadly, JSAM's Sound calls pretty much directs everything into one... Sound Object (Which is a portable/ScriptableObject AudioSource)
/// So I opted for this, along with a FadeIn-Out Corou.
/// 
/// Also if you're wondering why I didn't use MotorTorque instead of Speed, it'd be because Torque actually
/// decreases the faster you go, weird innit?
/// </summary>
/// 
////// ALSO, we don't really need to sync anything here. Since:
/// + This script depends on the current car's Rigidbody velocity.magnitude. And the velocity.magnitude itself 
/// depends on the inputs (vInput, hInput, isBrakeCalled, brakeVal) on the car. Those are already synced in CarControl.
/// + Each car has an AudioSource, with the Mixer being the mainly used one (MainMixer) and 3D Spatial Sounds enabled
/// so we don't need to worry about 3D car audio.
/// + (Also, GameManager var is only here for us to access its flags)
public class SimpleCarSounds : MonoBehaviour
{

    public float minSpeed;
    public float maxSpeed;
    private float currentSpeed;

    #region Private Serializable Variables
    [SerializeField]
    [Tooltip("Drag from Inspector of needed")]
    private Rigidbody carRb;
    [SerializeField]
    [Tooltip("Technically the CarControl script.\n" +
        "We're taking the motorTorque and currentTorque to calculate the pitch.")]
    private CarControl cctrl;
    //public CarControlNormal ccn;

    //[SerializeField]
    //[Tooltip("GameManager, technically to get flags to FadeIn/Out the car sounds")]
    //private GameManager gm;

    private float motorTorqueRatio;

    [SerializeField]
    private AudioSource carAudio;
    #endregion

    public float minPitch;
    public float maxPitch;
    private float pitchFromCar;

    // Fade flags
    [HideInInspector]
    public bool isFadeInDone, isFadeOutDone;

    private void Awake()
    {
        //carAudio = GetComponent<AudioSource>();
        //carRb = GetComponent<Rigidbody>();
        carRb = this.gameObject.GetComponent<Rigidbody>();
        carAudio = this.gameObject.GetComponent<AudioSource>();

        //cctrl = this.gameObject.GetComponent<CarControl>();

        //ccn = this.gameObject.GetComponent<CarControlNormal>();

        //gm = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    void Start()
    {
        carAudio.playOnAwake = true;
        carAudio.volume = 0f;

        isFadeInDone = false;
        isFadeOutDone = false;
    }

    /// <summary>
    /// 
    /// </summary>
    void Update()
    {
        // Getting flags from GameManager singleton 
        // If isGameStart --> FadeIn
        if (GameManager.Instance.GetGameStatus() == true && isFadeInDone == false)
        {
            StartCoroutine(SoundFade(true, carAudio, 0.5f, 0f, 1f));
            isFadeInDone = true;
        }
        
        EngineSound();
        //EngineSoundTorque();

        // If all players have finished the race + result screen appeared --> FadeOut car sound volume, and then stop the Audio Source
        if (GameManager.Instance.IsAllPlayersFinished() == true && isFadeOutDone == false)
        {
            StartCoroutine(SoundFade(false, carAudio, 0.5f, 1f, 0f));
            
            //isFadeOutDone = true;
        }
        // This is actually set in SoundFade
        if (isFadeOutDone)
        {
            carAudio.Stop();
        }
    }

    //void EngineSoundTorque()
    //{
    //    /// (motorTorque-currentMotorTorque)/motorTorque
    //    ///
    //    //motorTorqueRatio = (ccn.motorTorque - ccn.currentMotorTorque) / ccn.motorTorque;
    //    motorTorqueRatio = (cctrl.motorTorque - cctrl.currentMotorTorque) / cctrl.motorTorque;
    //    //print("motorTorqueRatio: " + motorTorqueRatio);
    //}

    void EngineSound()
    {
        

        currentSpeed = carRb.velocity.magnitude;
        pitchFromCar = carRb.velocity.magnitude / 60f;

        // Init this because if I don't, this will have a value of 1
        // This will sorta mess up the else if argument, so I limited it as well
        carAudio.pitch = minPitch;

        if (currentSpeed <= minSpeed)
        {
            if (carAudio.pitch > minPitch)
            {
                //carAudio.pitch -= 0.1f;
                carAudio.pitch = carAudio.pitch - pitchFromCar;
                //carAudio.pitch = maxPitch - pitchFromCar;
            }
            else
            {
                carAudio.pitch = minPitch;
            }
        }
        else if (currentSpeed > minSpeed && currentSpeed <= maxSpeed)
        {
            //carAudio.pitch = minPitch + pitchFromCar;
            //carAudio.pitch += pitchFromCar;
            if (carAudio.pitch < maxPitch)
                carAudio.pitch = carAudio.pitch + pitchFromCar;
            else
                carAudio.pitch = maxPitch;
        }
        //if (currentSpeed > maxSpeed)
        else
        {
            if (carAudio.pitch < maxPitch)
            {
                //carAudio.pitch += pitchFromCar;
                //carAudio.pitch += 0.1f;
                //carAudio.pitch = minPitch + pitchFromCar;
                carAudio.pitch = carAudio.pitch + pitchFromCar;
            }
            else
            {
                carAudio.pitch = maxPitch;                    
            }
        }

        // Debugging purposes
        //print("CarAudio pitch: " + carAudio.pitch);
    }

    /// <summary>
    /// For FadeOut, set isFadeIn = false, startingVolume to random since it's not going to be used.
    /// And targetVolume to 0
    /// </summary>
    /// <param name="isFadeIn"></param>
    /// <param name="source"></param>
    /// <param name="fadeDuration"></param>
    /// <param name="startingVolume"></param>
    /// <param name="targetVolume"></param>
    /// <returns></returns>
    private IEnumerator SoundFade(bool isFadeIn, AudioSource source, float fadeDuration, float startingVolume, float targetVolume)
    {
        float time = 0f;
        float startingVol = startingVolume;
        float targetVol = targetVolume;
        
        if (!isFadeIn)
        {
            startingVol = source.volume;
            targetVol = 0f;
        }

        //double lengthOfSource = (double)source.clip.samples / source.clip.frequency;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            source.volume = Mathf.Lerp(startingVol, targetVol, time / fadeDuration);
            yield return null;
        }

        if (!isFadeIn)
        {
            isFadeOutDone = true;
        }

        //if (!isFadeIn)
        //{
        //    //double lengthOfSource = (double)source.clip.samples / source.clip.frequency;
        //    while (time < fadeDuration)
        //    {
        //        time += Time.deltaTime;
        //        source.volume = Mathf.Lerp(startingVol, targetVolume, time / fadeDuration);
        //        yield return null;
        //    }

        //}

        //else
        //{

        //}



        yield break;
    }
}