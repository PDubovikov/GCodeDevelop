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

	public interface MachineController
	{

		void startBlock(GCodeParser parser, MachineStatus machineStatus, IDictionary<string, ParsedWord> block);

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void endBlock(GCodeParser parser, MachineStatus machineStatus, java.util.Map<String, ParsedWord> block) throws com.rvantwisk.gcodeparser.exceptions.SimException;
		void endBlock(GCodeParser parser, MachineStatus machineStatus, IDictionary<string, ParsedWord> block);

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void end(GCodeParser parser, MachineStatus machineStatus) throws com.rvantwisk.gcodeparser.exceptions.SimException;
		void end(GCodeParser parser, MachineStatus machineStatus);

	}
}
