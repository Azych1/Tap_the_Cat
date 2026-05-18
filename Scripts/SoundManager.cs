using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    [Header("Основные настройки")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private bool isMusicEnabled = true;
    [SerializeField] private bool isSfxEnabled = true;

    [Range(0f, 1f)][SerializeField] private float musicVolume = 0.5f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 0.7f;

    [Space]
    [Header("Музыкальные треки")]
    [SerializeField] private AudioClip[] musicTracks; // Массив песен для проигрывания по очереди

    [Space]
    [Header("Звуковые эффекты (префабы)")]
    [SerializeField] private GameObject clickMainObject;
    [SerializeField] private GameObject upgradeType1Object;
    [SerializeField] private GameObject upgradeType2Object;
    [SerializeField] private GameObject levelUpObject;

    private int currentTrackIndex = 0;
    private bool audioInitialized = false;
    private Coroutine musicPlayerCoroutine;

    public static SoundManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Создаём AudioSource для музыки, если его нет
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }
        musicSource.loop = false; // Не зацикливаем один трек, управляем очередью сами
        musicSource.volume = musicVolume;
    }

    void Start()
    {
        // Для WebGL: инициализируем аудио только после первого взаимодействия
        // Не запускаем музыку автоматически
    }

    // Метод для инициализации аудио после первого клика
    public void InitializeAudio()
    {
        if (!audioInitialized)
        {
            audioInitialized = true;
            PlayMusic();
        }
    }

    // Запуск музыки с первого трека
    private void PlayMusic()
    {
        if (!isMusicEnabled || musicTracks == null || musicTracks.Length == 0) return;

        currentTrackIndex = 0;
        PlayTrack(currentTrackIndex);
    }

    // Воспроизведение конкретного трека по индексу и запуск ожидания окончания
    private void PlayTrack(int index)
    {
        if (!isMusicEnabled || musicTracks == null || index < 0 || index >= musicTracks.Length) return;

        musicSource.clip = musicTracks[index];
        musicSource.Play();

        // Запускаем корутину для переключения на следующий трек после окончания текущего
        if (musicPlayerCoroutine != null)
            StopCoroutine(musicPlayerCoroutine);
        musicPlayerCoroutine = StartCoroutine(WaitForTrackEnd());
    }

    private IEnumerator WaitForTrackEnd()
    {
        // Ждём, пока трек играет
        while (musicSource.isPlaying)
        {
            yield return null;
        }
        // После окончания переключаем на следующий
        PlayNextTrack();
    }

    private void PlayNextTrack()
    {
        if (!isMusicEnabled || musicTracks == null || musicTracks.Length == 0) return;

        currentTrackIndex = (currentTrackIndex + 1) % musicTracks.Length; // Зацикливание
        PlayTrack(currentTrackIndex);
    }

    // Остановка музыки (например, при выключении звука)
    private void StopMusic()
    {
        if (musicPlayerCoroutine != null)
        {
            StopCoroutine(musicPlayerCoroutine);
            musicPlayerCoroutine = null;
        }
        musicSource.Stop();
    }

    // Внешний метод для принудительного переключения на следующий трек (может пригодиться)
    public void SkipToNextTrack()
    {
        if (!isMusicEnabled || musicTracks == null || musicTracks.Length == 0) return;
        PlayNextTrack();
    }

    // Управление звуками
    public void MuteAll()
    {
        SetMusicVolume(0f);
        SetSfxVolume(0f);
        isMusicEnabled = false;
        isSfxEnabled = false;
        StopMusic();
    }

    public void UnmuteAll()
    {
        // Восстанавливаем громкость
        SetMusicVolume(0.3f); // Можно сделать настраиваемым
        SetSfxVolume(0.28f);
        isMusicEnabled = true;
        isSfxEnabled = true;

        // Если музыка была остановлена и инициализирована, запускаем заново
        if (audioInitialized)
        {
            PlayMusic();
        }
    }

    // Воспроизведение звуковых эффектов
    private void PlaySfx(GameObject sfxPrefab)
    {
        if (!isSfxEnabled || sfxPrefab == null) return;

        GameObject sfxGO = Instantiate(sfxPrefab, transform);
        AudioSource sfxSource = sfxGO.GetComponent<AudioSource>();
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
            sfxSource.Play();
            float clipLength = sfxSource.clip.length;
            Destroy(sfxGO, clipLength + 0.1f);
        }
    }

    public void PlayMainClick() => PlaySfx(clickMainObject);
    public void PlayUpgradeType1() => PlaySfx(upgradeType1Object);
    public void PlayUpgradeType2() => PlaySfx(upgradeType2Object);
    public void PlayLevelUp() => PlaySfx(levelUpObject);

    // Управление громкостью
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp(volume, 0f, 1f);
        if (musicSource != null)
            musicSource.volume = musicVolume;
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp(volume, 0f, 1f);
    }
}