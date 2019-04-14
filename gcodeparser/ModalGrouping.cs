/*
 * Создано в SharpDevelop.
 * Пользователь: P.Dubovikov
 * Дата: 30.09.2016
 * Время: 15:21
 * 
 * Для изменения этого шаблона используйте меню "Инструменты | Параметры | Кодирование | Стандартные заголовки".
 */
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;


namespace gcodeparser.gcodes
{
	/// <summary>
	/// Description of ModalGrouping.
	/// </summary>
public class ModalGrouping {
		

    public static ReadOnlyDictionary<GCodeGroups, ISet<string>> groupToModals { get; set; }
    public static ReadOnlyDictionary<String, GCodeGroups> modalToGroup { get; set; }
    
    public static Dictionary<string, Double> myDict { get; set; } = new Dictionary<string, Double>();
   
	
    static ModalGrouping() {
    	
        Dictionary<GCodeGroups, ISet<string>> gGroups = new Dictionary<GCodeGroups, ISet<string>>();
        Dictionary<String, GCodeGroups> mGroups = new Dictionary<String, GCodeGroups>();

        // Add all model sets
      	
        gGroups.Add(GCodeGroups.ActivePlane, toStringSet(typeof(ActivePlane.ActPlane).GetEnumNames())) ;
        gGroups.Add(GCodeGroups.AxisOffset, toStringSet(typeof(AxisOffset.AxisOffsets).GetEnumNames())) ;
        gGroups.Add(GCodeGroups.CollantMode, toStringSet(typeof(CollantMode.CollantModes).GetEnumNames()));
        gGroups.Add(GCodeGroups.CoordinateSystemMode, toStringSet(typeof(CoordinateSystemMode.CoordinateSystemModes).GetEnumNames()));
        gGroups.Add(GCodeGroups.CutterLengthCompMode, toStringSet(typeof(CutterLengthCompMode.CutterLengthCompModes).GetEnumNames()));
        gGroups.Add(GCodeGroups.CutterRadiusCompMode, toStringSet(typeof(CutterRadiusCompMode.CutterRadiusCompModes).GetEnumNames()));
        gGroups.Add(GCodeGroups.DistanceMode, toStringSet(typeof(DistanceMode.DistanceModes).GetEnumNames()));
        gGroups.Add(GCodeGroups.FeedRateMode, toStringSet(typeof(FeedRateMode.FeedRateModes).GetEnumNames()));
        gGroups.Add(GCodeGroups.MotionsModes,  toStringSet(typeof(MotionMode.MotionModes).GetEnumNames()));
        gGroups.Add(GCodeGroups.PathControleMode, toStringSet(typeof(PathControleMode.PathControleModes).GetEnumNames()));
        gGroups.Add(GCodeGroups.PredefinedPosition,  toStringSet(typeof(PredefinedPosition.PredefinedPositions).GetEnumNames()));
        gGroups.Add(GCodeGroups.ReferenceLocation, toStringSet(typeof(ReferenceLocation.ReferenceLocations).GetEnumNames()));
        gGroups.Add(GCodeGroups.RetrackMode, toStringSet(typeof(RetrackMode.RetrackModes).GetEnumNames()));
        gGroups.Add(GCodeGroups.SFOverrideMode, toStringSet(typeof(SFOverrideMode.SFOverrideModes).GetEnumNames()));
        gGroups.Add(GCodeGroups.SpindleMode, toStringSet(typeof(SpindleMode.SpindleModes).GetEnumNames()));
        gGroups.Add(GCodeGroups.StopMode, toStringSet(typeof(StopMode.StopModes).GetEnumNames()));
        gGroups.Add(GCodeGroups.Units, toStringSet(typeof(Units).GetEnumNames()));

        // Create a modal to group map
        foreach (KeyValuePair<GCodeGroups, ISet<string>> group in gGroups)
        {
        	foreach (String modals in group.Value)
            {
				mGroups[modals] = group.Key;
				
            }
        }
        
//        foreach (KeyValuePair<String, GCodeGroups> pair in mGroups)
//        {
//        	Console.WriteLine("Key: " + pair.Key + " " + "Value: " + pair.Value) ;
//        }

        //  
        groupToModals = new ReadOnlyDictionary<GCodeGroups, ISet<string>>(gGroups) ;
        modalToGroup = new ReadOnlyDictionary<String, GCodeGroups>(mGroups);
        
    }


    /**
     * Return the group belonging to a modal word
     * @param word
     * @return
     */
    public static GCodeGroups whatGroup(string word)
    {
    	if (modalToGroup.ContainsKey(word))
    	{
    		return modalToGroup[word] ;
    	}

    	return GCodeGroups.Default ;
    	
    }
	

    private static ISet<String> toStringSet<T> (T[] enumClass)
    {
        ISet<string> stringSet = new HashSet<string>();
        foreach (T code in enumClass) 
        {
            stringSet.Add(code.ToString());
        }
        return stringSet;
    }

}
}
