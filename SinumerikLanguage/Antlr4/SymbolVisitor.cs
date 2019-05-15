using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Tree;
using static SinumerikParser;

namespace SinumerikLanguage.Antlr4
{
    public class SymbolVisitor : SinumerikBaseVisitor<SLValue>
    {
        private Dictionary<String, Function> functions;

        public SymbolVisitor(Dictionary<String, Function> functions)
        {
            this.functions = functions;
        }

        public override SLValue VisitFunctionDecl(FunctionDeclContext ctx)
        {

            List<ITerminalNode> param = ctx.idList() != null ? ctx.idList().Identifier().ToList() : new List<ITerminalNode>();
            IParseTree block = ctx.block();
            String id = ctx.Identifier().GetText() + param.Count;
            functions[id] = new Function(param, block);
            return SLValue.VOID;
        }
    }
}
