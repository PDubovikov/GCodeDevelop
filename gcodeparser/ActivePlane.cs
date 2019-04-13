/*
 * Создано в SharpDevelop.
 * Пользователь: P.Dubovikov
 * Дата: 30.09.2016
 * Время: 10:34
 * 
 * Для изменения этого шаблона используйте меню "Инструменты | Параметры | Кодирование | Стандартные заголовки".
 */

using System ;
using System.Collections ;

namespace gcodeparser.gcodes
{
	public class ActivePlane : ModalGroups
	{

		public enum ActPlane {
		
		G17 = GCodeGroups.ActivePlane,
		G18 = GCodeGroups.ActivePlane,
		G19 = GCodeGroups.ActivePlane }

	    GCodeGroups group ;

 	   	ActivePlane(GCodeGroups group) 
 	   	{
    	    this.group = group;
    	}

    		public Boolean isInGroup(GCodeGroups group)
    		{
    			return this.group == group;
    		}

	}
}