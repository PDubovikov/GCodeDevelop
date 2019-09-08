using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using SinumerikLanguage.Antlr4;
using ICSharpCode.AvalonEdit.Document;

namespace GCD.Model
{
    public enum MachineType
    {
        Turn,
        Mill
    }

    public class SinumerikCompiler
    {
        private StringBuilder _gcodeOutput;
        private string path = Path.GetDirectoryName(Directory.GetCurrentDirectory());
        private string baseDir, mainDir, subDir, mainIniFile;
        private Scope mainScope;
        private Dictionary<string, Function> functions;
        private TextWriter _errorTextWriter;
        private TextDocument _text;
        private Lexer mainLexer;

        public SinumerikCompiler(TextDocument text, TextWriter errorTextWriter, MachineType Type)
        {
            _gcodeOutput = new StringBuilder();
            functions = new Dictionary<string, Function>();
            baseDir = path.Remove(path.Length - 4);
            mainDir = baseDir + GetControllerPath(Type) + "\\Main\\";
            subDir = baseDir + GetControllerPath(Type) + "\\Sub\\";
            mainIniFile = mainDir + "Main.ini";
            mainScope = new Scope();
            _errorTextWriter = errorTextWriter;
            _text = text;

            CompilePrepare();

        }

        private string GetControllerPath(MachineType type)
        {
            string path = "";

            switch(type)
            {
                case MachineType.Mill:
                    path = "\\MachineControls\\Sinumerik840D_Mill";
                    break;
                case MachineType.Turn:
                    path = "\\MachineControls\\Sinumerik840D_Turn";
                    break;
            }
           
            return path;
        }
        
        public StringBuilder Compile()
        {
            Scope scope = new Scope(mainScope);
            SinumerikParser mainParser = new SinumerikParser(new CommonTokenStream(mainLexer), null, _errorTextWriter);
            mainParser.BuildParseTree = true;
            IParseTree mainTree = mainParser.parse();
            //    outputTextWriter.Close();
            SymbolVisitor mainSymbolVisitor = new SymbolVisitor(functions);
            mainSymbolVisitor.Visit(mainTree);
            EvalVisitor visitor = new EvalVisitor(scope, functions, _gcodeOutput);
            visitor.Visit(mainTree);

            return _gcodeOutput;
        }

        private void CompilePrepare()
        {
                if (File.Exists(mainIniFile))
                {
                    SinumerikLexer definitionLexer = new SinumerikLexer(CharStreams.fromPath(mainIniFile), null, _errorTextWriter);
                    SinumerikParser definitionParser = new SinumerikParser(new CommonTokenStream(definitionLexer), null, _errorTextWriter);
                    definitionParser.BuildParseTree = true;
                    IParseTree definitionTree = definitionParser.parse();

                    GlobalVarsVisitor globalVars = new GlobalVarsVisitor(mainScope);
                    globalVars.Visit(definitionTree);

                }

                foreach (var fileName in Directory.EnumerateFiles(subDir))
                {
                    SinumerikLexer subLexer = new SinumerikLexer(CharStreams.fromPath(fileName), null, _errorTextWriter);
                    SinumerikParser subParser = new SinumerikParser(new CommonTokenStream(subLexer), null, _errorTextWriter);

                    subParser.BuildParseTree = true;
                    IParseTree subTree = subParser.parse();

                    SymbolVisitor subSymbolVisitor = new SymbolVisitor(functions);
                    subSymbolVisitor.Visit(subTree);

                }

                mainLexer = new SinumerikLexer(CharStreams.fromstring(_text.Text), null, _errorTextWriter);

        }
    }
}
