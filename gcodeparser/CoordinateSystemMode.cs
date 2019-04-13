/*
 * Создано в SharpDevelop.
 * Пользователь: P.Dubovikov
 * Дата: 06.10.2016
 * Время: 10:42
 * 
 * Для изменения этого шаблона используйте меню "Инструменты | Параметры | Кодирование | Стандартные заголовки".
 */
using System ;
using System.Collections.Generic;
 
namespace gcodeparser.gcodes
{

	public class CoordinateSystemMode : ModalGroups
	{

		public enum CoordinateSystemModes
		{
			G54 = GCodeGroups.CoordinateSystemMode,
			G55 = GCodeGroups.CoordinateSystemMode,
			G56 = GCodeGroups.CoordinateSystemMode,
			G57 = GCodeGroups.CoordinateSystemMode,
			G58 = GCodeGroups.CoordinateSystemMode,
			G59 = GCodeGroups.CoordinateSystemMode,
			G59_1 = GCodeGroups.CoordinateSystemMode,
			G59_2 = GCodeGroups.CoordinateSystemMode,
			G59_3 = GCodeGroups.CoordinateSystemMode,
			G505 = GCodeGroups.CoordinateSystemMode,
			G506 = GCodeGroups.CoordinateSystemMode,
			G507 = GCodeGroups.CoordinateSystemMode,
			G508 = GCodeGroups.CoordinateSystemMode,
			G509 = GCodeGroups.CoordinateSystemMode,
			G510 = GCodeGroups.CoordinateSystemMode,
			G511 = GCodeGroups.CoordinateSystemMode,
			G512 = GCodeGroups.CoordinateSystemMode,
			G513 = GCodeGroups.CoordinateSystemMode,
			G514 = GCodeGroups.CoordinateSystemMode,
			G515 = GCodeGroups.CoordinateSystemMode,
			G516 = GCodeGroups.CoordinateSystemMode,
			G517 = GCodeGroups.CoordinateSystemMode,
			G518 = GCodeGroups.CoordinateSystemMode,
			G519 = GCodeGroups.CoordinateSystemMode,
			G520 = GCodeGroups.CoordinateSystemMode,
			G500 = GCodeGroups.CoordinateSystemMode,
			G53 = GCodeGroups.CoordinateSystemMode
		}
		

		GCodeGroups group;

		CoordinateSystemMode(GCodeGroups group)
		{
			this.group = group;
			
		}
		
		public Boolean isInGroup(GCodeGroups group)
    	{
    		return this.group == group;
    	}
		
	}
}
