using UnityEngine;


namespace _DPS
{
    public class KillerRotator : MonoBehaviour
    {
        public int speed;
        private Vector3 originalPos;
        // Update is called once per frame
        void Update()
        {
            transform.RotateAround(originalPos, Vector3.forward, speed * Time.deltaTime);
        }

        void OnEnable()
        {
            speed = Random.Range(35, 50);
            originalPos = transform.position;
            transform.position = new Vector3(transform.position.x  /*RandomSign()*/, transform.position.y+ Random.Range(5, 10), transform.position.z);
            transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.right);
        }

        private int RandomSign()
        {
            return Random.value < 0.5f ? 1 : -1;
        }
    }

}

