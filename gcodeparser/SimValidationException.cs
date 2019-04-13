/*
 * Создано в SharpDevelop.
 * Пользователь: P.Dubovikov
 * Дата: 30.09.2016
 * Время: 9:44
 * 
 * Для изменения этого шаблона используйте меню "Инструменты | Параметры | Кодирование | Стандартные заголовки".
 */
using System;

namespace gcodeparser.exceptions
{
	/// <summary>
	/// Description of SimValidationException.
	/// </summary>
	public class SimValidationException : SimException {
		public SimValidationException(String message) : base(message) {
        
    	}

		public SimValidationException(String message, Exception cause) : base (message, cause) {
        
    	}

		public SimValidationException(Exception cause) : base(cause) {
        
    	}

//		public SimValidationException(String message, Exception cause, Boolean enableSuppression, Boolean writableStackTrace) : base(message, cause, enableSuppression, writableStackTrace) {
//        
//    	}
	}
}
