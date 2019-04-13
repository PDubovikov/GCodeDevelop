/*
 * Создано в SharpDevelop.
 * Пользователь: P.Dubovikov
 * Дата: 30.09.2016
 * Время: 10:25
 * 
 * Для изменения этого шаблона используйте меню "Инструменты | Параметры | Кодирование | Стандартные заголовки".
 */
using System;
using System.Windows.Media.Media3D ;

namespace gcodeparser
{
	/// <summary>
	/// Description of ParsedWord.
	/// </summary>
public  class ParsedWord {
    public readonly String word;    // example: G or F
    public readonly String parsed;  // example: G0 or F100.0
    public readonly String cmode;   // example: IC or AC
    public readonly Double value;   // example: 0 or 100.0
    public readonly String asRead;  // example: G00 or F100.00

    public ParsedWord(String word, String cmode, String parsed, Double value, String asRead) {
        this.word = word;
        this.cmode = cmode;
        this.parsed = parsed;
        this.value = value;
        this.asRead = asRead;
        
    }
      

    
   public override String ToString() {
        return "ParsedWord{" +
                ", parsed='" + parsed + '\'' +
                ", value=" + value +
        		", cmode=" + cmode +
                '}';
    }
}
}
