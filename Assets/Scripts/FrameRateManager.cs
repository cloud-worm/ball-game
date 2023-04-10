using System.Collections;
using System.Threading;
using UnityEngine;

namespace BallGame
{
    public class FrameRateManager : MonoBehaviour
    {
        [Header("Frame Settings")]
        [SerializeField] private float targetFrameRate = 60f;

        private int maxRate = 9999;

        private float currentFrameTime;

        private void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = maxRate;
            currentFrameTime = Time.realtimeSinceStartup;
            StartCoroutine(WaitForNextFrame());
        }

        private IEnumerator WaitForNextFrame()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                currentFrameTime += 1f / targetFrameRate;
                float t = Time.realtimeSinceStartup;
                float sleepTime = currentFrameTime - t - .01f;
                if (sleepTime > 0)
                    Thread.Sleep((int)(sleepTime * 1000));
                while (t < currentFrameTime)
                    t = Time.realtimeSinceStartup;
            }
        }
    }
}