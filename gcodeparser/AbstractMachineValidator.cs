/*
 * Создано в SharpDevelop.
 * Пользователь: P.Dubovikov
 * Дата: 13.10.2016
 * Время: 14:08
 * 
 * Для изменения этого шаблона используйте меню "Инструменты | Параметры | Кодирование | Стандартные заголовки".
 */
using System;
using System.Collections.Generic;
using System.Reflection;
using gcodeparser.exceptions;

namespace gcodeparser
{

	public abstract class AbstractMachineValidator
	{


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract void preVerify(java.util.Map<String, ParsedWord> block) throws com.rvantwisk.gcodeparser.exceptions.SimException;
		public abstract void preVerify(IDictionary<string, ParsedWord> block);

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract void postVerify(MachineStatus machineStatus) throws com.rvantwisk.gcodeparser.exceptions.SimException;
		public abstract void postVerify(MachineStatus machineStatus);

		/// <summary>
		/// Helper to find multiple words in teh same block
		/// </summary>
		/// <param name="block"> </param>
		/// <param name="enumClass"> </param>
		/// @param <T>
		/// @return </param>
		protected internal virtual bool hasMultipleWords<T>(IDictionary<string, ParsedWord> block, Type enumClass) where T : IEnumerable<T>
		{
			return wordCount<int>(block, enumClass) > 1;
		}

		/// <summary>
		/// Returns a word count within teh current block
		/// This is usefull to find multiple the same words within a modal group in the current block
		/// </summary>
		/// <param name="block"> </param>
		/// <param name="enumClass"> </param>
		/// @param <T>
		/// @return </param>
		protected internal virtual int wordCount<T>(IDictionary<string, ParsedWord> block, Type enumClass)
		{
			int wordCount = 0;

	//		T items = enumClass.GetEnumNames() ;

			foreach (T item in enumClass.GetEnumValues())
			{
				if (block.ContainsKey(item.ToString()))
				{
					wordCount++;
				}
			}
			return wordCount;
		}

		/// <summary>
		/// Returns true of the block contains any of hasAnyOfThis </summary>
		/// <param name="block"> </param>
		/// <param name="hasAnyOfThis">
		/// @return </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: protected boolean hasAny(java.util.Map<String, Object> block, final String[] hasAnyOfThis)
		protected internal virtual bool hasAny(IDictionary<string, object> block, string[] hasAnyOfThis)
		{
			foreach (String item in hasAnyOfThis)
			{
				if (block.ContainsKey(item))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Returns true of the block contains any of hasAnyOfThis </summary>
		/// <param name="block"> </param>
		/// <param name="hasAnyOfThis">
		/// @return </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: protected boolean hasAny(java.util.Set<String> block, final String[] hasAnyOfThis)
		protected internal virtual bool hasAny(ISet<string> block, string[] hasAnyOfThis)
		{
			foreach (String item in hasAnyOfThis)
			{
				if (block.Contains(item))
				{
					return true;
				}
			}
			return false;
		}
	}

}
