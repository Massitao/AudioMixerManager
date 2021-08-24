using UnityEngine;

namespace Massitao.Pattern.Singleton
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance;

        protected virtual void Awake()
        {
            CheckInstance();
            SingletonAwake();
        }
        protected void CheckInstance()
        {
            if (Instance == null)
            {
                Instance = FindObjectOfType<T>();

                if (Instance == null)
                {
                    GameObject newSingleton = new GameObject(typeof(T).ToString());
                    Instance = newSingleton.AddComponent<T>();
                }
            }
        }
        protected void SingletonAwake()
        {
            if (Instance != this)
            {
                Destroy(this);
            }

            DontDestroyOnLoad(gameObject);
        }

        protected void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}