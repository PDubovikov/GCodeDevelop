/*
 * Создано в SharpDevelop.
 * Пользователь: P.Dubovikov
 * Дата: 13.10.2016
 * Время: 16:11
 * 
 * Для изменения этого шаблона используйте меню "Инструменты | Параметры | Кодирование | Стандартные заголовки".
 */
using System;
using System.Collections.Generic;
using gcodeparser.exceptions;
using gcodeparser;

namespace gcodeparser
{

	public interface IMachineController
	{

		void startBlock(GCodeParser parser, MachineStatus machineStatus, IDictionary<string, ParsedWord> block);

		
		void endBlock(GCodeParser parser, MachineStatus machineStatus, IDictionary<string, ParsedWord> block);

		
		void end(GCodeParser parser, MachineStatus machineStatus);

	}
}
