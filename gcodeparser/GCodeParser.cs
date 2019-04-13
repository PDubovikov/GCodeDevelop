using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using gcodeparser.exceptions;
using System.Windows.Media.Media3D;
using gcodeparser.gcodes ;

namespace gcodeparser
{
	public class GCodeParser
	{
//		private static string SEPARATOR = System.getProperty("line.separator");
		private readonly MachineStatus machineStatus = new MachineStatus(); // kept's track of machine status after end of block
		private readonly MachineStatus intermediateStatus = new MachineStatus(); // Keeps tracking of machine status during block processing
		private readonly MachineController[] machineController; // A machine controller to send parsed block's + machine status into
		private readonly AbstractMachineValidator machineValidator; // A machine controller to send parsed block's + machine status into
//		private readonly Regex GCODEPATTERN = new Regex("([GXYZABCDFHIJKLMNPQRSTUVW]o?)\\s*([0-9.+-]+)?(\\s*/?\\s*)([0-9.+-]+)?", RegexOptions.Compiled);
		private static readonly string gcodepattern = @"([aA-zZ]{1,7})=?([AI][C])?\(?([-+]?\d*\.?\d*)?\)?";
		private readonly Regex Gcodepattern = new Regex(gcodepattern, RegexOptions.Compiled) ;
		private readonly Regex COMMENTS1 = new Regex("\\(.*\\)", RegexOptions.Compiled); // Comment between ()
		private readonly Regex COMMENTS2 = new Regex("\\;.*", RegexOptions.Compiled); // comment after ;
		private readonly Regex G04 = new Regex("G[0]?[4]\\s*[aA-zZ]?") ;
		private readonly Regex WORD = new Regex("(DIAMON|DIAMOF|DIAM90)", RegexOptions.Compiled) ;
//		private readonly Regex COMAND = new Regex(@"([A-Z]{3,})\s*([A-Z])?\s*([-+]?\d*\.?\d*)?\s*([A-Z])?\s*([-+]?\d*\.?\d*)?\s*([A-Z])?\s*([-+]?\d*\.?\d*)?(?=\s|[;])", RegexOptions.Compiled);
//		private readonly Regex COMAND = new Regex(@"(TRANS|ATRANS|ROT|AROT)(?=\s|[;])", RegexOptions.Compiled);
//		private DecimalFormat wordFormatter = new DecimalFormat("#.#########"); // Formatting and trimming of numbers
		private string currentLine = ""; // Hold's the current line between begin and endblock calls
		private int currentLineNumber = 1;


		public GCodeParser(AbstractMachineValidator machineValidator, StringBuilder input, params MachineController[] machineController)
		{
			foreach (object c in machineController)
			{
				if (!(c is MachineController))
				{
					throw new System.ArgumentException("StatisticLimitsController only accepts type's of MachineController");
				}
			}

			this.machineController = machineController;
			this.machineValidator = machineValidator;
			
			string[] lines = input.ToString().Split('\n');
			foreach (String line in lines)
			{

					currentLine = line;
					currentLineNumber++;
					parseLine();
				
			}
			foreach (MachineController controller in this.machineController)
			{
				controller.end(this, intermediateStatus);
			}

		}
				

