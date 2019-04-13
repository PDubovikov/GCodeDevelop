/*
 * Создано в SharpDevelop.
 * Пользователь: P.Dubovikov
 * Дата: 06.10.2016
 * Время: 11:07
 * 
 * Для изменения этого шаблона используйте меню "Инструменты | Параметры | Кодирование | Стандартные заголовки".
 */
using System.Collections.Generic;

namespace gcodeparser.gcodes
{

	public class DistanceMode
	{

		public enum DistanceModes
		{
			G90 = GCodeGroups.DistanceMode,
			G91 = GCodeGroups.DistanceMode
		}


		GCodeGroups group;

		DistanceMode(GCodeGroups group)
		{
			this.group = group;

		}

	}
}