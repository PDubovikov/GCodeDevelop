/*
 * Создано в SharpDevelop.
 * Пользователь: P.Dubovikov
 * Дата: 06.10.2016
 * Время: 13:38
 * 
 * Для изменения этого шаблона используйте меню "Инструменты | Параметры | Кодирование | Стандартные заголовки".
 */

using System;
using System.Collections.Generic;


namespace gcodeparser.gcodes
{

	public class PredefinedPosition
	{
	
		public enum PredefinedPositions
		{
			G30 = GCodeGroups.PredefinedPosition,
			G30_1 = GCodeGroups.PredefinedPosition
		}

		
		GCodeGroups group;

		PredefinedPosition(GCodeGroups group)
		{
			this.group = group;

		}
		
	}

}
