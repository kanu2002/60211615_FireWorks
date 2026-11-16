using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class FireworksController : MonoBehaviour
{
    const string DefaultBgmPath = "Audios/BGMs/bgm_Fireworks";
    const double AudioScheduleLeadTime = 0.15;

    [SerializeField] AudioClip _bgmClip;
    [SerializeField] Phase[] _phases;

    AudioSource _bgmSource;
    readonly List<ParticleSystem> _activeFireworks = new();

    private void Awake()
    {
        GameObject bgmObject = GameObject.Find("/ObjsRoot/Audios/BGM");
        if (bgmObject != null && bgmObject.TryGetComponent(out _bgmSource))
        {
            _bgmSource.playOnAwake = false;
            _bgmSource.Stop();
        }
    }

    private IEnumerator Start()
    {
        AudioClip clip = _bgmClip;
        if (clip == null)
            clip = Resources.Load<AudioClip>(DefaultBgmPath);
        if (clip == null && _bgmSource != null)
            clip = _bgmSource.clip;
        if (clip != null && clip.loadState == AudioDataLoadState.Unloaded)
            clip.LoadAudioData();

        PageBase page = FindFirstObjectByType<PageBase>();
        while (page != null && !page.IsReadyForPresentation)
            yield return null;
        while (clip != null && clip.loadState == AudioDataLoadState.Loading)
            yield return null;

        double startTime = AudioSettings.dspTime;
        if (_bgmSource != null && clip != null && clip.loadState == AudioDataLoadState.Loaded)
        {
            _bgmSource.enabled = true;
            _bgmSource.clip = clip;
            startTime += AudioScheduleLeadTime;
            _bgmSource.PlayScheduled(startTime);
        }

        yield return PlayPhases(startTime);
    }

    IEnumerator PlayPhases(double startTime)
    {
        foreach (Phase phase in _phases)
        {
            double cueTime = startTime + phase._fStartTime;
            while (AudioSettings.dspTime < cueTime)
                yield return null;

            PlayCurrentParticleSystems(phase);
        }
    }

    void PlayCurrentParticleSystems(Phase currentPhase)
    {
        foreach (ParticleSystem fireworks in _activeFireworks)
        {
            if (fireworks != null)
                fireworks.Stop();
        }

        _activeFireworks.Clear();
        foreach (ParticleSystem prefab in currentPhase._pFireworks)
        {
            if (prefab == null)
                continue;

            ParticleSystem fireworks = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            _activeFireworks.Add(fireworks);
            fireworks.Play();
        }
    }
}

[Serializable]
public class Phase
{
    public float _fStartTime;
    public ParticleSystem[] _pFireworks;
}
