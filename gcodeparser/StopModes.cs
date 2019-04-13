/*
 * Создано в SharpDevelop.
 * Пользователь: P.Dubovikov
 * Дата: 06.10.2016
 * Время: 13:49
 * 
 * Для изменения этого шаблона используйте меню "Инструменты | Параметры | Кодирование | Стандартные заголовки".
 */
using System;
using System.Collections.Generic;

namespace gcodeparser.gcodes
{

	public sealed class StopMode
	{

		public enum StopModes
		{
			M0 = GCodeGroups.StopMode,
			M1 = GCodeGroups.StopMode,
			M2 = GCodeGroups.StopMode,
			M30 = GCodeGroups.StopMode,
			M60 = GCodeGroups.StopMode
		}


		GCodeGroups group;

		internal StopMode(GCodeGroups group)
		{
			this.group = group;

		}

	}
}
