using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public sealed class PlaygroundControls : MonoBehaviour
{
    [SerializeField] private Transform character;
    private const float FallResetHeight = -12f;
    private bool restarting;

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            Application.Quit();

        if (restarting)
            return;

        bool requestedReset = keyboard != null && keyboard.rKey.wasPressedThisFrame;
        bool fellOffTrack = character && character.position.y < FallResetHeight;
        if (requestedReset || fellOffTrack)
        {
            restarting = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
