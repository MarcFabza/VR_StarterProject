using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class ButtonScript : MonoBehaviour
{
    public TMP_Text textGameStartsIn;
    public TMP_Text textStartCounter;

    private bool pressed = false;

    public UnityEvent OnPressed;

    void Start()
    {
        textGameStartsIn.enabled = false;
        textStartCounter.enabled = false;
    }

    public void Pressed()
    {
        if (pressed) return;

        pressed = true;

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