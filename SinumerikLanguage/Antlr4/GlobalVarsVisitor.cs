using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using static SinumerikParser;

namespace SinumerikLanguage.Antlr4
{
    public class GlobalVarsVisitor : SinumerikBaseVisitor<SLValue>
    {
        private Scope _scope;

        public GlobalVarsVisitor(Scope scope)
        {
            this._scope = scope;
        }

        public override SLValue VisitParse(ParseContext ctx)
        {
            VisitBlock(ctx.block());

            return new SLValue(false);
        }

        public override SLValue VisitBlock(BlockContext ctx)
        {
            for (int i = 0; i < ctx.statement().Length; i++)
            {
                if (ctx.statement()[i].returnStatement() != null)
                {
                    _scope = _scope.parent();
                    return SLValue.VOID;
                }

                this.Visit(ctx.statement()[i]);
            }

            _scope = _scope.parent();
            return SLValue.VOID;
        }

        public override SLValue VisitVardefinition(VardefinitionContext ctx)
        {
            SLValue newVal = null;
            List<SLValue> list = new ArrayList<SLValue>();
            String type = ctx.typeDef().GetText();

            foreach (var item in ctx.varlist())
            {
                if (item.expression() != null) { newVal = this.Visit(item.expression()); }
                else { newVal = _scope.GetDefaultValue(type); }

                if (item.indexes() != null)
                {
                    //     List<ExpressionContext> exps = item.indexes().expression().ToList();
                    foreach (var expr in item.indexes().expression())
                    {
                        list.AddRange(new SLValue[(int)this.Visit(expr).asDouble()]);

                    }
                    _scope.GlobalAssign(item.Identifier().GetText(), new SLValue(list));

                    //  return new SLValue(list);
                }
                else
                {
                    _scope.GlobalAssign(item.Identifier().GetText(), newVal);
                }

                newVal = null;
            }

            return SLValue.VOID;
        }

        public override SLValue VisitAssignment(AssignmentContext ctx)
        {
            
            SLValue newVal = this.Visit(ctx.expression());

            if (ctx.indexes() != null)
            {
                SLValue val = _scope.resolve(ctx.Identifier().GetText());
                List<ExpressionContext> exps = ctx.indexes().expression().ToList();
                if (val == null) { throw new EvalException(ctx); }
                SetAtIndex(ctx, exps, val, newVal);
            }
            else
            {
                String id = ctx.Identifier().GetText();
                _scope.GlobalAssign(id, newVal);
            }
            return SLValue.VOID;
        }

        private void SetAtIndex(ParserRuleContext ctx, List<ExpressionContext> indexes, SLValue val, SLValue newVal)
        {
            for (int i = 0; i < indexes.Count - 1; i++)
            {
                SLValue idx = this.Visit(indexes[i]);
                if (!idx.isNumber())
                {
                    throw new EvalException(ctx);
                }
                //   val = val.asList()[((int)idx.asDouble())];
            }
            SLValue _idx = this.Visit(indexes[(indexes.Count - 1)]);
            if (!_idx.isNumber())
            {
                throw new EvalException(ctx);
            }
            val.asList().Insert((int)_idx.asDouble(), newVal);
        }

        public override SLValue VisitNumberExpression(NumberExpressionContext ctx)
        {
            return new SLValue(Convert.ToDouble(ctx.GetText().Replace('.', ',')));
        }

        public override SLValue VisitBoolExpression(BoolExpressionContext ctx)
        {
            return new SLValue(Convert.ToBoolean(ctx.GetText()));
        }

        public override SLValue VisitStringExpression(StringExpressionContext ctx)
        {
            String text = ctx.GetText();
            text = text.Substring(1, text.Length - 2);
            SLValue val = new SLValue(text);
            if (ctx.indexes() != null)
            {
                List<ExpressionContext> exps = ctx.indexes().expression().ToList();
                val = resolveIndexes(val, exps);
            }
            return val;
        }

        private SLValue resolveIndexes(SLValue val, List<ExpressionContext> indexes)
        {
            foreach (ExpressionContext ec in indexes)
            {
                SLValue idx = this.Visit(ec);
                if (!idx.isNumber() || (!val.isList() && !val.isString()))
                {
                    throw new EvalException("Problem resolving indexes on " + val + " at " + idx, ec);
                }
                int i = (int)idx.asDouble();
                if (val.isString())
                {
                    val = new SLValue(val.asString().Substring(i, i + 1));
                }
                else
                {
                    val = val.asList()[i];
                }
            }
            return val;
        }
    }
}
