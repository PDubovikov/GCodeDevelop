/*
 * Создано в SharpDevelop.
 * Пользователь: P.Dubovikov
 * Дата: 06.10.2016
 * Время: 13:46
 * 
 * Для изменения этого шаблона используйте меню "Инструменты | Параметры | Кодирование | Стандартные заголовки".
 */
using System;
using System.Collections.Generic;

namespace gcodeparser.gcodes
{

	public sealed class SFOverrideMode
	{

		public enum SFOverrideModes
		{
			M48 = GCodeGroups.SFOverrideMode,
			M49 = GCodeGroups.SFOverrideMode
		}


		GCodeGroups group;

		SFOverrideMode(GCodeGroups group)
		{
			
			this.group = group;
		}

	}
}
