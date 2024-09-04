using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

//This is the script that handles all the sound in the game.
namespace _DPS
{

//This is the script that handles all the sound in the game.
public class AudioManager : Singleton<AudioManager>
{

    protected AudioManager(){}
    public float GlobalVolumeOverride =.2f;
    public float MusicVolumeOverride =.75f;

    public Image SoundButtonImage;
    public Sprite SoundOnSprite, SoundOffSprite;
    public Image MusicButtonImage;
    public Sprite MusicOnSprite, MusicOffSprite;
    public AudioMixerGroup EffectsMixer;
    public AudioMixerGroup MusicMixer;
    public Slider VolumeSlider;
    public static bool ToggleMusic;
    public Sound[] sounds;
    
    private Coroutine _timerPlay;
    private Coroutine _musicPlay;
    private int _lastMusicClip = -1;
    private List<Sound> _musicList;
    public static bool PausedGame;
    public static int MusicSetting;
    public static int EffectsSetting;

    void Awake () 
    {
        
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            Destroy(gameObject);
        }
        DontDestroyOnLoad(Instance);
        //music is default 0 as enabled 1 is disabled
        if (!PlayerPrefs.HasKey("Music"))
        {
            PlayerPrefs.SetInt("Music", 0);
        }
        
        if (!PlayerPrefs.HasKey("Effects"))
        {
            PlayerPrefs.SetInt("Effects", 0);
        }

        if (!PlayerPrefs.HasKey("Volume"))
        {
            PlayerPrefs.SetFloat("Volume", GlobalVolumeOverride);
        }
        //DontDestroyOnLoad(gameObject);
        foreach (Sound s in sounds) {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = PlayerPrefs.GetFloat("Volume");
            s.source.pitch = s.pitch;
        }

        _musicList = GetAllMusic();
    }

    void Start()
    {
        MusicSetting = PlayerPrefs.GetInt("Music");
        EffectsSetting = PlayerPrefs.GetInt("Effects");
        //start music?
        if (MusicSetting == 0)
        {
            PlayRandomMusic();
        }
        global::Logger.Log("Music " + MusicSetting);

        //VolumeSlider.onValueChanged.AddListener(delegate { ManageVolume(); });
        //VolumeSlider.value = PlayerPrefs.GetFloat("Volume");
    }

    public void Play(string name)
    {
        if(EffectsSetting != 0) return;
        Play(name, 0f);
    }


    //The function to play the sound
    public void Play(string name, float volOverride = 0f) 
    {
        if(EffectsSetting != 0) return;
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            global::Logger.Log("Returning zeero!");
            return;
        }
        global::Logger.Log("Audiomanager is playing " + name);

        //do not let simple sounds go over 2 second
        //use play looped for long sounds and music
        _timerPlay = StartCoroutine(PlayTimer(s.source, volOverride));
    }

    private IEnumerator PlayTimer(AudioSource source, float volOverride, float duration = 2f)
    {
        source.volume = volOverride > 0 ? volOverride :PlayerPrefs.GetFloat("Volume");

        source.outputAudioMixerGroup = EffectsMixer;
        source.Stop();
        yield return new WaitForEndOfFrame();
        source.Play();
        while (duration > 0 && source.isPlaying)
        {
            duration -= Time.unscaledDeltaTime;
            //fadeout
            if (duration < 3f)
            {
                source.volume = Mathf.Clamp(Mathf.Lerp(source.volume, -0.1f, Time.unscaledDeltaTime),0,1);
            }

            //if (Time.timeScale <= 0)
            //{
            //    source.Stop();
            //}
            yield return null;
        }
        source.Stop();
    }

    public void StopAllButMusic()
    {
        foreach (var s in sounds)
        {
            if (s.name == "music")
            {
                continue;
            }
            s.source.Stop();
        }
        if (_timerPlay != null)
        {
            StopCoroutine(_timerPlay);
        }
    }
    //The function to stop the sound
    public void Stop(string name) {
        var s = Array.Find(sounds, sound => sound.name == name);
        s?.source.Stop();
    }

    private List<Sound> GetAllMusic()
    {
        var tmpList = new List<Sound>();
        foreach (var s in sounds)
        {
            if (s.name == "music")
            {
                tmpList.Add(s);
            }
        }

        return tmpList;
    }

    private void PlayRandomMusic()
    {
        var rng = UnityEngine.Random.Range(0, _musicList.Count);
        while (rng == _lastMusicClip)
        {
            rng = UnityEngine.Random.Range(0, _musicList.Count);
        }
        PlayMusic(rng);
    }

    private void PlayMusic(int rng)
    {
        _lastMusicClip = rng;
        _musicList[rng].source.volume = PlayerPrefs.GetFloat("Volume") * MusicVolumeOverride;
        _musicList[rng].source.outputAudioMixerGroup = MusicMixer;
        _musicList[rng].source.Play();
    }

    public void TogglePauseMusic(bool togglePaused)
    {
        if (togglePaused)
        {
            _musicList[_lastMusicClip].source.Pause();
        }
        else
        {
            _musicList[_lastMusicClip].source.UnPause();
        }
    }

    private void StopMusic()
    {
        _musicList[_lastMusicClip].source.Stop();
    }

    //Play looped sound
    public void PlayLooped(string name)
    {
    var s = Array.Find(sounds, sound => sound.name == name);
    if (s == null)
    {
        return;
    }
        s.source.loop = true;
        s.source.Play();
    }

    //Stop looped sound
    public void StopLoop(string name)
    {
        var s = Array.Find(sounds, sound => sound.name == name);
        if (s == null || !s.source.isPlaying)
        {  
            return;
        }
        s.source.loop = false;
        s.source.Stop();
    }

    public void ToggleMusicPlay()
    {
        if (PlayerPrefs.GetInt("Music") == 0)
        {
            MusicButtonImage.overrideSprite = MusicOffSprite;
            StopMusic();
            PlayerPrefs.SetInt("Music", 1);
            MusicSetting = 1;
        }
        else
        {
            MusicButtonImage.overrideSprite = MusicOnSprite;
            PlayRandomMusic();
            PlayerPrefs.SetInt("Music", 0);
            MusicSetting = 0;
        }
    }
    public void ToggleEffectscPlay()
    {
        if (PlayerPrefs.GetInt("Effects") == 0)
        {
            SoundButtonImage.overrideSprite = SoundOffSprite;
            StopAllButMusic();
            PlayerPrefs.SetInt("Effects", 1);
            EffectsSetting = 1;
        }
        else
        {
            SoundButtonImage.overrideSprite = SoundOnSprite;
            PlayerPrefs.SetInt("Effects", 0);
            EffectsSetting = 0;
        }
    }

    public void ManageVolume()
    {
        var volume = VolumeSlider.value;
        foreach (var sound in sounds)
        {
            sound.source.volume = volume;
        }
        PlayerPrefs.SetFloat("Volume", volume);
    }

    public void StopAllSounds()
    {
        foreach (Sound s in sounds) {
            s.source.Stop();
        }
    }

    void Update()
    {
        if (MusicSetting == 1)
            return;
        if (PausedGame)
        {
            _musicList[_lastMusicClip].source.Pause();
        }
        else
        {
            _musicList[_lastMusicClip].source.UnPause();
            if (!_musicList[_lastMusicClip].source.isPlaying)
            {
                PlayRandomMusic();
            }
        }
    }
}
}