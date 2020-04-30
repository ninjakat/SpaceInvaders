using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[ExecuteInEditMode]
public class GameManager : MonoBehaviour
{
    static private GameManager s_Instance = null;

    [SerializeField] Transform m_WaveStart = null;
    [SerializeField] Transform m_PlayerStart = null;

    [SerializeField] GameObject m_InvaderPrefab = null;
    [SerializeField] GameObject m_PlayerPrefab = null;

    [SerializeField] GameSettings m_GameSettings;

    public static float invaderAmplitude { get => s_Instance ? s_Instance.m_GameSettings.invaderAmplitude : 1f; }
    public static float invaderPeriod { get => s_Instance ? s_Instance.m_GameSettings.invaderPeriod : 1f; }
    public static float invaderExponent
    {
        get
        {
            if (s_Instance == null)
                return 1f;
            if (s_Instance.m_GameSettings.invertExponent)
                return 1f / s_Instance.m_GameSettings.invaderExponent;
            else
                return s_Instance.m_GameSettings.invaderExponent;
        }
    }
    public static float invaderSpeed { get => s_Instance ? s_Instance.m_GameSettings.invaderSpeed : 1f; }

    public static bool playerCanShoot { get => s_Instance.m_PlayerCanShoot; }
    public static bool playerCanMove { get => s_Instance.m_PlayerCanMove; }

    public static event UnityAction onWaveCleared;
    public static event UnityAction<int> onScoreChanged;
    public static event UnityAction<int> onWaveStarted;
    public static event UnityAction onGameOver;

    List<GameObject> m_Invaders;
    bool m_PlayerCanShoot;
    bool m_PlayerCanMove;
    int m_Score;
    Coroutine m_ShootRoutine;
    Coroutine m_WaveCleared;
    GameObject m_Player;
    GameSettings m_OriginalSettings;
    int m_CurrentWave;

    private void Awake()
    {
        // Singleton
        Debug.Assert(s_Instance == null);
        s_Instance = this;

        // Allow the player to move even in the menus
        m_PlayerCanShoot = false;
        m_PlayerCanMove = true;

        // Find or spawn player
        SetupPlayer();

        // Save original difficulty settings
        m_OriginalSettings = m_GameSettings;
    }

    private void Update()
    {
        // Cheat to increase difficulty
        if (Input.GetKeyDown("k"))
        {
            m_GameSettings.IncreaseDifficulty();
            ++s_Instance.m_CurrentWave;
            onWaveStarted?.Invoke(s_Instance.m_CurrentWave);
        }

        if (Input.GetKeyDown("escape"))
        {
            Application.Quit();
        }
    }

    public static void StartGame()
    {
        // Restore difficulty settings
        s_Instance.m_GameSettings = s_Instance.m_OriginalSettings;

        // Reset score
        s_Instance.m_Score = 0;
        onScoreChanged?.Invoke(0);
        s_Instance.m_CurrentWave = 1;
        onWaveStarted?.Invoke(s_Instance.m_CurrentWave);

        // Spawn invaders
        s_Instance.StartWave();
    }

    private void SetupPlayer()
    {
        if (m_Player == null)
        {
            const string playerName = "Player";
            m_Player = GameObject.Find(playerName);
            if (m_Player == null)
            {
                m_Player = GameObject.Instantiate(m_PlayerPrefab);
                m_Player.name = playerName;
                m_Player.transform.position = m_PlayerStart.position;
            }
            m_Player.GetComponent<Player>().onDeath += OnPlayerDeath;
        }
    }

    public static void NextWave()
    {
        s_Instance.m_GameSettings.IncreaseDifficulty();
        s_Instance.StartWave();

        ++s_Instance.m_CurrentWave;
        onWaveStarted?.Invoke(s_Instance.m_CurrentWave);
    }

    public static void ResetGame()
    {
        foreach (GameObject obj in s_Instance.m_Invaders)
        {
            GameObject.Destroy(obj);
        }

        if (s_Instance.m_Player != null)
        {
            s_Instance.m_Player.name = "Dead Player";
            Destroy(s_Instance.m_Player);
            s_Instance.m_Player = null;
        }
        s_Instance.SetupPlayer();

        s_Instance.m_PlayerCanShoot = false;
        s_Instance.m_PlayerCanMove = true;
    }

    private void StartWave()
    {
        // Spawn invaders
        int width = m_GameSettings.waveWidth;
        int height = m_GameSettings.waveHeight;
        m_Invaders = new List<GameObject>(width * height);

        for (int j = 0; j < height; ++j)
        {
            for (int i = 0; i < width; ++i)
            {
                GameObject invader = GameObject.Instantiate(m_InvaderPrefab, m_WaveStart);
                invader.transform.localPosition = new Vector3(i - 0.5f * width + 0.5f, 0, j);
                m_Invaders.Add(invader);

                Invader script = invader.GetComponent<Invader>();
                script.onDeath += OnInvaderDeath;
                script.onReachBottom += OnInvaderReachedBottom;
            }
        }

        // Make invaders shoot
        m_ShootRoutine = StartCoroutine(RandomShoot());

        // Allow player to shoot
        m_PlayerCanShoot = true;
        m_PlayerCanMove = true;
    }

    private IEnumerator RandomShoot()
    {
        while (true)
        {
            float t = Random.Range(m_GameSettings.waveFireDelayRange.x, m_GameSettings.waveFireDelayRange.y);
            t = Mathf.Max(t, 0.1f);
            yield return new WaitForSeconds(t);
            int i = Random.Range(0, m_Invaders.Count);
            m_Invaders[i].GetComponent<Invader>().Shoot();
        }
    }

    private void OnInvaderDeath(GameObject invader)
    {
        // Update score
        ++m_Score;
        onScoreChanged?.Invoke(m_Score);

        // Check if the wave was cleared
        m_Invaders.Remove(invader);
        if (m_Invaders.Count == 0)
        {
            // Stop invaders shooting
            StopCoroutine(m_ShootRoutine);

            // Wait a bit before signaling, in case the last invader simultaneously killed the player
            m_WaveCleared = StartCoroutine(WaveCleared());
        }
    }

    private IEnumerator WaveCleared()
    {
        yield return new WaitForSeconds(2f);

        // Check the player hasn't died
        if (m_Player != null)
        {
            // Back in menus, allow to the player to move around still
            m_PlayerCanShoot = false;
            m_PlayerCanMove = true;

            onWaveCleared?.Invoke();
        }
    }

    private void OnPlayerDeath(GameObject player)
    {
        GameOver();
    }

    private void OnInvaderReachedBottom(GameObject invader)
    {
        GameOver();
    }

    private void GameOver()
    {
        // Stop the invaders (but keep them on display)
        foreach (GameObject obj in m_Invaders)
        {
            obj.GetComponent<Invader>().enabled = false;
        }

        // Stop invaders shooting
        if (m_ShootRoutine != null)
        {
            StopCoroutine(m_ShootRoutine);
        }

        if (m_WaveCleared != null)
        {
            StopCoroutine(m_WaveCleared);
        }

        // Stop player from moving
        m_PlayerCanShoot = false;
        m_PlayerCanMove = false;

        onGameOver?.Invoke();
    }
}
