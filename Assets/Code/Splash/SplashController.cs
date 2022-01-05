using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace ho
{
    public class SplashController : MonoBehaviour
    {
        [Serializable]
        internal class SplashPair
        {
            public AssetReferenceSprite splashSpriteRef;
            public float showTime = 2f;

            [NonSerialized]
            public Sprite splashSprite;
            [NonSerialized]
            public AsyncOperationHandle<Sprite> spriteHandle;
        }

        [SerializeField]
        AudioClip splashMusic;
        [SerializeField]
        List<SplashPair> splashSetup = new List<SplashPair>();

        [SerializeField] SplashPair defaultSplashPair;

        [SerializeField]
        SpriteRenderer splashRenderer;
        [SerializeField]
        TextMeshProUGUI loadingText;
        [SerializeField]
        Camera splashCamera;

        float currentSplashTimer = 0f;
        int currentSplashIndex = -1;
        float leadInDelay = 0.5f;
        float loadTextFadeAlpha = 1f;

        bool begunLoading = false;
        //bool    begunStateCache = false;

        AsyncOperation sceneHandle;

        public void SetSplashScreens(float timer, AssetReferenceSprite[] splashes)
        {
            while (splashSetup.Count > 1)
                splashSetup.RemoveAt(0);

            foreach (var s in splashes)
            {
                splashSetup.Insert(0, new SplashPair { showTime = timer, splashSpriteRef = s });
            }
        }

        string GetVersionString()
        {
#if SURVEY_BUILD
                return Application.version + " survey";
#else
            return Application.version;
#endif
        }

        private void OnDisable()
        {
            foreach (var pair in splashSetup)
            {
                pair.splashSprite = null;
                Addressables.Release(pair.spriteHandle);
            }
        }

        private void Awake()
        {
            // start loading of addressables data
            //      -- HORoomAssetManager will handle itself
            //      -- need to preload meta stuff

            string dataPath = "";
#if PLATFORM_STANDALONE_OSX
            dataPath = Application.dataPath + "/../";
#endif

            Debug.Log(dataPath);

            if (System.IO.File.Exists(dataPath + "config"))
            {
                Debug.Log("Opening config file success");
                string[] data = System.IO.File.ReadAllLines(dataPath + "config");
                foreach (var l in data)
                {
                    Debug.Log(l);
                    SplashPair npair = new SplashPair();
                    if (splashSetup.Count > 0)
                        npair.showTime = splashSetup[0].showTime;
                    else
                        npair.showTime = 2f;

                    npair.splashSpriteRef = new AssetReferenceSprite(l);
                    splashSetup.Add(npair);
                }
            }

            //Add BZ Logo after vendor logo
            splashSetup.Add(defaultSplashPair);

            Debug.Log(System.IO.Directory.GetCurrentDirectory());

            //StateCache.instance.PreloadAll();

            foreach (var splashDef in splashSetup)
            {
                splashDef.spriteHandle = splashDef.splashSpriteRef.LoadAssetAsync();
                splashDef.spriteHandle.Completed += (_) => splashDef.splashSprite = splashDef.spriteHandle.Result;

            }

            StateCache.instance.PreloadAll();
        }

        private void Start()
        {
            //loadingText.text = LocalizationUtil.FindLocalizationEntry("Loading", string.Empty, false, TableCategory.UI);
            //loadingText.ForceMeshUpdate();
        }

        void SetSplash(int idx)
        {
            if (splashRenderer == null) return;

            currentSplashIndex = idx;
            currentSplashTimer = 0f;

            if (currentSplashIndex >= splashSetup.Count)
                return;

            //Rect splashSize = splashSetup[idx].splashSprite.rect;
            //float scaleH = splashSize.width / 3350f;
            //float scaleV = splashSize.height / 1536f;

            //float scale = 1f / Math.Max(scaleH, scaleV);

            //splashRenderer.transform.localScale = new Vector3(1f * scale, 1f * scale, 1f);
            splashRenderer.color = new Color(0f, 0f, 0f, 1f);
            splashRenderer.sprite = splashSetup[idx].splashSprite;

            splashCamera.orthographicSize = splashRenderer.bounds.size.y * 0.5f;
        }

        private void Update()
        {
            leadInDelay -= Time.deltaTime;

            if (leadInDelay > 0f)
                return;


            //if (!begunStateCache && (splashSetup.Length == 0 || splashSetup.All(x => x.splashSprite != null)))
            //{
            //    StateCache.instance.PreloadAll();
            //    begunStateCache = true;
            //}

            if (!Audio.instance.currentMusic == splashMusic)
                Audio.instance.PlayMusic(splashMusic);

            int sceneLoadPercent = sceneHandle == null ? 0 : (int)((sceneHandle.progress / .9f) * 100f);
            int totalLoadPercent = (sceneLoadPercent + (int)(StateCache.instance.LoadProgress() * 100f)) / 2;

            //string loadText = LocalizationUtil.FindLocalizationEntry("Loading", string.Empty, false, TableCategory.UI);
            //int dotCount = (int)(Time.time * 3f) % 4;

            //for (int i = 0; i < dotCount; i++)
            //{
            //    loadText += '.';
            //}

            //loadingText.text = loadText;

            // fade out load text when we're ready
            if (totalLoadPercent >= 100)
            {
                loadTextFadeAlpha -= Time.deltaTime;
                loadingText.color = new Color(1f, 1f, 1f, loadTextFadeAlpha);
            }

            if (currentSplashIndex == -1 && splashSetup.Count > 0 && splashSetup[0].splashSprite != null)
            {
                // waiting for first splash addressable to load
                SystemSaveContainer.instance.Vendor = splashSetup[0].splashSprite.name;
                SetSplash(0);
            }

            //Debug.Log($"{begunLoading} {StateCache.instance.LoadProgress() >= 1f}");

            // intentionally wait to start loading until we're done with state cache
            if (!begunLoading && StateCache.instance.LoadProgress() >= 1f)
            {
                begunLoading = true;

                // loading in Awake( ) causes allowSceneActivation to be ignored.
                sceneHandle = SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
                sceneHandle.allowSceneActivation = false;
                sceneHandle.completed += (AsyncOperation) => { Debug.Log("Game loaded"); };
            }

            // cycle splash screens
            if (currentSplashIndex < splashSetup.Count && currentSplashIndex >= 0)
            {
                if (splashSetup[currentSplashIndex].splashSprite == null) return;

                if (splashSetup.Count == currentSplashIndex + 1)
                {
                    // wait for load on the last splash screen
                    if ((sceneLoadPercent < 50f || StateCache.instance.LoadProgress() < 1f) && (currentSplashTimer >= (splashSetup[currentSplashIndex].showTime * 0.5f)))
                        return;
                }

                // if next splash is not ready, don't go to next screen until it is
                if (splashSetup.Count > currentSplashIndex + 1)
                {
                    if (splashSetup[currentSplashIndex + 1].splashSprite == null)
                    {
                        if (currentSplashTimer >= (splashSetup[currentSplashIndex].showTime * 0.5f))
                            return;
                    }
                }

                currentSplashTimer += Time.deltaTime;
                currentSplashTimer = Mathf.Clamp(currentSplashTimer, 0f, splashSetup[currentSplashIndex].showTime);

                float a = 1f;


                if (currentSplashTimer < 0.2f)
                    a = currentSplashTimer * 5f;
                else if (currentSplashTimer > (splashSetup[currentSplashIndex].showTime - 0.2f))
                    a = (splashSetup[currentSplashIndex].showTime - currentSplashTimer) * 5f;


                splashRenderer.color = new Color(a, a, a, 1f);



                if (currentSplashTimer >= splashSetup[currentSplashIndex].showTime)
                {
                    SetSplash(currentSplashIndex + 1);
                }
            }
            else if (sceneLoadPercent >= 100)
            {
                sceneHandle.allowSceneActivation = true;
            }
        }
    }
}
