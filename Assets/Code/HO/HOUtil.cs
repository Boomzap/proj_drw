using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Tables;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Localization;
#endif

using System.Globalization;
using UnityEngine.Localization.Settings;

namespace ho
{
    public static class HOUtil
    {
        public static Dictionary<string, System.Type> objectTypeMap = new Dictionary<string, System.Type>
        {
            ["o"] = typeof(HOFindableObject),
            ["k"] = typeof(HOKeyItem),
            ["d"] = typeof(HODoorItem),
            ["x"] = typeof(HOFindableObject),
            ["p"] = typeof(HOFindableObject),
            ["t"] = typeof(HOTriviaObject),
        };

        public static void SetupObjectDefaultsFromName(GameObject obj, string roomName)
        {
            HOInteractiveObject[] interactive = obj.GetComponents<HOInteractiveObject>();
            if (interactive != null)
            {
                foreach (HOInteractiveObject o in interactive)
                    GameObject.DestroyImmediate(o, true);
            }

            Collider2D[] colliders = obj.GetComponents<Collider2D>();
		    if (colliders != null) 
            {
                foreach (Collider2D o in colliders) 
                    GameObject.DestroyImmediate(o, true);
            }

            // if we match any prefixes for our object types, default initialize them
            string nameLower = obj.name.ToLower();

            var split = nameLower.Split('_');
            if (split != null && split.Length > 0)
            {
                var objectTypeKey = split[0];

                if (!objectTypeMap.ContainsKey(objectTypeKey))
                    return;

                SetupObjectDefaults(obj, objectTypeMap[objectTypeKey], roomName);
            }
        }

        internal static HORoom FindRoomParent(GameObject obj)
        {
            GameObject f = obj;

            while (f.transform.parent != null && f.GetComponent<HORoom>() == null)
            {
                f = f.transform.parent.gameObject;
            }

            return f.GetComponent<HORoom>();
        }

        internal static void SetupDoorItem(HODoorItem doorItem)
        {
            var split = doorItem.name.ToLower().Split('.');
            string baseName = split[0];
            string stateName = split[1];

            HORoom room = FindRoomParent(doorItem.gameObject);

            HODoorHandler handler = room.GetHODoorHandler(baseName);

            if (handler == null)
            {
                handler = new HODoorHandler();

                handler.baseName = baseName;
                room.doorHandlers.Add(handler);
            }

            if (stateName.Equals("closed"))
                handler.closedState = doorItem.gameObject;
            else
            {
                handler.openState = doorItem.gameObject;
            }


            doorItem.doorName = baseName;

            #if UNITY_EDITOR
            doorItem.mouseoverMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/brighten.mat");
            #endif
        }

        public static void SetupObjectDefaults(GameObject obj, System.Type objType, string roomName)
        {
            // remove any interactiveobject instances
            HOInteractiveObject[] interactive = obj.GetComponents<HOInteractiveObject>();
            if (interactive != null)
            {
                foreach (HOInteractiveObject o in interactive)
                    GameObject.DestroyImmediate(o);
            }

            Collider2D[] colliders = obj.GetComponents<Collider2D>();
		    if (colliders != null) 
            {
                foreach (Collider2D o in colliders) 
                    GameObject.DestroyImmediate(o);
            }

            var newComponent = obj.AddComponent(objType) as HOInteractiveObject;
            newComponent.InitializeDefaults(roomName);

            if (objType == typeof(HODoorItem))
            {
                SetupDoorItem(newComponent as HODoorItem);
            }
        }

        // don't bother in non-editor builds
        public static string GetOrAddDefaultTermIfNeeded(string key, string value = "", bool returnKey = false, bool allowMismatchCategory = true)
        {
            return LocalizationUtil.FindLocalizationEntry(key, value, returnKey); ;
        }

        public static string ToTitleCase(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }

        public static string GetRoomObjectLocalizedName(string roomName, string objectName, bool returnKey = false)
        {
            string nameLower = roomName.ToLower();
            string objNameLower = objectName.ToLower().TrimEnd(' ');

            var locTerm = string.Format("{0}/{1}", nameLower, objNameLower);

            //Get English Loc for Objects
            string[] objLoc = objNameLower.Split('_');
            string locValue = objLoc.Length > 1 ? ToTitleCase(objLoc[1]) : string.Empty;

#if UNITY_EDITOR
            bool isEntryFoundInBank = LocalizationUtil.FindEntryInLocalizationBank(locTerm, objNameLower, TableCategory.Game);

            Debug.Log($"Entry found in bank? {isEntryFoundInBank} Return Key? {returnKey}");
            if (isEntryFoundInBank)
            {
                return GetOrAddDefaultTermIfNeeded(locTerm, string.Empty, returnKey);
            }
#endif
            return GetOrAddDefaultTermIfNeeded(locTerm, locValue, returnKey);
        }

