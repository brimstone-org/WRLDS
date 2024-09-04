using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _DPS
{
    public class HudManager : Singleton<HudManager>
    {
        protected HudManager(){}
        [Header("Refs")]
        public GameObject LifeHolder;
        public GameObject ArmorHolder;
        public GameObject BuffHolder;
        public TextMeshProUGUI ScoreText, BottomText;
        public Image DirectionImage, PlusImage, DeathWrap;
        [Header("Progress Refs")] 
        public Image ProgressBackground;
        public GameObject ProgressBubblePrefab;
        [HideInInspector] 
        public Image[] ProgressBubbles;
        [Header("Boss Hp Refs")]
        [SerializeField]
        private Image _bossHpBar;
        [SerializeField]
        private TextMeshProUGUI _bossHpTxt;
        private Coroutine timer;
        
        public enum ArrowPoint
        {
            Up = 0,
            TopLeft = 45,
            Left = 90,
            Down = 180,
            Right = 270,
            TopRight = 315
        }

        //public refs
        public Dictionary<int, GameObject> Buffs = new Dictionary<int, GameObject>();
        [Space]
        public GameObject LifePrefab, ArmorPrefab, BuffPrefab;
        public Sprite ArrowSprite, DoubleArrowSprite, PlusSprite, MinusSprite;
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                Destroy(gameObject);
            }
            DontDestroyOnLoad(Instance);
            SetPlusImage();
            SetBottomText("",0);
            ToggleBossHp(false);
            DeathWrap.gameObject.SetActive(false);
        }

        public void SetBossHp(int bossHp, float fill)
        {
            _bossHpBar.fillAmount = fill;
            _bossHpTxt.text = bossHp.ToString("N0");
        }

        public void FireDeathWrap()
        {
            DeathWrap.gameObject.SetActive(true);
            DeathWrap.DOColor(new Color(0,0,0,0), 0f).OnComplete((() =>
            {
                DeathWrap.DOFade(1f, 1f).SetEase(Ease.InOutQuad).SetUpdate(true).SetAutoKill().OnComplete((() =>
                {
                    DeathWrap.gameObject.SetActive(false);
                }));
            }));


        }

        public void ToggleBossHp(bool turnOn)
        {
            _bossHpBar.gameObject.SetActive(turnOn);
            _bossHpTxt.gameObject.SetActive(turnOn);
        }
        /// <summary>
        /// concatatenation of the old SetArrowImage and SetArrowDirection methods
        /// </summary>
        /// <param name="singleArrow"></param>
        /// <param name="arrowPoint"></param>
        public void SetArrowImageAndDirection(bool singleArrow = true, ArrowPoint arrowPoint = ArrowPoint.TopRight)
        {
            DirectionImage.sprite = singleArrow ? ArrowSprite : DoubleArrowSprite;
            DirectionImage.transform.localRotation = Quaternion.Euler(0, 0, (float)arrowPoint);
        }

        public void SetArrowImage(bool singleArrow = true)
        {
            DirectionImage.sprite = singleArrow? ArrowSprite : DoubleArrowSprite;
           
        }

        public void SetPlusImage(bool enabled = false, bool plus = true)
        {
            PlusImage.enabled = enabled;
            if (!enabled) return;

            PlusImage.sprite = plus ? PlusSprite : MinusSprite;
        }
        public void SetArrowDirection(ArrowPoint arrowPoint = ArrowPoint.TopRight)
        {
            //global::Logger.Log("~~~~ Set arrow dir " + (int)arrowPoint);
            DirectionImage.transform.localRotation = Quaternion.Euler(0,0, (float)arrowPoint);
        }

        public void SetBottomText(string text = "", float timeToDisplay = 5f)
        {
            BottomText.text = text;
        
            if (timer != null)
            {
                StopCoroutine(timer);
            }

            timer = StartCoroutine(TextDelay(timeToDisplay));
        }

        private IEnumerator TextDelay(float timeToDisplay)
        {
            yield return new WaitForSeconds(timeToDisplay);
            BottomText.text = "";
        }

        public void CleanHud()
        {
            //cleanup
            for (int i = 0; i < HudManager.Instance.ProgressBackground.transform.childCount; i++)
            {
                Destroy(HudManager.Instance.ProgressBackground.transform.GetChild(i).gameObject);
            }
        }

        void Update()
        {
            if(!PlusImage.enabled) return;
            PlusImage.color = PlusImage.sprite == PlusImage ? Color.green : Color.red;
        }
    }
}
