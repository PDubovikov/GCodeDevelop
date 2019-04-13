/*
 * Создано в SharpDevelop.
 * Пользователь: P.Dubovikov
 * Дата: 30.09.2016
 * Время: 10:00
 * 
 * Для изменения этого шаблона используйте меню "Инструменты | Параметры | Кодирование | Стандартные заголовки".
 */
using System;

namespace gcodeparser.exceptions
{
	/// <summary>
	/// Description of SimParsingException.
	/// </summary>
	public class SimParsingException : SimException {
		public SimParsingException(String message) : base(message) {
        
    	}

		public SimParsingException(String message, Exception cause) : base(message, cause) {
        
    	}

		public SimParsingException(Exception cause) : base(cause) {
        
    	}

//		public SimParsingException(String message, Exception cause, Boolean enableSuppression, Boolean writableStackTrace) : base(message, cause, enableSuppression, writableStackTrace) {
//        
//    	}
	}
}
