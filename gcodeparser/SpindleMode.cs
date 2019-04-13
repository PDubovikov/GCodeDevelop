/*
 * Создано в SharpDevelop.
 * Пользователь: P.Dubovikov
 * Дата: 06.10.2016
 * Время: 13:47
 * 
 * Для изменения этого шаблона используйте меню "Инструменты | Параметры | Кодирование | Стандартные заголовки".
 */
using System;
using System.Collections.Generic;

namespace gcodeparser.gcodes
{

	public sealed class SpindleMode
	{

		public enum SpindleModes
		{
			M3 = GCodeGroups.SpindleMode,
			M4 = GCodeGroups.SpindleMode,
			M5 = GCodeGroups.SpindleMode,
			M6 = GCodeGroups.SpindleMode,
			M41 = GCodeGroups.SpindleMode,
			M42 = GCodeGroups.SpindleMode,
			M43 = GCodeGroups.SpindleMode,
			M44 = GCodeGroups.SpindleMode,
			M71 = GCodeGroups.SpindleMode,
			M72 = GCodeGroups.SpindleMode,
			M73 = GCodeGroups.SpindleMode,
			M74 = GCodeGroups.SpindleMode,
			G96 = GCodeGroups.SpindleMode	
		}

		GCodeGroups group;

		SpindleMode(GCodeGroups group)
		{
			this.group = group;

		}
	}
}
