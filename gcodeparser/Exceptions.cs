/*
 * Создано в SharpDevelop.
 * Пользователь: P.Dubovikov
 * Дата: 30.09.2016
 * Время: 9:14
 * 
 * Для изменения этого шаблона используйте меню "Инструменты | Параметры | Кодирование | Стандартные заголовки".
 */
using System;

namespace gcodeparser.exceptions
{
	/// <summary>
	/// Description of Exceptions.
	/// </summary>
	public class SimException : Exception {
		public SimException(String message) : base(message) { 
		
		}
    
		public SimException(String message, Exception cause) : base(message, cause) {
        
    	}

		public SimException(Exception cause) : base(cause.ToString()) {
        
    	}

//		public SimException(String message, Exception cause, Boolean enableSuppression, Boolean writableStackTrace) : base(message, cause, enableSuppression, writableStackTrace) {
//			
//    	}
	}
}
