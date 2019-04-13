/*
 * Создано в SharpDevelop.
 * Пользователь: P.Dubovikov
 * Дата: 06.10.2016
 * Время: 13:41
 * 
 * Для изменения этого шаблона используйте меню "Инструменты | Параметры | Кодирование | Стандартные заголовки".
 */

using System;
using System.Collections.Generic;

namespace gcodeparser.gcodes
{

	public class ReferenceLocation
	{

		public enum ReferenceLocations
		{
			G28 = GCodeGroups.ReferenceLocation,
			M18 = GCodeGroups.ReferenceLocation,
			G28_1 = GCodeGroups.ReferenceLocation
		}


		GCodeGroups group;

		ReferenceLocation(GCodeGroups group)
		{
			this.group = group;

		}
		
	}
}
