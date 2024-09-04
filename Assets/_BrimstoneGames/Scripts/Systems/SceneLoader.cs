using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace _DPS
{
    public class SceneLoader : Singleton<SceneLoader>
    {
        protected SceneLoader(){}
        public float LoadTimer = 5f;
        public GameObject MainMenu;
        public Canvas Canvas;
        public GameObject ScrollButtonPrefab;
        public Transform ScrollHolder;
        public HorizontalScrollSnap HorizontalScrollSnap;
        public List<Sprite> LoadingBackgrounds = new List<Sprite>();
        public List<Sprite> LoadingBackgroundsNew = new List<Sprite>();
        public List<string> LoadingQuotes = new List<string>();
        public Image FillTimer, BackgroundHolder;
        public TextMeshProUGUI QuoteHolder;
        public Coroutine timerbar;

        private List<ScrollButtonParams> _scrollButtonsParams =  new List<ScrollButtonParams>();

        void Awake()
        {
        
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                Destroy(gameObject);
            }
            DontDestroyOnLoad(Instance);
            //SceneManager.sceneLoaded += OnSceneLoaded;


            PopulateMainMenu();
        }

        #region MainMenu

        //popualte menu
        private void PopulateMainMenu()
        {
            //prepare buttons
            var lastScene = GameManager.Instance.GetPlayerParams().LastScene;
            for (int i = 0; i < LoadingBackgroundsNew.Count; i++)
            {
                var scrollButtonGO = Instantiate(ScrollButtonPrefab, ScrollHolder);
                var scrollButtonParams = scrollButtonGO.GetComponent<ScrollButtonParams>();
                scrollButtonParams.SceneId = i + 1;
                scrollButtonParams.ScrollButtonImage.overrideSprite = LoadingBackgroundsNew[i];
                scrollButtonParams.enabled = true;
                //enable disable unlocked scenes
                scrollButtonParams.ScrollButton.interactable = scrollButtonParams.SceneId <= lastScene;
                _scrollButtonsParams.Add(scrollButtonParams);
            }

            HorizontalScrollSnap.StartingScreen = lastScene-1;
        }

        public void RefreshButtons()
        {
            var lastScene = GameManager.Instance.GetPlayerParams().LastScene;
            for (int i = 0; i < _scrollButtonsParams.Count; i++)
            {
                _scrollButtonsParams[i].ScrollButton.interactable = _scrollButtonsParams[i].SceneId <= lastScene;
            }
            
            HorizontalScrollSnap.StartingScreen = lastScene-1;
        }

        #endregion

        public void UnloadScene()
        {
            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            GameManager.Instance.Cleanup();
        }

        public void LoadScene(int sceneId)
        {
            //LoadRandomBg();
            LoadBackground(sceneId);
            Canvas.enabled = true;
            global::Logger.Log("scenetoload " + sceneId);
            StopAllCoroutines();
            StartCoroutine(LoadNextScene(sceneId));
        }
        private IEnumerator LoadNextScene(int sceneId)
        {
            yield return StartCoroutine(ProgressBar(sceneId));
        }

        private IEnumerator ProgressBar(int sceneId)
        {
            //if (timerbar == null)
            //{
            //    timerbar = StartCoroutine(FillTimerBar());
            //}
            //load next scene
            var loaded = SceneManager.LoadSceneAsync(sceneId, LoadSceneMode.Additive);
            global::Logger.Log("Scene " + sceneId + " Scenecount " + SceneManager.sceneCount);


            while (loaded.progress < 0.99f)
            {
                if (FillTimer != null)
                {
                    FillTimer.fillAmount = loaded.progress;
                }
                yield return null;
                if (FillTimer != null)
                {
                    FillTimer.fillAmount = 1;
                }
            }
        
        
            SceneManager.SetActiveScene(SceneManager.GetSceneAt(SceneManager.sceneCount -1));
            GameManager.CurrentScene = sceneId;
            GameManager.Instance.Init();


        
        }

        private IEnumerator FillTimerBar()
        {
            //fill bar
            var t = 0f;
            while (t < LoadTimer)
            {
                t += Time.deltaTime;
                t = Mathf.Clamp(t, 0, LoadTimer);
                var fill = Mathf.Clamp(t / LoadTimer, 0, 1);
                if (FillTimer != null)
                {
                    FillTimer.fillAmount = fill;
                }

                yield return null;
            }
        }

        private void LoadBackground(int sceneId)
        {
            BackgroundHolder.sprite = LoadingBackgrounds[sceneId-1];
        }

        private void LoadRandomBg()
        {
            if (LoadingBackgroundsNew.Count > 0)
            {
                var rng = UnityEngine.Random.Range(0, LoadingBackgroundsNew.Count);
                BackgroundHolder.sprite = LoadingBackgroundsNew[rng];
            }
        
            if (LoadingQuotes.Count > 0)
            {
                var rng = UnityEngine.Random.Range(0, LoadingQuotes.Count);
                QuoteHolder.text = LoadingQuotes[rng];
            }
        }
    }
}
