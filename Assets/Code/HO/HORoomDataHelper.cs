using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ho
{
    public class HORoomDataHelper : SimpleSingleton <HORoomDataHelper>
    {
        Savegame.HORoomData CreateNewRoomData(string serializableGUID)
        {
            Savegame.HORoomData newRoomData = new Savegame.HORoomData()
            {
                assetGUID = serializableGUID
            };

            GameController.save.currentProfile.hoRoomDatas.Add(newRoomData);
            Savegame.SetDirty();
            return newRoomData;
        }

        Savegame.ModeData CreateNewModeData(string mode)
        {
            Savegame.ModeData newModeData = new Savegame.ModeData()
            {
                modeName = mode
            };
            return newModeData;
        }

        public Savegame.HORoomData GetHORoomData(string serializableGUID)
        {
            //Look if Room data exists
            if (GameController.save.currentProfile.hoRoomDatas.Any(x => x.assetGUID == serializableGUID))
            {
                return GameController.save.currentProfile.hoRoomDatas.First(x => x.assetGUID == serializableGUID);
            }

            return CreateNewRoomData(serializableGUID);
        }

        public List<Savegame.HORoomData> GetHORoomsUnlockedData(List<Chapter.Entry> roomReferences)
        {
            var roomsUnlocked = roomReferences.Where(x => GameController.save.IsChapterEntryUnlocked(x)).ToList();

            List<Savegame.HORoomData> roomDatas = new List<Savegame.HORoomData>();
            foreach(var room in roomsUnlocked)
            {
                roomDatas.Add(GetHORoomData(room.hoRoom.AssetGUID));
            }
            return roomDatas;
        }


        public Savegame.ModeData GetModeData(string serializableGUID, string mode)
        {
            Savegame.HORoomData roomData = GetHORoomData(serializableGUID);

            //Check if Mode Data exists
            if (roomData.modesData.Any(x => x.modeName == mode))
            {
                return roomData.modesData.First(x => x.modeName == mode);
            }

            //Create new data if mode data is not found
            Savegame.ModeData newModeData = CreateNewModeData(mode);
            roomData.modesData.Add(newModeData);

            return newModeData;
        }

        public void InsertModeData(string serializableGUID, string mode, int highScore, int clearTime)
        {
            Savegame.ModeData modeData = GetModeData(serializableGUID, mode);

            modeData.playedGames++;

            if (highScore > modeData.highScore)
                modeData.highScore = highScore;

            if (clearTime < modeData.fastestGameClear)
            {
                modeData.fastestGameClear = clearTime;
            }
        }

        public void InsertTriviaFound(string serializableGUID, string triviaKey)
        {
            Savegame.HORoomData roomData = GetHORoomData(serializableGUID);

            if(roomData.triviasFound.Contains(triviaKey))
            {
                Debug.Log($"Room data already has this trivia key {triviaKey}");
            }
            else
            {
                roomData.triviasFound.Add(triviaKey);
            }
        }

#if UNITY_EDITOR

        public HORoomReference testReference;
        [Sirenix.OdinInspector.Button]
        public void ShowRoomData()
        {
            var roomData = GetHORoomData(testReference.AssetGUID);
            
            foreach(var mode in roomData.modesData)
            {
                Debug.Log($"Mode Name: {mode.modeName} Played Games: {mode.playedGames}");
            }

            foreach (var trivia in roomData.triviasFound)
            {
                Debug.Log($"Trivia Name: {trivia} Played Games: {trivia}");
            }
        }
#endif
    }
}

