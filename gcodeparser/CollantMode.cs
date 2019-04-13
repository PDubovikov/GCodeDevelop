/*
 * Создано в SharpDevelop.
 * Пользователь: P.Dubovikov
 * Дата: 06.10.2016
 * Время: 10:34
 * 
 * Для изменения этого шаблона используйте меню "Инструменты | Параметры | Кодирование | Стандартные заголовки".
 */
using System.Collections.Generic;
 

namespace gcodeparser.gcodes
{

	public class CollantMode
	{

		public enum CollantModes
		{
			M7 = GCodeGroups.CollantMode,
			M8 = GCodeGroups.CollantMode,
			M9 = GCodeGroups.CollantMode,
			M78 = GCodeGroups.CollantMode
		}

		 GCodeGroups group;

		CollantMode(GCodeGroups group)
		{
			this.group = group;

		}

	}

}