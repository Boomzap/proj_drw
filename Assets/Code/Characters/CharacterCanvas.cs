using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Boomzap.Character
{
    public class LayerAttribute : PropertyAttribute
    {

    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(LayerAttribute))]
    internal class LayerAttributeEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.intValue = EditorGUI.LayerField(position, label, property.intValue);
        }
    }

#endif
    // manages a set of characters that progressively change between preset positions,
    // and renders them to a render texture that can be used by other tools.
    public class CharacterCanvas : MonoBehaviour
    {
        [SerializeField] Camera renderCamera;
        [Layer, SerializeField] int renderLayer;
        [SerializeField] float characterScale = 100f;
        [SerializeField] float fadeTime = 0.2f;
        public int SlotCount => characters.Length;
        public Character[] Characters => characters;

        RenderTexture   texture;
        
        Character[]     characters = new Character[0];
        Character[]     prevCharacters = new Character[0];

        Vector3[]       slotPositions;

        internal class StateChange
        {
            public Character target = null;

            public float alphaFrom = 1f, alphaTo = 1f;
            public Vector3 posFrom = Vector3.zero, posTo = Vector3.zero;

            public float timeStart = 0f;
            public float timeEnd = 0f;
        }

        List<StateChange>           activeChanges = new List<StateChange>();

        List<Character>             loadedCharacters = new List<Character>();
        Dictionary<CharacterReference, AsyncOperationHandle> loadingCharacters = new Dictionary<CharacterReference, AsyncOperationHandle>();

        int                         waitingLoadCount = 0;

        public bool                 IsWaitingOnLoad => waitingLoadCount > 0;
        public Texture              OutputTexture { get => texture; }
        public bool                 IsAnimating => activeChanges.Count > 0;

        [SerializeField]
        bool                        renderToTexture = false;

        void OnAssetInstantiated(AsyncOperationHandle handle, CharacterInfo info, UnityAction<Character> andThen)
        {
            Character character = loadedCharacters.FirstOrDefault(x => x.name == info.name);

            waitingLoadCount--;

            if (loadingCharacters.ContainsKey(info.characterRef))
            {
                loadingCharacters.Remove(info.characterRef);
            }

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                // prevent double requests if it was already asked to load
                if (character == null)
                { 
                    character = (handle.Result as GameObject).GetComponent<Character>();
                    if (character == null)
                    {
                        Debug.LogError("Async loading character failed, the result doesn't have a character component");
                        return;
                    }

                    character.name = info.name;
                    loadedCharacters.Add(character);

                    foreach (var t in character.GetComponentsInChildren<Transform>())
                        t.gameObject.layer = renderLayer;
                    gameObject.layer = renderLayer;

                    character.gameObject.SetActive(false);
                    character.SetScale(characterScale);
                } else
                {
                    
                }

                andThen?.Invoke(character);

                character.ForceAlpha(0f);
            }
            else
            {
                Debug.LogError($"Failed to load a character {info.name}");
            }
        }

        void FetchCharacterAsync(CharacterInfo info, UnityAction<Character> andThen)
        {
            Character alreadyLoaded = loadedCharacters.FirstOrDefault(x => x.name == info.name);

            if (alreadyLoaded == null)
            {
                AsyncOperationHandle loadingHandle;
                if (loadingCharacters.TryGetValue(info.characterRef, out loadingHandle))
                {
                    // already loading.. what do we do? this means 2x in one frame the asset was requested, likely with different andThens

                    // ignore
                    return;
                }

                waitingLoadCount++;

                var handle = info.characterRef.InstantiateAsync(transform, true);
                // completed synchronously, i was already loaded
                if (handle.IsDone)
                {
                    OnAssetInstantiated(handle, info, andThen);
                } else
                {
                    handle.Completed += (AsyncOperationHandle<GameObject> handle2) => OnAssetInstantiated(handle2, info, andThen);
                    loadingCharacters[info.characterRef] = handle;
                }
            } else
            {
                andThen?.Invoke(alreadyLoaded);

            }
        }


        public void FreeCharacters()
        {
            foreach (var character in loadedCharacters)
            {
                Addressables.ReleaseInstance(character.gameObject);
            }

            loadedCharacters.Clear();
        }

        public void SetLayout(Vector3[] slotBaseWorldPos, Vector3 cameraWorldPos)
        {
            if (slotBaseWorldPos.Length != characters.Length)
            {
                ResetCharacters();
                characters = new Character[slotBaseWorldPos.Length];
                prevCharacters = new Character[slotBaseWorldPos.Length];
            }

            cameraWorldPos.z -= 10f;
            renderCamera.transform.position = cameraWorldPos;

            slotPositions = slotBaseWorldPos;

            for (int i = 0; i < characters.Length; i++)
            {
                if (characters[i])
                    characters[i].transform.position = GetSlotPos(i);
                if (prevCharacters[i])
                    prevCharacters[i].transform.position = GetSlotPos(i);
            }
        }


        public void ResetCharacters()
        {
            for (int i = 0; i < characters.Length; i++)
            {
                characters[i] = prevCharacters[i] = null;
            }

            activeChanges.Clear();
        }

        private void OnEnable()
        {
            ResetCharacters();
            FreeCharacters();
        }

        private void OnDisable()
        {
            ResetCharacters();
            FreeCharacters();
        }

        public void FinishAllStateChanges()
        {
            foreach (var stateChange in activeChanges)
            {
                if (stateChange.target == null) continue;

                stateChange.target.ForceAlpha(stateChange.alphaTo);
                stateChange.target.transform.position = stateChange.posTo;

                if (stateChange.alphaTo <= 0f)
                {
                    stateChange.target.gameObject.SetActive(false);
                }
            }

            activeChanges.Clear();
        }

        StateChange GetStateChange(Character forChar)
        {
            StateChange active = activeChanges.FirstOrDefault(x => x.target == forChar);

            if (active != null) return active;

            active = new StateChange();
            active.target = forChar;
            active.alphaFrom = active.alphaTo = forChar.CurrentAlpha;
            active.posFrom = active.posTo = forChar.transform.position;
            active.timeStart = active.timeEnd = Time.time;

            activeChanges.Add(active);
            return active;
        }

        private void Update()
        {
            foreach (var stateChange in activeChanges)
            {
                if (stateChange.target == null) continue;

                float a = (Time.time - stateChange.timeStart) / (stateChange.timeEnd - stateChange.timeStart);
                float posA = a * 0.5f;
                a = Mathf.Clamp(a, 0f, 1f);

                stateChange.target.ForceAlpha(Mathf.Lerp(stateChange.alphaFrom, stateChange.alphaTo, a));
                
                posA = 1f - Mathf.Pow(1f - posA, 4);

                stateChange.target.transform.position = Vector3.Lerp(stateChange.posFrom, stateChange.posTo, posA);
                

                stateChange.target.gameObject.SetActive(stateChange.target.CurrentAlpha > 0f);
            }

            activeChanges.RemoveAll(x => x.timeEnd <= Time.time);

            UpdateRenderTexture();
        }

        private void UpdateRenderTexture()
        {
            if (!renderToTexture)
            {
                if (texture != null)
                    Destroy(texture);

                texture = null;

                renderCamera.targetTexture = null;

                return;
            }

            if (renderCamera == null) return;

            if (texture == null || texture.width != Screen.width || texture.height != Screen.height)
            {
                Destroy(texture);
                texture = new RenderTexture(Screen.width, Screen.height, 32);
            }

            renderCamera.targetTexture = texture;
            renderCamera.Render();
            renderCamera.targetTexture = null;
        }

        public void StartChanges()
        {
            FinishAllStateChanges();

            for (int i = 0; i < characters.Length; i++)
                prevCharacters[i] = characters[i];
        }

        public void UpdateSlot(int slot, CharacterInfo characterInfo, string state, string emotion, string eyes, List<CharacterSpineSlot.SlotToggle> slotToggles, bool flip, bool lookBack)
        {
            if (characterInfo == null)
            {
                characters[slot] = null;
                return;
            }

            FetchCharacterAsync(characterInfo, (Character c) =>
            {
                c.SetState(state);
                c.SetEmotion(emotion, eyes);
                c.SetLookBack(lookBack);
                c.Flip(flip);
                //c.SetSpineSlots(slotToggles);

                characters[slot] = c;
            });
        }

        public void MoveAwayCharacterSlots()
        {
           for(int i = 0; i < characters.Length; i++)
            {
                if (characters[i] == null) continue;

                if(i < 3)
                    characters[i].gameObject.LeanMoveLocalX(characters[i].transform.position.x - 2000f, 0.25f);
                else
                    characters[i].gameObject.LeanMoveLocalX(characters[i].transform.position.x + 2000f, 0.25f);
            }
        }

        Vector3 GetSlotPos(int idx)
        {
            Vector3 tPoint = renderCamera.ScreenToWorldPoint(slotPositions[idx]);
            tPoint.z = slotPositions[idx].z;

            return tPoint;
        }

        Vector3 GetSlotPosOffscreen(int idx)
        {
            Vector3 tPoint = renderCamera.ScreenToWorldPoint(slotPositions[idx]);
            tPoint.z = slotPositions[idx].z;

            if (characters[idx].IsFlipped)
                tPoint.x -= 3350f * 0.5f;
            else
                tPoint.x += 3350f * 0.5f;

            return tPoint;
        }

        public void AdjustCharacterToLayerForSlot(int slot, Character c, bool speaking = false)
        {
            int slotBase = slot * 10;

            Renderer[] renderers = c.gameObject.GetComponentsInChildren<Renderer>(true);

            foreach (var r in renderers)
            {
                r.sortingOrder = speaking? r.sortingOrder + 50 : r.sortingOrder % 10;
                r.sortingOrder += slotBase;
            }
        }

        IEnumerator WaitThenUpdateCor()
        {
            while (waitingLoadCount > 0)
                yield return null;

            // characters that will be removed, fade out
            IEnumerable<Character> toDie = prevCharacters.Where(x => x != null && !characters.Contains(x));
            // characters to fade in
            IEnumerable<Character> toIntro = characters.Where(x => x != null && !prevCharacters.Contains(x));
            // characters to move
            IEnumerable<Character> toMove = prevCharacters.Where(x => x != null && characters.Contains(x) && Array.IndexOf(characters, x) != Array.IndexOf(prevCharacters, x));

            foreach (var c in toDie)
            {
                var s = GetStateChange(c);
                s.timeStart = Time.time;
                s.timeEnd = fadeTime + s.timeStart;
                s.alphaTo = 0f;
                s.alphaFrom = c.CurrentAlpha;
            }

            foreach (var c in toIntro)
            {
                int cidx = Array.IndexOf(characters, c);
                var s = GetStateChange(c);
                s.timeStart = Time.time;
                s.timeEnd = fadeTime + s.timeStart;
                s.alphaTo = 1f;
                s.alphaFrom = 0f;
                
                if (prevCharacters.Count(x => x != null) > 0)
                {
                    c.transform.position = GetSlotPos(cidx);
                    s.posTo = c.transform.position;
                    s.posFrom = c.transform.position;
                } else
                {   
                    s.posTo = GetSlotPos(cidx); 
                    s.posFrom = GetSlotPosOffscreen(cidx);
                }

                //AdjustCharacterToLayerForSlot(cidx, c);
                c.ForceAlpha(s.alphaFrom);
            }

            foreach (var c in toMove)
            {
                int cidx = Array.IndexOf(characters, c);
                var s = GetStateChange(c);
                s.timeStart = Time.time;
                s.timeEnd = fadeTime + s.timeStart;
                s.posTo = GetSlotPos(cidx);
                s.posFrom = c.transform.position;
                //c.transform.position = GetSlotPos(Array.IndexOf(characters, c));

                //AdjustCharacterToLayerForSlot(cidx, c);
            }
        }

        public void ChangesFinished()
        {
            StopAllCoroutines();

            StartCoroutine(WaitThenUpdateCor());
        }
    }
}