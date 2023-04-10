using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDetails : MonoBehaviour
{
    [Header("Current scene level details")]
    public int number;

    [HideInInspector]
    public Dictionary<int, int> levelAttempts = new Dictionary<int, int>()
    {
        { 1, 2 },
        { 2, 1 },
        { 3, 2 },
    };

    public int NumAttempts() { return levelAttempts[number]; }
}
