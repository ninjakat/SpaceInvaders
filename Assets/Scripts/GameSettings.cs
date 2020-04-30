using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct GameSettings
{
    [Header("Invaders Settings")]
    [SerializeField] public float invaderAmplitude;
    [SerializeField] public float invaderPeriod;
    [SerializeField] public float invaderExponent;
    [SerializeField] public float invaderSpeed;

    [Header("Wave Settings")]
    [SerializeField] public int waveWidth;
    [SerializeField] public int waveHeight;
    [SerializeField] public Vector2 waveFireDelayRange;

    [Header("Difficulty Increase Settings")]
    [SerializeField] public float amplitudeIncrease;
    [SerializeField] public float periodIncrease;
    [SerializeField] public float exponentIncrease;
    [SerializeField] public bool invertExponent;
    [SerializeField] public float speedIncrease;
    [SerializeField] public float fireDelayIncreaseIncrease;
    [SerializeField] public int waveHeightIncrease;

    public void IncreaseDifficulty()
    {
        invaderAmplitude += amplitudeIncrease;
        invaderPeriod += periodIncrease;
        invaderExponent += exponentIncrease;
        invertExponent = !invertExponent;
        invaderSpeed += speedIncrease;
        waveFireDelayRange.x += fireDelayIncreaseIncrease;
        waveFireDelayRange.y += fireDelayIncreaseIncrease;
        waveHeight += waveHeightIncrease;
    }
}
