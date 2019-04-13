/*
 * Создано в SharpDevelop.
 * Пользователь: P.Dubovikov
 * Дата: 06.10.2016
 * Время: 10:45
 * 
 * Для изменения этого шаблона используйте меню "Инструменты | Параметры | Кодирование | Стандартные заголовки".
 */
using System.Collections.Generic;


namespace gcodeparser.gcodes
{

	public class CutterLengthCompMode
	{

		public enum CutterLengthCompModes
		{
			G43 = GCodeGroups.CutterLengthCompMode,
			G43_1 = GCodeGroups.CutterLengthCompMode,
			G49 = GCodeGroups.CutterLengthCompMode
		}


		GCodeGroups group;

		CutterLengthCompMode(GCodeGroups group)
		{
			this.group = group;
			
		}

	}

}