		private void parseLine()
		{
			// Remove comments between () and all comments after ;
				StringBuilder parsedLine = new StringBuilder(currentLine) ;
				Match g4 = G04.Match(parsedLine.ToString()) ;
				Match comment1 = COMMENTS1.Match(parsedLine.ToString()) ;
				Match comment2 = COMMENTS2.Match(parsedLine.ToString()) ;
				if (g4.Success)
				{
//					currentLine.Replace(g4.Value, "") ;						
				}
				if (comment1.Success)
				{
			//		parsedLine.Remove(comment1.Index, comment1.Length) ; // for Fanuc, Okuma, Haas
				}
				if(comment2.Success)
				{
					parsedLine.Remove(comment2.Index, comment2.Length) ;
				}
				
			ISet<string> wordcmd = new HashSet<string>();
        	String parsedWords ;
        	if ((parsedWords = findWords(parsedLine)) != null)
        	{
        		wordcmd.Add(parsedWords);
        		
        	}	
				
      //  	Console.WriteLine("currentPosition: " + "X:"+ machineStatus.newPosition[MachineStatus.Axis.X].Value + "Y:"+ machineStatus.newPosition[MachineStatus.Axis.Y].Value + "Z:"+ machineStatus.newPosition[MachineStatus.Axis.Z].Value ) ;
      //		Console.WriteLine(machineStatus.newLine) ;
			// A map that holds all parsed codes
			IDictionary<string, ParsedWord> block = new Dictionary<string, ParsedWord>(10);
			IDictionary<string, ParsedCmdGroup> CmdBlock = new Dictionary<string, ParsedCmdGroup>();
			// Hold's the current parsed word
			ParsedWord thisWord;
			ParsedWord addWord ;
			ParsedCmdGroup thisCmdGroup ;
			
		
//			while ((addWord = findWordInBlock(machineStatus.newLine)) != null)
//			{
//					int pos = machineStatus.newLine.ToString().IndexOf(addWord.asRead);
//					machineStatus.newLine.Remove(pos, pos + addWord.asRead.Length - pos).Insert(pos, "");
//				
//					string blockKey = addWord.word;
//					block.Add(blockKey, addWord);								
//			}
			
			
			while ((thisWord = findWordInBlock(parsedLine)) != null)
			{
				
				// Devide string by block
				int pos = parsedLine.ToString().IndexOf(thisWord.asRead);
				parsedLine.Remove(pos, pos + thisWord.asRead.Length - pos).Insert(pos, "");
				
				
				// We can have multiple G/M words within a block, so we move them to the 'key'
				string blockKey = thisWord.word;
				string CmdBlockKey = "" ;
				
				if (blockKey.Equals("G") || blockKey.Equals("M"))
				{
					blockKey = thisWord.parsed.Replace('.', '_'); // Store gwords with a . as _ Example G33.1
				}
				if (blockKey.Contains("TRANS") || blockKey.Contains("ROT") || blockKey.Contains("SCALE") || blockKey.Contains("MIRROR")  )
				{
					string Block = parsedLine.ToString();
					Match CmdMatcher = Gcodepattern.Match(Block);
					
					while(CmdMatcher.Success)
					{
						string g0 = CmdMatcher.Groups[0].ToString();
						string g1 = CmdMatcher.Groups[1].ToString();
						string g3 = CmdMatcher.Groups[3].ToString().Replace('.',',');
						if (g3 != "")
						{
							double v = Convert.ToDouble(g3);
							string value = String.Format("{0:}",v);
							thisCmdGroup = new ParsedCmdGroup(blockKey, g1, v, g1 + value, g0) ;
							CmdBlockKey = thisCmdGroup.AxisName1 ;
							CmdBlock.Add(CmdBlockKey, thisCmdGroup) ;
						}
							
						CmdMatcher = CmdMatcher.NextMatch();
					}
					
					parsedLine.Clear();
					
				}
				
				
				if (block.ContainsKey(blockKey))
				{
					throw new SimValidationException("Multiple " + thisWord.word + " words on one line.");
				}
				else
				{
					block.Add(blockKey, thisWord);
				}
			}
			
				
			foreach (KeyValuePair<string, ParsedWord> pair in block)
			{
//				Console.WriteLine("Key : " + pair.Key + " " + "Value : " + pair.Value) ;

			}
			

			// First verify if the block itself is valid before we process it
			if (machineValidator != null)
			{
				machineValidator.preVerify(block);
			}

			// Copy to intermediate status to ensure our machine status is always valid
			intermediateStatus.copyFrom(machineStatus);
			// Notify the controller that we are about to start a new block, the block itself is valid, for example there we be no G1's and G0 on one line
			foreach (MachineController controller in this.machineController)
			{
				controller.startBlock(this, intermediateStatus, block);
			}
			intermediateStatus.startBlock();

			// Copy the block to the machine
			intermediateStatus.setWord(wordcmd);
			intermediateStatus.setCommandBlk(CmdBlock, block) ;
			intermediateStatus.setBlock(block);

			// Block en, no more data will come in for this block
			intermediateStatus.endBlock();

			// Verify machine's state, for example if a R was found, do we also have a valid G to accompany with it?
			if (machineValidator != null)
			{
				machineValidator.postVerify(intermediateStatus);
			}

			// Notify the controller that everything was ok, now teh controller start 'running' the data
			foreach (MachineController controller in this.machineController)
			{
				controller.endBlock(this, intermediateStatus, block);
			}
			
			
			// setup new and valid machine status
			machineStatus.copyFrom(intermediateStatus);			
		}

		public string findWords(StringBuilder word)
    	{
			Match mword = WORD.Match(word.ToString());
    	
    		while(mword.Success)
    		{
    			string g0 = mword.Groups[0].ToString();
    			if (g0 != "")
    			{
    				return g0 ; 
    			}
    			
    			mword = mword.NextMatch();
    		}
    		return null;
    	}
		
