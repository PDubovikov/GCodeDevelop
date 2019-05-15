/*
 * Создано в SharpDevelop.
 * Пользователь: P.Dubovikov
 * Дата: 06.10.2016
 * Время: 11:27
 * 
 * Для изменения этого шаблона используйте меню "Инструменты | Параметры | Кодирование | Стандартные заголовки".
 */
using System ;
using System.Collections.Generic;


namespace gcodeparser.gcodes
{


	public class MotionMode : ModalGroups
	{

		public enum MotionModes
		{
			G0 = GCodeGroups.MotionsModes,
			G1 = GCodeGroups.MotionsModes,
			G2 = GCodeGroups.MotionsModes,
			G3 = GCodeGroups.MotionsModes,
			G33 = GCodeGroups.MotionsModes,
			G33_1 = GCodeGroups.MotionsModes,
	//   	 G53(GCodeGroups.MotionsModes),    // Machine coordinate moves, this is more like a modifier...
			G73 = GCodeGroups.MotionsModes,
			G76 = GCodeGroups.MotionsModes,
			G80 = GCodeGroups.MotionsModes, // cancel canned cycle modal motion.
			G81 = GCodeGroups.MotionsModes,
			G82 = GCodeGroups.MotionsModes,
			G83 = GCodeGroups.MotionsModes,
			G84 = GCodeGroups.MotionsModes,
			G85 = GCodeGroups.MotionsModes,
			G86 = GCodeGroups.MotionsModes,
			G87 = GCodeGroups.MotionsModes,
			G88 = GCodeGroups.MotionsModes,
			G89 = GCodeGroups.MotionsModes
		}
	
	    	GCodeGroups group ;

    		public MotionMode(GCodeGroups group)
    		{
    	  		this.group = group ;
    		}

    		public Boolean isInGroup(GCodeGroups group)
    		{
    			return this.group == group;
    		}
	
	}
}