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
        { 1, 1 },
        { 2, 2 },
        { 3, 3 },
    };

    [HideInInspector]
    public Dictionary<int, Vector2> levelPos = new Dictionary<int, Vector2>()
    {
        { 1, new Vector2(-1, -2.3f) },
    };

    public int NumAttempts() { return levelAttempts[number]; }
    public Vector2 StartPos() { return levelPos[number]; }
}
