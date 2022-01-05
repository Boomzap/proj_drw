using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary; 
using System;
using System.IO;
using UnityEngine.Video;

//  SystemSave is for data that is needed to be loaded on game load - 
//      things that don't apply to gameplay so much as config settings.

namespace ho
{
    [Serializable] 
    public class SystemSave
    {
	    // system values
	    public bool				isDirty = false;

	    public float			musicVolume = 0.3f;
	    public float			audioVolume = 0.5f;
	    public float			ambientVolume = 0.25f;
	    public float			backgroundAlpha = 200.0f/255.0f;
	    public bool				scrollText = true;
	    public Int64			userID;
	    public float			autoSkipInterval = 0.5f;
	    public bool				fullscreen = true;
		public bool				skipDialog = false;
	    public bool				increaseTextSize = false;
		public int				languageIndex = 0;
	
	    // disclaimer
	    public bool				disclaimer = false;

	    public SystemSave(Int64 userID = 0) 
	    {

	    }

	    public void TriggerSave()
	    {
		    Save(GameController.systemSave, Savegame.GetPath("system.sav"));
	    }

	    public void ExitSave()
	    {
		    if (isDirty)
		    {
			    TriggerSave();
			    isDirty = false;
		    }
	    }

	    public static void Save(SystemSave save, string filename)
	    {
		    try
		    {
			    BinaryFormatter bf = new BinaryFormatter();
			    FileStream file = File.Create(filename);
			    bf.Serialize(file, save);
			    file.Close();	
		    } catch (Exception e)
		    {
			    Debug.Log("Exception " + e.ToString()+ " saving systemsave " + filename );
		    }
		    save.isDirty = false;
	    }

	    public static SystemSave Load(string filename)
	    {
		    Int64 randomID = (Int64)(UnityEngine.Random.value * 9223372036854775806);
		    if (!File.Exists(filename)) 
		    {
			    SystemSave save = new SystemSave(randomID);		// default
			    return save;
		    }
		    try
		    {
			    BinaryFormatter bf = new BinaryFormatter();
			    FileStream fileStream = File.Open(filename, FileMode.Open);
			    SystemSave  newSave = (SystemSave)bf.Deserialize(fileStream);
			    fileStream.Close();
			    newSave.autoSkipInterval = 0.1f;

			    if (newSave.backgroundAlpha < 0.1f) newSave.backgroundAlpha = 1f;

			    return newSave;
		    } catch (Exception e)
		    {
			    Debug.Log("Exception " + e.ToString()+ " loading system save. Resetting");
			    SystemSave save = new SystemSave(randomID);		// default
			    return save;
		    }
	    }
    }
}