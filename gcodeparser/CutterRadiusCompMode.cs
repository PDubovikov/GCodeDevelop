/*
 * Создано в SharpDevelop.
 * Пользователь: P.Dubovikov
 * Дата: 06.10.2016
 * Время: 10:50
 * 
 * Для изменения этого шаблона используйте меню "Инструменты | Параметры | Кодирование | Стандартные заголовки".
 */
using System.Collections.Generic ;

namespace gcodeparser.gcodes
{

	public class CutterRadiusCompMode
	{


		public enum CutterRadiusCompModes
		{
			G40 = GCodeGroups.CutterRadiusCompMode,
			G41 = GCodeGroups.CutterRadiusCompMode,
			G41_1 = GCodeGroups.CutterRadiusCompMode,
			G42 = GCodeGroups.CutterRadiusCompMode,
			G42_1 = GCodeGroups.CutterRadiusCompMode
		}

		GCodeGroups group;

		CutterRadiusCompMode(GCodeGroups group)
		{
			this.group = group;

		}

	}

}
