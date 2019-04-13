/*
 * Создано в SharpDevelop.
 * Пользователь: P.Dubovikov
 * Дата: 06.10.2016
 * Время: 11:26
 * 
 * Для изменения этого шаблона используйте меню "Инструменты | Параметры | Кодирование | Стандартные заголовки".
 */
using System;
using System.Collections.Generic;


namespace gcodeparser.gcodes
{

	public class FeedRateMode
	{

		public enum FeedRateModes
		{
			G93 = GCodeGroups.FeedRateMode,
			G94 = GCodeGroups.FeedRateMode,
			G95 = GCodeGroups.FeedRateMode	
		}

		GCodeGroups group;

		FeedRateMode(GCodeGroups group)
		{
			this.group = group;
			
		}

	}

}
