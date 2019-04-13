/*
 * Создано в SharpDevelop.
 * Пользователь: P.Dubovikov
 * Дата: 06.10.2016
 * Время: 13:36
 * 
 * Для изменения этого шаблона используйте меню "Инструменты | Параметры | Кодирование | Стандартные заголовки".
 */
using System;
using System.Collections.Generic;


namespace gcodeparser.gcodes
{

	public class PathControleMode
	{

		public enum PathControleModes
		{
			G61 = GCodeGroups.PathControleMode,
			G61_1 = GCodeGroups.PathControleMode,
			G64 = GCodeGroups.PathControleMode,
			G9 = GCodeGroups.PathControleMode	
		}

		GCodeGroups group;

		internal PathControleMode(GCodeGroups group)
		{
			this.group = group;

		}

	}

}