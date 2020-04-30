using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject m_StartButton = null;
    [SerializeField] GameObject m_NextWave = null;
    [SerializeField] GameObject m_Background = null;
    [SerializeField] GameObject m_Score = null;
    [SerializeField] GameObject m_Wave = null;
    [SerializeField] GameObject m_GameOver = null;

    enum Panel
    {
        Start, NextWave, Game, GameOver
    }

    private void SetPanelView(Panel view)
    {
        m_Background.SetActive(view != Panel.Game);
        m_StartButton.SetActive(view == Panel.Start);
        m_NextWave.SetActive(view == Panel.NextWave);
        m_GameOver.SetActive(view == Panel.GameOver);

        // Hide the cursor during game since it's not used
        Cursor.lockState = view == Panel.Game ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = view != Panel.Game;

        if (view == Panel.Start)
        {
            m_StartButton.GetComponent<Button>().Select();
        }
        else if (view == Panel.NextWave)
        {
            m_NextWave.GetComponent<Button>().Select();
        }
        else if (view == Panel.GameOver)
        {
            m_GameOver.GetComponentInChildren<Button>().Select();
        }
    }

    public void OnEnable()
    {
        GameManager.onWaveStarted += OnWaveStarted;
        GameManager.onWaveCleared += OnWaveCleared;
        GameManager.onScoreChanged += OnScoreChanged;
        GameManager.onGameOver += OnGameOver;
    }

    public void OnDisable()
    {
        GameManager.onWaveStarted -= OnWaveStarted;
        GameManager.onWaveCleared -= OnWaveCleared;
        GameManager.onScoreChanged -= OnScoreChanged;
        GameManager.onGameOver -= OnGameOver;
    }

    private void Start()
    {
        SetPanelView(Panel.Start);
        m_StartButton.GetComponent<Button>().Select();
    }

    public void StartGame()
    {
        SetPanelView(Panel.Game);
        GameManager.StartGame();
    }

    public void StartNextWave()
    {
        SetPanelView(Panel.Game);
        GameManager.NextWave();
    }

    public void BackToStart()
    {
        SetPanelView(Panel.Start);
        GameManager.ResetGame();
    }

    private void OnWaveStarted(int wave)
    {
        m_Wave.GetComponent<Text>().text = wave.ToString();
    }

    private void OnWaveCleared()
    {
        SetPanelView(Panel.NextWave);
    }

    private void OnScoreChanged(int score)
    {
        m_Score.GetComponent<Text>().text = score.ToString();
    }

    private void OnGameOver()
    {
        SetPanelView(Panel.GameOver);
    }
}
