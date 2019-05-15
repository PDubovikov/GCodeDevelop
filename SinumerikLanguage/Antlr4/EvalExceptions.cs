using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SinumerikLanguage.Antlr4
{
    public class EvalException : SystemException
    {
        public EvalException(ParserRuleContext ctx) : this("Illegal expression: " + ctx.GetText(), ctx)
        {
            
        }

        public EvalException(String msg, ParserRuleContext ctx) : base(msg + " line:" + ctx.Start.Line)
        {
            
        }

    }
}
