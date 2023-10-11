using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundType
{
    Bgm,
    Effect,
    MaxCount,
}

public class SoundManager : SingletonMono<SoundManager>
{
    [SerializeField] private List<AudioSource> audioSourceLists;

    Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();
    protected override void OnSingletonAwake()
    {
        base.OnSingletonAwake();
        Init();
        Clear();
    }
    public void Init()
    {
        audioSourceLists[(int)SoundType.Bgm].loop = true;
    }

    public void Clear()
    {
        // 재생기 전부 재생 스탑, 음반 빼기
        foreach (AudioSource audioSource in audioSourceLists)
        {
            audioSource.clip = null;
            audioSource.Stop();
        }
        // 효과음 Dictionary 비우기
        _audioClips.Clear();
    }
    public void Play(AudioClip audioClip, SoundType type = SoundType.Effect, float pitch = 1.0f)
    {
        if (audioClip == null)
            return;

        if (type == SoundType.Bgm) // BGM 배경음악 재생
        {
            AudioSource audioSource = audioSourceLists[(int)type];
            if (audioSource.isPlaying)
                audioSource.Stop();

            audioSource.pitch = pitch;
            audioSource.clip = audioClip;
            audioSource.Play();
        }
        else // Effect 효과음 재생
        {
            AudioSource audioSource = audioSourceLists[(int)type];
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(audioClip);
        }
    }

    public void Play(string path, SoundType type = SoundType.Effect, float pitch = 1.0f)
    {
        AudioClip audioClip = GetOrAddAudioClip(path, type);
        Play(audioClip, type, pitch);
    }

    AudioClip GetOrAddAudioClip(string path, SoundType type = SoundType.Effect)
    {
        if (path.Contains("Sounds/") == false)
            path = $"Sounds/{path}"; 

        AudioClip audioClip = null;

        if (type == SoundType.Bgm) // BGM 배경음악 클립 붙이기
        {
            audioClip = Resources.Load<AudioClip>(path);
        }
        else // Effect 효과음 클립 붙이기
        {
            if (_audioClips.TryGetValue(path, out audioClip) == false)
            {
                audioClip = Resources.Load<AudioClip>(path);
                _audioClips.Add(path, audioClip);
            }
        }

        if (audioClip == null)
            Debug.Log($"AudioClip Missing ! {path}");

        return audioClip;
    }
}
