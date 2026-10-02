using UnityEngine;
using UnityEngine.Events;

public class ButtonScript : MonoBehaviour
{

    public GameObject textHighScore;

    public GameObject textValue;

    public GameObject TimeText;

    bool pressed = false;

    public UnityEvent OnPressed;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Pressed()
    {
        pressed = true;
        OnPressed?.Invoke();


        Debug.Log("Button Pressed");
        Debug.Log("Button Pressed");
        Debug.Log("Button Pressed");
        Debug.Log("Button Pressed");
        Debug.Log("Button Pressed");
        Debug.Log("Button Pressed");
        Debug.Log("Button Pressed");
        Debug.Log("Button Pressed");
        Debug.Log("Button Pressed");
        Debug.Log("Button Pressed");
        Debug.Log("Button Pressed");
        Debug.Log("Button Pressed");
        Debug.Log("Button Pressed");
        Debug.Log("Button Pressed");
        Debug.Log("Button Pressed");




    }

}
