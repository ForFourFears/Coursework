using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

namespace Coursework.Managers
{
    public class SceneLoader : MonoBehaviour, ISceneInitializable
    {
        public UnityEvent OnExit;
        public UnityEvent OnEnter;

        [SerializeField] private AnimationClip _fadeInClip;
        [SerializeField] private AnimationClip _fadeOutClip;
        private WaitForSeconds waitFadeIn;
        private WaitForSeconds waitFadeOut;

        public static SceneLoader Instance { get; private set; }

        public void Initialize()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);


            waitFadeIn = new WaitForSeconds(_fadeInClip.length + 0.1f);
            waitFadeOut = new WaitForSeconds(_fadeOutClip.length + 0.1f);

            OnEnter?.Invoke();
        }

        public void ReloadCurrentScene()
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            StartCoroutine(TransitionCoroutine(currentSceneIndex));
        }

        public void LoadScene(string sceneName)
        {
            StartCoroutine(TransitionCoroutine(sceneName));
        }

        public void LoadScene(int sceneIndex)
        {
            StartCoroutine(TransitionCoroutine(sceneIndex));
        }

        private IEnumerator TransitionCoroutine(string sceneName)
        {
            OnExit?.Invoke();

            yield return waitFadeIn;

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            OnEnter?.Invoke();

            yield return waitFadeOut;
        }

        private IEnumerator TransitionCoroutine(int sceneIndex)
        {
            OnExit?.Invoke();

            yield return waitFadeIn;

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            OnEnter?.Invoke();

            yield return waitFadeOut;

        }

    }
}