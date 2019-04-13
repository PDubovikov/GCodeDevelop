/*
 * Создано в SharpDevelop.
 * Пользователь: P.Dubovikov
 * Дата: 06.10.2016
 * Время: 9:50
 * 
 * Для изменения этого шаблона используйте меню "Инструменты | Параметры | Кодирование | Стандартные заголовки".
 */
using System ;

 
namespace gcodeparser.gcodes
{

	public class AxisOffset : ModalGroups
	{

		public enum AxisOffsets
		{
    		G92 = GCodeGroups.AxisOffset,
   			G92_1 = GCodeGroups.AxisOffset,
    		G92_2 = GCodeGroups.AxisOffset,
    		G92_3 = GCodeGroups.AxisOffset,
    		G500 = GCodeGroups.AxisOffset
			
		}
		
		   	GCodeGroups group ;

    		public AxisOffset(GCodeGroups group)
    		{
    	  		this.group = group ;
    		}		
		
		    public Boolean isInGroup(GCodeGroups group)
    		{
    			return this.group == group;
    		}
    		
	}

}