		public static bool IsOneOf<T>(T value, params T[] items)
		{
			for (int i = 0; i<items.Length; ++i)
			{
				if(items[i].Equals(value))
				{
					return true ;
				}
				
			}
			return false ;
		}
		
		/// <summary>
		/// Find the next gcode in the current block
		/// </summary>
		/// <param name="gcodeBlock"> </param>
		/// <returns> TODO: Parse commands </returns>

		public virtual ParsedWord findWordInBlock(StringBuilder gcodeBlock)
		{
			Match GcodeMatcher = Gcodepattern.Match(gcodeBlock.ToString());
			
			while (GcodeMatcher.Success)
			{
				try
				{
					string g0 = GcodeMatcher.Groups[0].ToString();		// summary
					string g1 = GcodeMatcher.Groups[1].ToString();		// axis name || command name
					string g2 = GcodeMatcher.Groups[2].ToString();		// coord mode
					string g3 = GcodeMatcher.Groups[3].ToString().Replace('.',','); 	// value
					if (IsOneOf(g1, "TRANS", "ATRANS", "ROT", "AROT", "SCALE", "ASCALE", "MIRROR", "AMIRROR"))
					{
						return new ParsedWord(g1, "", "", double.NaN, g0) ;
					}
										
					double v = Convert.ToDouble(g3);
					string value = String.Format("{0:}",v);
					
					// public ParsedWord(String word, String parsed, Double value, String asRead) {..}
					// When a G block was found and a G 'value' use that examples G4/10 or G4 10
					if (g1 == "G")
					{
							return new ParsedWord(g1, "", g1 + value, double.NaN, g0);
						
					}
					
						return new ParsedWord(g1, g2, g1 + value, v, g0);

				}
				catch (Exception e)
				{
			//		 Console.WriteLine(e.Message + ":"+gcodeBlock);
				}
				GcodeMatcher = GcodeMatcher.NextMatch();
			}
			
			return null;
		}
			
		/// <summary>
		/// Test of the current command holds a specific word
		/// If the word contains more then 1 character we check the complete word, else we check the letter
		/// </summary>
		/// <param name="currentBlock"> Block </param>
		/// <param name="word">         A word examples G94, A, B, G0 G30.1, G30_1
		/// @return </param>

		private bool hasWord(IDictionary<string, ParsedWord> currentBlock, string word)
		{
			
			return currentBlock.ContainsKey(word);
			
		}

		/// <summary>
		/// Returns a word count within teh current block
		/// This is usefull to find multiple the same words within a modal group in the current block
		/// </summary>
		/// <param name="currentBlock"> </param>
		/// <param name="enumClass"> </param>
		/// @param <T>
		/// @return </param>
		private int wordCount<T>(IDictionary<string, ParsedWord> currentBlock, Type enumClass)
		{
			int wordCount = 0;

//			T[] items = enumClass.EnumConstants;

			foreach (T item in enumClass.GetEnumValues())
			{
				if (hasWord(currentBlock, item.ToString()))
				{
					wordCount++;
				}
			}
			return wordCount;
		}

		/// <summary>
		/// Replace a word within a GCODE block
		/// </summary>
		/// <param name="GCodeBLock"> </param>
		/// <param name="replaceWith">
		/// @return </param>
		/// <exception cref="Exception"> </exception>

		protected internal virtual string replaceWord(string GCodeBLock, string replaceWith)
		{
			if (replaceWith.StartsWith("M", StringComparison.Ordinal) || replaceWith.StartsWith("G", StringComparison.Ordinal))
			{
				throw new SimValidationException("M and G words cannot be replaced at this moment.");
			}

			ParsedWord word = findWordInBlock(new StringBuilder(GCodeBLock));
			if (word == null)
			{
				return GCodeBLock + replaceWith;
			}
			else
			{
				return GCodeBLock.Replace(word.asRead, replaceWith);
			}
		}

		/// <summary>
		/// Helper to find multiple words in teh same block
		/// </summary>
		/// <param name="currentBlock"> </param>
		/// <param name="enumClass"> </param>
		/// @param <T>
		/// @return </param>
		private bool hasMultipleWords<T>(IDictionary<string, ParsedWord> currentBlock, Type enumClass)
		{
			return wordCount<int>(currentBlock, enumClass) > 1;
		}

		public virtual string getCurrentLine()
		{

				return currentLine;
		}

		public virtual int getCurrentLineNumber()
		{

				return currentLineNumber;
		}
	}

}
