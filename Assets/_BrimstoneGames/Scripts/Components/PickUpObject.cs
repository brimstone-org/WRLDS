using UnityEngine;

namespace _DPS
{
    [RequireComponent(typeof(Collider2D))]
    public class PickUpObject : MonoBehaviour
    {
        public bool Respawn;
        public GameObject Effect, PickUp;
        private EffectOnPlayer PlayerEffect;
        private float effectTimer = 5f;
        private bool triggered;
        private SpriteRenderer renderer;
        private GameObject[] rendrerChildren;
        private Collider2D collider;
        private bool picked;
        void Start()
        {
            if (PlayerEffect == null)
            {
                PlayerEffect = GetComponent<EffectOnPlayer>();
            }


            collider = GetComponent<Collider2D>();


            if (PickUp != null)
            {
                if (PickUp.GetComponent<SpriteRenderer>())
                {
                    renderer = PickUp.GetComponent<SpriteRenderer>();
                }

                if (renderer != null)
                {
                    rendrerChildren = new GameObject[renderer.transform.childCount];
                    for (int i = 0; i < rendrerChildren.Length; i++)
                    {
                        rendrerChildren[i] = renderer.transform.GetChild(i).gameObject;
                    }
                }
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && !other.isTrigger)
            {
                triggered = true;
                if(Effect!=null)
                    Effect.SetActive(true);
                if (PlayerEffect != null && !picked)
                {
                    picked = false;
                    GameManager.ApplyEffect?.Invoke(PlayerEffect);
                }

                if (PickUp != null)
                {
                    TogglePickup(false);
                }
            }
        }

        private void TogglePickup(bool turnOn)
        {
            if (collider != null)
            {
                collider.enabled = turnOn;
            }

            if (renderer != null)
            {
                renderer.enabled = turnOn;

                for (int i = 0; i < rendrerChildren.Length; i++)
                {
                    rendrerChildren[i].SetActive(turnOn);
                }
            }
        }

        void Update()
        {
            if(!Respawn) return; 
            //rearm
            if (triggered)
            {
                effectTimer -= Time.deltaTime;
                if (effectTimer <= 0)
                {
                    if (Effect != null)
                    {
                        Effect.SetActive(false);
                    }

                    if (PickUp != null)
                    {
                        TogglePickup(true);
                    }

                    effectTimer = 5f;
                    GetComponent<EffectOnPlayer>().PopulateEffect();
                    triggered = false;

                }
            }
        }
    }
}
