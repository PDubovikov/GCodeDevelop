/*
 * Создано в SharpDevelop.
 * Пользователь: P.Dubovikov
 * Дата: 06.10.2016
 * Время: 13:44
 * 
 * Для изменения этого шаблона используйте меню "Инструменты | Параметры | Кодирование | Стандартные заголовки".
 */

using System;
using System.Collections.Generic;


namespace gcodeparser.gcodes
{

	public class RetrackMode
	{

		public enum RetrackModes
		{
			
			G98 = GCodeGroups.RetrackMode,
			G99 = GCodeGroups.RetrackMode
		}

		GCodeGroups group;

		internal RetrackMode(GCodeGroups group)
		{
			this.group = group;

		}
		
	}
}