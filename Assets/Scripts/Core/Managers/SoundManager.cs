using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : PersistentMonoSingleton<SoundManager>
{
    [SerializeField] public AudioMixer audioMixer;
    
    [SerializedDictionary("BgmClipId", "AudioClip")]
    [SerializeField] private SerializedDictionary<BgmClipId, AudioClip> bgmClips;
    [SerializeField] [ReadOnly] public AudioSource bgmAudioSource;
    public float bgmVolume { get; private set; }
    
    [SerializedDictionary("SfxClipId", "AudioClip")]
    [SerializeField] private SerializedDictionary<SfxClipId, AudioClip> sfxClips;
    [SerializeField] [ReadOnly] public List<AudioSource> sfxAudioSourceList;
    public float sfxVolume { get; private set; }
    private int channelIndex;
    public GameObject audioSourcePlayer;

    protected override void Awake()
    {
        if (audioSourcePlayer != null)
            audioSourcePlayer.transform.SetParent(transform);

        base.Awake();

        SettingPanel.onBgmVolumeChanged += OnBgmVolumeChangedHandler;
    }

    private void OnDestroy()
    {
        SettingPanel.onBgmVolumeChanged -= OnBgmVolumeChangedHandler;
    }

    private void Start()
    {
        bgmVolume = GameDataManager.Instance.PlayerAccountData.BgmVolume;
        sfxVolume = GameDataManager.Instance.PlayerAccountData.SfxVolume;
        channelIndex = 0;
        
        bgmAudioSource.loop = true;
        bgmAudioSource.playOnAwake = true;
        bgmAudioSource.volume = bgmVolume;
        
        foreach (var sfxAudioSource in sfxAudioSourceList)
        {
            sfxAudioSource.loop = false;
            sfxAudioSource.playOnAwake = false;
            sfxAudioSource.volume = sfxVolume;
        }
        
        audioSourcePlayer.transform.SetParent(transform);
    }
    
    public void PlayBgm(BgmClipId clipId)
    {
        if (!bgmClips.ContainsKey(clipId))
        {
            Debug.Assert(false, $"BgmClipId {clipId} is not found in bgmClips");
            
            return;
        }

        PlayBgm(bgmClips[clipId]);
    }
    
    private void PlayBgm(AudioClip clip)
    {
        if (bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Stop();
        }

        bgmAudioSource.clip = clip;
        
        bgmAudioSource.Play();
    }
    
    public void StopBgm()
    {
        bgmAudioSource.Stop();
    }
    
    public void PauseBgm()
    {
        bgmAudioSource.Pause();
    }
    
    public void ResumeBgm()
    {
        bgmAudioSource.UnPause();
    }
    
    public void PlaySfx(SfxClipId clipId)
    {
        if (!sfxClips.ContainsKey(clipId))
        {
            Debug.Assert(false, $"SfxClipId {clipId} is not found in sfxClips");
            
            return;
        }

        if (clipId == SfxClipId.None)
        {
            return;
        }
        
        PlaySfx(sfxClips[clipId]);
    }
    
    private void PlaySfx(AudioClip clip)
    {
        for (int i = 0; i < sfxAudioSourceList.Count; ++i)
        {
            int loopIndex = (i + channelIndex) % sfxAudioSourceList.Count;

            if (sfxAudioSourceList[loopIndex].isPlaying)
            {
                continue;
            }
            
            sfxAudioSourceList[loopIndex].clip = clip;
            
            channelIndex = loopIndex;
            
            sfxAudioSourceList[loopIndex].Play();
            
            Debug.Log($"Play SFX Index: {loopIndex}");
            
            break;
        }
    }

    public void PlaySfxLoop(SfxClipId clipId)
    {
        if (!sfxClips.ContainsKey(clipId)) return;
        sfxAudioSourceList[0].clip = sfxClips[clipId];
        sfxAudioSourceList[0].loop = true;
        sfxAudioSourceList[0].Play();
    }

    public void StopSfxLoop()
    {
        sfxAudioSourceList[0].loop = false;
        sfxAudioSourceList[0].Stop();
    }

    public void StopSfx()
    {
        for (int i = 0; i < sfxAudioSourceList.Count; ++i)
        {
            sfxAudioSourceList[i].Stop();
        }
    }

    public void PauseSfx()
    {
        for (int i = 0; i < sfxAudioSourceList.Count; ++i)
        {
            sfxAudioSourceList[i].Pause();
        }
    }
    
    public void ResumeSfx()
    {
        for (int i = 0; i < sfxAudioSourceList.Count; ++i)
        {
            sfxAudioSourceList[i].UnPause();
        }
    }
    
    public void SetBgmVolume(float volume)
    {
        volume = Mathf.Clamp(volume, 0.0001f, 1f);
        
        bgmVolume = volume;
        
        bgmAudioSource.volume = bgmVolume;
    }

    public void SetSfxVolume(float volume)
    {
        volume = Mathf.Clamp(volume, 0.0001f, 1f);
        
        sfxVolume = volume;
        
        foreach (var sfxAudioSource in sfxAudioSourceList)
        {
            sfxAudioSource.volume = sfxVolume;
        }
    }
    
    private void OnBgmVolumeChangedHandler(float bgmVolume, float sfxVolume)
    {
       SetBgmVolume(bgmVolume);
       SetSfxVolume(sfxVolume);
       
       GameDataManager.Instance.PlayerAccountData.BgmVolume = bgmVolume;
       GameDataManager.Instance.PlayerAccountData.SfxVolume = sfxVolume;
       
       Debug.Log($"BGM Value : {this.bgmVolume}");
       Debug.Log($"SFX Value : {this.sfxVolume}");
    }
}