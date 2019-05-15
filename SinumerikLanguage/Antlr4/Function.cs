using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using static SinumerikParser;

namespace SinumerikLanguage.Antlr4
{
    public class Function
    {
        private List<ITerminalNode> _param;
        private IParseTree block;

        public Function(List<ITerminalNode> param, IParseTree block)
        {
            this._param = param;
            this.block = block;

        }

        public SLValue invoke(List<ExpressionContext> param, Dictionary<String, Function> functions, Scope scope, StringBuilder gcodeBuffer)
        {
            if (param.Count != this._param.Count) {
                throw new Exception("Illegal Function call");
            }
            Scope scopeNext = new Scope(null); // create function scope

            EvalVisitor evalVisitor = new EvalVisitor(scope, functions, null);
            for (int i = 0; i < this._param.Count; i++) {
                SLValue value = evalVisitor.Visit(param[i]);
                scopeNext.assignParam(this._param[i].GetText(), value);
            }
            EvalVisitor evalVistorNext = new EvalVisitor(scopeNext, functions, gcodeBuffer);

            SLValue ret = SLValue.VOID;
            try
            {
                evalVistorNext.Visit(this.block);
            }
            catch (ReturnValue returnValue)
            {
                ret = returnValue.value;
            }
            return ret;
        }
    }
}
