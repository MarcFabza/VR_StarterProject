using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

namespace Autohand{
    public class PhysicsGadgetButton : PhysicsGadgetConfigurableLimitReader{
        bool pressed = false;

        [Tooltip("The percentage (0-1) from the required value needed to call the event, if threshold is 0.1 OnPressed will be called at 0.9, and OnUnpressed at 0.1"), Min(0.01f)]
        public float threshold = 0.1f;
        public bool lockOnPressed = false;
        [Space]
        public UnityEvent OnPressed;
        public UnityEvent OnUnpressed;

        public GameObject textGameStartsIn;
        public GameObject textStartCounter;
        public string sceneName = "GameScene";

        [Header("Best score")]
        public GameObject textBestScore;
        public string highScoreKey = "HighScore";
        public float bestScoreY = 70f;

        [Header("Sonido")]
        public AudioClip clickSound;
        [Range(0f, 1f)] public float clickVolume = 1f;

        bool countdownStarted = false;

        Vector3 startPos;
        Vector3 pressedPos;
        float pressedValue;

        Rigidbody body;

        new protected void Start(){
            base.Start();
            startPos = transform.localPosition;
            body = joint.GetComponent<Rigidbody>();

            textGameStartsIn.SetActive(false);
            textStartCounter.SetActive(false);

            ShowBestScore();
        }

        void ShowBestScore(){
            if(textBestScore == null){
                textBestScore = Instantiate(textGameStartsIn, textGameStartsIn.transform.parent);
                textBestScore.name = "Text_BestScore";
                textBestScore.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, bestScoreY);
            }

            textBestScore.SetActive(true);
            textBestScore.GetComponent<TMP_Text>().text = "Best Score: " + PlayerPrefs.GetInt(highScoreKey, 0);
        }

        protected void FixedUpdate(){
            var value = GetValue();
            if(!pressed && value+threshold >= 1)
                Pressed();
            else if(!lockOnPressed && pressed && value-threshold <= 0)
                Unpressed();
        }

        public void Pressed(){
            pressed = true;
            pressedValue = GetValue();
            pressedPos = transform.localPosition;
            OnPressed?.Invoke();
            if(lockOnPressed){
                body.isKinematic = true;
            }
            Debug.Log("Button Pressed");

            if(countdownStarted) return;
            countdownStarted = true;

            if(clickSound != null)
                AudioSource.PlayClipAtPoint(clickSound, transform.position, clickVolume);

            textGameStartsIn.SetActive(true);
            textStartCounter.SetActive(true);

            textGameStartsIn.GetComponent<TMP_Text>().text = "Game Starts In:";
            textStartCounter.GetComponent<TMP_Text>().text = "3";

            GameObject helper = new GameObject("CountdownHelper");
            CountdownRunner runner = helper.AddComponent<CountdownRunner>();
            runner.counter = textStartCounter.GetComponent<TMP_Text>();
            runner.sceneName = sceneName;
            runner.StartCountdown();
        }

        public void Unpressed(){
            pressed = false;
            OnUnpressed?.Invoke();
        }

        public void Unlock(){
            lockOnPressed = false;
            body.isKinematic = false;
            body.WakeUp();
        }
    }

    public class CountdownRunner : MonoBehaviour{
        public TMP_Text counter;
        public string sceneName;

        public void StartCountdown(){
            StartCoroutine(Countdown());
        }

        IEnumerator Countdown(){
            for(int i = 3; i >= 1; i--){
                counter.text = i.ToString();
                Debug.Log("Cuenta atrás: " + i);
                yield return new WaitForSecondsRealtime(1f);
            }

            Debug.Log("Cargando escena " + sceneName);
            SceneManager.LoadScene(sceneName);
        }
    }
}