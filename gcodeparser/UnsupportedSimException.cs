/*
 * Создано в SharpDevelop.
 * Пользователь: P.Dubovikov
 * Дата: 30.09.2016
 * Время: 10:05
 * 
 * Для изменения этого шаблона используйте меню "Инструменты | Параметры | Кодирование | Стандартные заголовки".
 */
using System;

namespace gcodeparser.exceptions
{
	/// <summary>
	/// Description of UnsupportedSimException.
	/// </summary>
	public class UnsupportedSimException : SimException {

		public UnsupportedSimException(String message) : base(message) {
       
    	}

		public UnsupportedSimException(String message, Exception cause) : base(message, cause) {
        
    	}

		public UnsupportedSimException(Exception cause) : base(cause) {
        
    	}

	}
}