        public static string GetRoomObjectFindXTerm(string roomName, string objectName)
        {
            string nameLower = roomName.ToLower();
            string objNameLower = objectName.ToLower().TrimEnd(' ');
            
            var locTerm = string.Format("{0}/{1}_findall", nameLower, objNameLower);

            return GetOrAddDefaultTermIfNeeded(locTerm);
        }

        public static string[] GetRoomObjectRiddle(string roomName, string objectName)
        {
            string nameLower = roomName.ToLower();
            string objNameLower = objectName.ToLower().TrimEnd(' ');
            
            var locTerm = string.Format("{0}/{1}_riddle", nameLower, objNameLower);
            GetOrAddDefaultTermIfNeeded(locTerm);
            var locTerm2 = string.Format("{0}/{1}_riddle2", nameLower, objNameLower);
            GetOrAddDefaultTermIfNeeded(locTerm2);
            var locTerm3 = string.Format("{0}/{1}_riddle3", nameLower, objNameLower);
            GetOrAddDefaultTermIfNeeded(locTerm3);

            return new string[] { locTerm, locTerm2, locTerm3 };
        }

        public static string GetRoomObjectPluralization(string roomName, string objectName, int count, bool returnKey = false)
        {
            if (count <= 1) return GetRoomObjectLocalizedName(roomName, objectName, true);
        
            string nameLower = roomName.ToLower();
            string objNameLower = objectName.ToLower().TrimEnd(' ');
            
            var locTerm = string.Format("{0}/{1}_x{2}", nameLower, objNameLower, count);

            return GetOrAddDefaultTermIfNeeded(locTerm, string.Empty, returnKey);            
        }

        public static string GetRoomLocalizedName(string roomName)
        {
            string nameLower = roomName.ToLower();
            var locTerm = string.Format("{0}/scene_name", nameLower);

            return GetOrAddDefaultTermIfNeeded(locTerm, string.Empty, true);
        }

        public static string LocStringWithParameters(string str, params string[] keyValuePairs)
        {
            string v = str;
            for (int i = 0; i+1 < keyValuePairs.Length; i+=2)
            {
                v = v.Replace("{[" + keyValuePairs[0+i] + "]}", keyValuePairs[1+i]);
            }

            return v;
        }

        public static string LocString(string str)
        {
            return str;
        }

        public static double UnixTime()
        {
            System.DateTime epoch = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);

            return (System.DateTime.UtcNow - epoch).TotalSeconds;
        }

        public static string FormatDurationHHMMSS(double duration)
        {
            int hours = Mathf.FloorToInt((float)duration / (60 * 60));
            duration -= (hours * 60 * 60);
            int minutes = Mathf.FloorToInt((float)duration / 60);
            duration -= (minutes * 60);
            int seconds = (int)duration;

            return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        }

        public static string FormatDurationMMSS(double duration)
        {
            int minutes = Mathf.FloorToInt((float)duration / 60);
            duration -= (minutes * 60);
            int seconds = (int)duration;

            return $"{minutes:D2}:{seconds:D2}";
        }

        public static Vector3 UIWidgetCenterPosToWorldPos(RectTransform rectTransform)
        {
            Vector2 s = Vector2.Scale(rectTransform.rect.size, rectTransform.lossyScale);
            Rect p = new Rect((Vector2)rectTransform.position - (s * 0.5f), s);

            return GameController.instance.currentCamera.ScreenToWorldPoint(p.center);
        }

        public static Vector3 UIWidgetCenterPosToScreenPos(RectTransform rectTransform)
        {
            return rectTransform.position;
        }

 
        public static Vector2 WorldPosToCanvasPos(Vector3 worldPos, Canvas uiCanvas)
        {
            var screenPos = GameController.instance.currentCamera.WorldToScreenPoint(worldPos);
            Vector2 canvasPos;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(uiCanvas.transform as RectTransform, screenPos,
                uiCanvas.worldCamera, out canvasPos);

            return canvasPos;
        }

        public static string GetCultureNumberSeparator(int number)
        {
            var localeId = LocalizationSettings.SelectedLocale.Identifier;
            NumberFormatInfo nfi = new CultureInfo(localeId.Code, false).NumberFormat;

            //Debug.Log(number.ToString("N0" , nfi));
            return number.ToString("N0", nfi);
        }
    }
}