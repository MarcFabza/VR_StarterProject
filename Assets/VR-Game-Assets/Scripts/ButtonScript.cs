using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class ButtonScript : MonoBehaviour
{
    public TMP_Text textGameStartsIn;   // Text_GameStartsIn
    public TMP_Text textStartCounter;   // Text_StartCounter

    private bool pressed = false;

    public UnityEvent OnPressed;

    void Start()
    {
        // Al inicio los dos textos están desactivados
        textGameStartsIn.enabled = false;
        textStartCounter.enabled = false;
    }

    public void Pressed()
    {
        if (pressed) return;

        pressed = true;

        // Al pulsar el botón se activan los dos textos
        textGameStartsIn.enabled = true;
        textStartCounter.enabled = true;

        textGameStartsIn.text = "Game Starts In:";

        OnPressed?.Invoke();

        StartCoroutine(Countdown());
    }

    IEnumerator Countdown()
    {
        yield return new WaitForSeconds(3f);

        for (int i = 3; i >= 1; i--)
        {
            textStartCounter.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        SceneManager.LoadScene("GameScene");
    }
}