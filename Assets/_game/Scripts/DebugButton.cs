using UnityEngine;

public class DebugButton : MonoBehaviour
{
    public void ButtonClicked_SellTestGame()
    {
        Game game = new Game($"Test{Random.Range(10000, 99999)}", GameManager.Instance.Genres[Random.Range(0, GameManager.Instance.Genres.Length)].Name, 1);
        DevelopGameManager.OnGameReleased?.Invoke(game);
        Debug.Log($"{game.Name} | {game.Genre}");
    }

    public void ButtonClicked_SetTimeScale(float timeScale)
    {
        Debug.Log($"Timescale set to {timeScale}");
        Time.timeScale = timeScale;
    }
}
