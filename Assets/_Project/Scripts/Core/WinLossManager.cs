using UnityEngine;

namespace Core
{
    /// <summary>
    /// Handles win/loss state changes. FlowManager triggers these.
    /// </summary>
    public class WinLossManager : MonoBehaviour
    {
        public void OnTrafficJam()
        {
            Debug.LogError("GAME OVER: Traffic Jam!");
            if (GameManager.Instance != null)
                GameManager.Instance.SetState(GameState.GameOver);
        }

        public void OnLevelCleared()
        {
            Debug.Log("VICTORY: Level Cleared!");
            if (GameManager.Instance != null)
                GameManager.Instance.SetState(GameState.Victory);
        }
    }
}
