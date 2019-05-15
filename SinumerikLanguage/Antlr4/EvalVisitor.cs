using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using static SinumerikParser;

namespace SinumerikLanguage.Antlr4
{
    public class EvalVisitor : SinumerikBaseVisitor<SLValue>
    {
        private StringBuilder log = new StringBuilder();
        public StringBuilder GcodeBuffer { get; private set; }
        private static ReturnValue returnValue = new ReturnValue();
        private Scope scope;
        private Dictionary<String, Function> functions;
        private Dictionary<String, int> NumberedLabel { get; set; }
        private Dictionary<String, int> numberStatement;
        private StatementContext mainStatement;
        private StatementContext currentStatement;
        private BlockContext mainBlock;
        private List<BlockContext> listBlocks;
        private ISet<String> lastToken;
        private ISet<String> icacMode;
        private int nextStatement;
        private int ifStatementNumber;
        private int countStatement;


        public EvalVisitor(Scope scope, Dictionary<String, Function> functions, StringBuilder gcodeBuffer)
        {
            this.scope = scope;
            this.functions = functions;
            GcodeBuffer = gcodeBuffer;
            NumberedLabel = new Dictionary<String, int>();
            numberStatement = new Dictionary<String, int>();
            listBlocks = new List<BlockContext>();
            lastToken = new HashSet<String>();
            icacMode = new HashSet<String>();
        }

      
        void RememberLineLabels(StatementContext[] statements)
        {
            countStatement = -1;
            for (int i = 0; i < statements.Length; ++i)
            {
                mainStatement = statements[i];
                string statName = mainStatement.GetText();

                if (mainStatement.metkaStart() != null)
                {
                    NumberedLabel[mainStatement.metkaStart().GetText()] = i; 
                }

                if(mainStatement.ifStatement() != null)
                {
                    ifStatementNumber = i;
                }

              //  numberStatement[mainStatement.GetText()] = i;
                countStatement = i;
            }
        }

        public override SLValue VisitParse(ParseContext ctx)
        {
//            RememberLineLabels(ctx.block().statement());
            mainBlock = ctx.block();
            listBlocks.Add(mainBlock);

            VisitBlock(ctx.block());

            return new SLValue(false);
        }


        // functionDecl
        public override SLValue VisitFunctionDecl(FunctionDeclContext ctx)
        {
            return SLValue.VOID;
        }

        // list: '[' exprList? ']'
        public override SLValue VisitList(ListContext ctx)
        {
            List<SLValue> list = new List<SLValue>();
            if (ctx.exprList() != null)
            {
                foreach(ExpressionContext ex in ctx.exprList().expression())
                {
                    list.Add(this.Visit(ex));
                }
            }
            return new SLValue(list);
        }

        // '-' expression                           #unaryMinusExpression
        public override SLValue VisitUnaryMinusExpression(UnaryMinusExpressionContext ctx)
        {
            SLValue v = this.Visit(ctx.expression());
            if (!v.isNumber())
            {
                throw new EvalException(ctx);
            }
            return new SLValue(-1 * v.asDouble());
        }

        // '!' expression                           #notExpression
        public override SLValue VisitNotExpression(NotExpressionContext ctx)
        {
            SLValue v = this.Visit(ctx.expression());
            if (!v.isBoolean())
            {
                throw new EvalException(ctx);
            }
            return new SLValue(!v.asBoolean());
        }

        // expression '^' expression                #powerExpression
        public override SLValue VisitPowerExpression(PowerExpressionContext ctx)
        {
            SLValue lhs = this.Visit(ctx.expression(0));
            SLValue rhs = this.Visit(ctx.expression(1));
            if (lhs.isNumber() && rhs.isNumber())
            {
                return new SLValue(Math.Pow(lhs.asDouble(), rhs.asDouble()));
            }
            throw new EvalException(ctx);
        }

        // expression op=( '*' | '/' | '%' ) expression         #multExpression
        public override SLValue VisitMultExpression(MultExpressionContext ctx)
        {
            switch (ctx.op.Type)
            {
                case SinumerikLexer.Multiply:
                    return Multiply(ctx);
                case SinumerikLexer.Divide:
                    return Divide(ctx);
                case SinumerikLexer.Modulus:
                    return Modulus(ctx);
                default:
                    throw new SystemException("unknown operator type: " + ctx.op.Type);
            }
        }

        // expression op=( '+' | '-' ) expression               #addExpression
        public override SLValue VisitAddExpression(AddExpressionContext ctx)
        {
            switch (ctx.op.Type)
            {
                case SinumerikLexer.Add:
                    return Add(ctx);
                case SinumerikLexer.Subtract:
                    return Subtract(ctx);
                default:
                    throw new SystemException("unknown operator type: " + ctx.op.Type);
            }
        }

        // expression op=( '>=' | '<=' | '>' | '<' ) expression #compExpression
        public override SLValue VisitCompExpression(CompExpressionContext ctx)
        {
            switch (ctx.op.Type)
            {
                case SinumerikLexer.LT:
                    return Lt(ctx);
                case SinumerikLexer.LTEquals:
                    return LtEq(ctx);
                case SinumerikLexer.GT:
                    return Gt(ctx);
                case SinumerikLexer.GTEquals:
                    return GtEq(ctx);
                default:
                    throw new SystemException("unknown operator type: " + ctx.op.Type);
            }
        }

        // expression op=( '==' | '<>' ) expression             #eqExpression
        public override SLValue VisitEqExpression(EqExpressionContext ctx)
        {
            switch (ctx.op.Type)
            {
                case SinumerikLexer.Equals:
                    return Eq(ctx);
                case SinumerikLexer.NEquals:
                    return NEq(ctx);
                default:
                    throw new SystemException("unknown operator type: " + ctx.op.Type);
            }
        }

        public SLValue Multiply(MultExpressionContext ctx)
        {
            SLValue lhs = this.Visit(ctx.expression(0));
            SLValue rhs = this.Visit(ctx.expression(1));
            if (lhs == null || rhs == null)
            {
                log.Append("lhs " + lhs + " rhs " + rhs);
                throw new EvalException(ctx);
            }

            // number * number
            if (lhs.isNumber() && rhs.isNumber())
            {
                return new SLValue(lhs.asDouble() * rhs.asDouble());
            }

            // string * number
            if (lhs.isString() && rhs.isNumber())
            {
                StringBuilder str = new StringBuilder();
                int stop = (int)rhs.asDouble();
                for (int i = 0; i < stop; i++)
                {
                    str.Append(lhs.asString());
                }
                return new SLValue(str.ToString());
            }

            // list * number
            if (lhs.isList() && rhs.isNumber())
            {
                List<SLValue> total = new List<SLValue>();
                int stop = (int)rhs.asDouble();
                for (int i = 0; i < stop; i++)
                {
                    total.AddRange(lhs.asList());
                }
                return new SLValue(total);
            }

            throw new EvalException(ctx);
        }

        private SLValue Divide(MultExpressionContext ctx)
        {
            SLValue lhs = this.Visit(ctx.expression(0));
            SLValue rhs = this.Visit(ctx.expression(1));
            if (lhs.isNumber() && rhs.isNumber())
            {
                return new SLValue(lhs.asDouble() / rhs.asDouble());
            }
            throw new EvalException(ctx);
        }

        private SLValue Modulus(MultExpressionContext ctx)
        {
            SLValue lhs = this.Visit(ctx.expression(0));
            SLValue rhs = this.Visit(ctx.expression(1));
            if (lhs.isNumber() && rhs.isNumber())
            {
                return new SLValue(lhs.asDouble() % rhs.asDouble());
            }
            throw new EvalException(ctx);
        }

        private SLValue Add(AddExpressionContext ctx)
        {
            SLValue lhs = this.Visit(ctx.expression(0));
            SLValue rhs = this.Visit(ctx.expression(1));

            if (lhs == null || rhs == null)
            {
                throw new EvalException(ctx);
            }

            // number + number
            if (lhs.isNumber() && rhs.isNumber())
            {
                return new SLValue(lhs.asDouble() + rhs.asDouble());
            }

            // list + any
            if (lhs.isList())
            {
                List<SLValue> list = lhs.asList();
                list.Add(rhs);
                return new SLValue(list);
            }

            // string + any
            if (lhs.isString())
            {
                return new SLValue(lhs.asString() + "" + rhs.ToString());
            }

            // any + string
            if (rhs.isString())
            {
                return new SLValue(lhs.ToString() + "" + rhs.asString());
            }

            return new SLValue(lhs.ToString() + rhs.ToString());
        }

        private SLValue Subtract(AddExpressionContext ctx)
        {
            SLValue lhs = this.Visit(ctx.expression(0));
            SLValue rhs = this.Visit(ctx.expression(1));
            if (lhs.isNumber() && rhs.isNumber())
            {
                return new SLValue(lhs.asDouble() - rhs.asDouble());
            }
            if (lhs.isList())
            {
                List<SLValue> list = lhs.asList();
                list.Remove(rhs);
                return new SLValue(list);
            }
            throw new EvalException(ctx);
        }

        private SLValue GtEq(CompExpressionContext ctx)
        {
            SLValue lhs = this.Visit(ctx.expression(0));
            SLValue rhs = this.Visit(ctx.expression(1));
            if (lhs.isNumber() && rhs.isNumber())
            {
                return new SLValue(lhs.asDouble() >= rhs.asDouble());
            }
            if (lhs.isString() && rhs.isString())
            {
                return new SLValue(lhs.asString().CompareTo(rhs.asString()) >= 0);
            }
            throw new EvalException(ctx);
        }

        private SLValue LtEq(CompExpressionContext ctx)
        {
            SLValue lhs = this.Visit(ctx.expression(0));
            SLValue rhs = this.Visit(ctx.expression(1));
            if (lhs.isNumber() && rhs.isNumber())
            {
                return new SLValue(lhs.asDouble() <= rhs.asDouble());
            }
            if (lhs.isString() && rhs.isString())
            {
                return new SLValue(lhs.asString().CompareTo(rhs.asString()) <= 0);
            }
            throw new EvalException(ctx);
        }

        private SLValue Gt(CompExpressionContext ctx)
        {
            SLValue lhs = this.Visit(ctx.expression(0));
            SLValue rhs = this.Visit(ctx.expression(1));
            if (lhs.isNumber() && rhs.isNumber())
            {
                return new SLValue(lhs.asDouble() > rhs.asDouble());
            }
            if (lhs.isString() && rhs.isString())
            {
                return new SLValue(lhs.asString().CompareTo(rhs.asString()) > 0);
            }
            throw new EvalException(ctx);
        }

        private SLValue Lt(CompExpressionContext ctx)
        {
            SLValue lhs = this.Visit(ctx.expression(0));
            SLValue rhs = this.Visit(ctx.expression(1));
            if (lhs.isNumber() && rhs.isNumber())
            {
                return new SLValue(lhs.asDouble() < rhs.asDouble());
            }
            if (lhs.isString() && rhs.isString())
            {
                return new SLValue(lhs.asString().CompareTo(rhs.asString()) < 0);
            }
            throw new EvalException(ctx);
        }

        private SLValue Eq(EqExpressionContext ctx)
        {
            SLValue lhs = this.Visit(ctx.expression(0));
            SLValue rhs = this.Visit(ctx.expression(1));

            if (lhs == null)
            {
                throw new EvalException(ctx);
            }

            return new SLValue(lhs.Equals(rhs));
        }

        private SLValue NEq(EqExpressionContext ctx)
        {
            SLValue lhs = this.Visit(ctx.expression(0));
            SLValue rhs = this.Visit(ctx.expression(1));
            return new SLValue(!lhs.Equals(rhs));
        }

        // expression '&&' expression               #andExpression
        public override SLValue VisitAndExpression(AndExpressionContext ctx)
        {
            SLValue lhs = this.Visit(ctx.expression(0));
            SLValue rhs = this.Visit(ctx.expression(1));

            if (!lhs.isBoolean() || !rhs.isBoolean())
            {
                throw new EvalException(ctx);
            }
            return new SLValue(lhs.asBoolean() && rhs.asBoolean());
        }

        // expression '||' expression               #orExpression
        public override SLValue VisitOrExpression(OrExpressionContext ctx)
        {
            SLValue lhs = this.Visit(ctx.expression(0));
            SLValue rhs = this.Visit(ctx.expression(1));

            if (!lhs.isBoolean() || !rhs.isBoolean())
            {
                throw new EvalException(ctx);
            }
            return new SLValue(lhs.asBoolean() || rhs.asBoolean());
        }

        // expression MOD expression                #modExpression
        public override SLValue VisitModExpression(ModExpressionContext ctx)
        {
            SLValue lhs = this.Visit(ctx.expression(0));
            SLValue rhs = this.Visit(ctx.expression(1));

            if(!lhs.IsNumber() || !rhs.IsNumber())
            {
                throw new EvalException(ctx);
            }

            return new SLValue(Math.IEEERemainder(lhs.asDouble(), rhs.asDouble()));
        }

        // expression DIV expression                #divExpression
        public override SLValue VisitDivExpression(DivExpressionContext ctx)
        {
            SLValue lhs = this.Visit(ctx.expression(0));
            SLValue rhs = this.Visit(ctx.expression(1));

            if (!lhs.IsNumber() || !rhs.IsNumber())
            {
                throw new EvalException(ctx);
            }

            return new SLValue(Math.Truncate(lhs.asDouble()/rhs.asDouble()));
        }


        // Number                                   #numberExpression
        public override SLValue VisitNumberExpression(NumberExpressionContext ctx)
        {
            return new SLValue(Convert.ToDouble(ctx.GetText().Replace('.',',')));
        }

        // Bool                                     #boolExpression
        public override SLValue VisitBoolExpression(BoolExpressionContext ctx)
        {
            return new SLValue(Convert.ToBoolean(ctx.GetText()));
        }

        // Null                                     #nullExpression
        public override SLValue VisitNullExpression(NullExpressionContext ctx)
        {
            return SLValue.NULL;
        }

        private SLValue resolveIndexes(SLValue val, List<ExpressionContext> indexes)
        {
            foreach(ExpressionContext ec in indexes)
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

        private void SetAtIndex(ParserRuleContext ctx, List<ExpressionContext> indexes, SLValue val, SLValue newVal)
        {
            if (!val.isList())
            {
                throw new EvalException(ctx);
            }
            for (int i = 0; i < indexes.Count - 1; i++)
            {
                SLValue idx = this.Visit(indexes[i]);
                if (!idx.isNumber())
                {
                    throw new EvalException(ctx);
                }
                val = val.asList()[((int)idx.asDouble())];
            }
            SLValue _idx = this.Visit(indexes[(indexes.Count - 1)]);
            if (!_idx.isNumber())
            {
                throw new EvalException(ctx);
            }
            val.asList()[(int)_idx.asDouble()] = newVal;
        }

        // functionCall indexes?                    #functionCallExpression
        public override SLValue VisitFunctionCallExpression(FunctionCallExpressionContext ctx)
        {
            SLValue val = this.Visit(ctx.functionCall());
            if (ctx.indexes() != null)
            {
                List<ExpressionContext> exps = ctx.indexes().expression().ToList(); 
                val = resolveIndexes(val, exps);
            }
            return val;
        }

        // list indexes?                            #listExpression
        public override SLValue VisitListExpression(ListExpressionContext ctx)
        {
            SLValue val = this.Visit(ctx.list());
            if (ctx.indexes() != null)
            {
                List<ExpressionContext> exps = ctx.indexes().expression().ToList();
                val = resolveIndexes(val, exps);
            }
            return val;
        }

        // Identifier indexes?                      #identifierExpression
        public override SLValue VisitIdentifierExpression(IdentifierExpressionContext ctx)
        {
            String id = ctx.Identifier().GetText();
            SLValue val = scope.resolve(id);

            if (ctx.indexes() != null)
            {
                List<ExpressionContext> exps = ctx.indexes().expression().ToList();
                val = resolveIndexes(val, exps);
            }
            return val;
        }

        // String indexes?                          #stringExpression
        public override SLValue VisitStringExpression(StringExpressionContext ctx)
        {
            String text = ctx.GetText();
            text = text.Substring(1, text.Length - 1).Replace("\\\\(.)", "$1");
            SLValue val = new SLValue(text);
            if (ctx.indexes() != null)
            {
                List<ExpressionContext> exps = ctx.indexes().expression().ToList();
                val = resolveIndexes(val, exps);
            }
            return val;
        }

        // '(' expression ')' indexes?              #expressionExpression
        public override SLValue VisitExpressionExpression(ExpressionExpressionContext ctx)
        {
            SLValue val = this.Visit(ctx.expression());
            if (ctx.indexes() != null)
            {
                List<ExpressionContext> exps = ctx.indexes().expression().ToList();
                val = resolveIndexes(val, exps);
            }
            return val;
        }

        // Input '(' String? ')'                    #inputExpression
        public override SLValue VisitInputExpression(InputExpressionContext ctx)
        {
            ITerminalNode inputString = ctx.String();
            try
            {
                if (inputString != null)
                {
                    String text = inputString.GetText();
                    text = text.Substring(1, text.Length - 1).Replace("\\\\(.)", "$1");
                    byte[] array = File.ReadAllBytes(text);
                    return new SLValue(Encoding.UTF8.GetString(array));
                }
                else
                {
                    using (StreamReader buffer = new StreamReader(Console.ReadLine()))
                    {
                        return new SLValue(buffer.ReadLine());
                    }
                }
            }
            catch (IOException e)
            {
                throw new SystemException(e.Message);
            }
        }

        // assignment
        // : Identifier indexes? '=' expression
        public override SLValue VisitAssignment(AssignmentContext ctx)
        {

            SLValue newVal = this.Visit(ctx.expression());

            if (ctx.indexes() != null)
            {
                SLValue val = scope.resolve(ctx.Identifier().GetText());
                List<ExpressionContext> exps = ctx.indexes().expression().ToList();
                SetAtIndex(ctx, exps, val, newVal);
            }
            else
            {
                String id = ctx.Identifier().GetText();
                scope.assign(id, newVal);
            }
            return SLValue.VOID;
        }

        // Identifier '(' exprList? ')' #identifierFunctionCall
        public override SLValue VisitIdentifierFunctionCall(IdentifierFunctionCallContext ctx)
        {
            List<ExpressionContext> param = ctx.exprList() != null ? ctx.exprList().expression().ToList() : new ArrayList<ExpressionContext>();
            String id = ctx.Identifier().GetText() + param.Count;
            Function function;
            if ((function = functions[id]) != null)
            {
                return function.invoke(param, functions, scope, GcodeBuffer);
            }
            throw new EvalException(ctx);
        }

        // Println '(' expression? ')'  #printlnFunctionCall
        public override SLValue VisitPrintlnFunctionCall(PrintlnFunctionCallContext ctx)
        {
            Console.WriteLine(this.Visit(ctx.expression()));
            return SLValue.VOID;
        }

        // Print '(' expression ')'     #printFunctionCall
        public override SLValue VisitPrintFunctionCall(PrintFunctionCallContext ctx)
        {
            Console.WriteLine(this.Visit(ctx.expression()));
            return SLValue.VOID;
        }

        // Assert '(' expression ')'    #assertFunctionCall
        public override SLValue VisitAssertFunctionCall(AssertFunctionCallContext ctx)
        {
            SLValue value = this.Visit(ctx.expression());

            if (!value.isBoolean())
            {
                throw new EvalException(ctx);
            }

            if (!value.asBoolean())
            {
                throw new InvalidOperationException("Failed Assertion " + ctx.expression().GetText() + " line:" + ctx.Start.Line);
            }

            return SLValue.VOID;
        }

        // Sin '(' expression ')' #sinFunctionCall
        public override SLValue VisitSinFunctionCall(SinFunctionCallContext ctx)
        {
            SLValue value = this.Visit(ctx.expression());

            if (value.isNumber())
            {
                return new SLValue(Math.Sin(value.asDouble()*Math.PI/180));
            }
            else
            {
                throw new EvalException(ctx);
            }
        }

        // Cos '(' expression ')' #cosFunctionCall
        public override SLValue VisitCosFunctionCall(CosFunctionCallContext ctx)
        {
            SLValue value = this.Visit(ctx.expression());

            if (value.isNumber())
            {
                return new SLValue(Math.Cos(value.asDouble() * Math.PI / 180));
            }
            else
            {
                throw new EvalException(ctx);
            }
        }

        // Tan '(' expression ')' #tanFunctionCall
        public override SLValue VisitTanFunctionCall(TanFunctionCallContext ctx)
        {
            SLValue value = this.Visit(ctx.expression());

            if (value.isNumber())
            {
                return new SLValue(Math.Tan(value.asDouble() * Math.PI / 180));
            }
            else
            {
                throw new EvalException(ctx);
            }
        }

        // Asin '(' expression ')' #asinFunctionCall
        public override SLValue VisitAsinFunctionCall(AsinFunctionCallContext ctx)
        {
            SLValue value = this.Visit(ctx.expression());

            if (value.isNumber())
            {
                return new SLValue(Math.Asin(value.asDouble() * Math.PI / 180));
            }
            else
            {
                throw new EvalException(ctx);
            }
        }

        // ACos '(' expression ')' #acosFunctionCall
        public override SLValue VisitAcosFunctionCall(AcosFunctionCallContext ctx)
        {
            SLValue value = this.Visit(ctx.expression());

            if (value.isNumber())
            {
                return new SLValue(Math.Acos(value.asDouble() * Math.PI / 180));
            }
            else
            {
                throw new EvalException(ctx);
            }
        }

        // Atan '(' expression ')' #atanFunctionCall
        public override SLValue VisitAtanFunctionCall(AtanFunctionCallContext ctx)
        {
            SLValue value = this.Visit(ctx.expression());

            if (value.isNumber())
            {
                return new SLValue(Math.Atan(value.asDouble() * Math.PI / 180));
            }
            else
            {
                throw new EvalException(ctx);
            }
        }

        // Atan2 '(' expression ',' expression ')' #atan2FunctionCall
        public override SLValue VisitAtan2FunctionCall(Atan2FunctionCallContext ctx)
        {
            SLValue value1 = this.Visit(ctx.expression()[0]);
            SLValue value2 = this.Visit(ctx.expression()[1]);

            if (value1.isNumber() && value2.isNumber())
            {
                return new SLValue(Math.Atan2((value1.asDouble() * Math.PI / 180), (value2.asDouble()* Math.PI / 180)));
            }
            else
            {
                throw new EvalException(ctx);
            }
        }

        // Abs '(' expression ')' #absFunctionCall
        public override SLValue VisitAbsFunctionCall(AbsFunctionCallContext ctx)
        {
            SLValue value = this.Visit(ctx.expression());

            if (value.isNumber())
            {
                return new SLValue(Math.Abs(value.asDouble()));
            }
            else
            {
                throw new EvalException(ctx);
            }
        }

        // Round '(' expression ')' #roundFunctionCall
        public override SLValue VisitRoundFunctionCall(RoundFunctionCallContext ctx)
        {
            SLValue value = this.Visit(ctx.expression());

            if (value.isNumber())
            {
                return new SLValue(Math.Round(value.asDouble()));
            }
            else
            {
                throw new EvalException(ctx);
            }
        }

        // ModeAC '(' expression ')' #modeacFunctionCall
        public override SLValue VisitModeacFunctionCall(ModeacFunctionCallContext ctx)
        {
            SLValue value = this.Visit(ctx.expression());
            icacMode.Clear();

            if (value.isNumber())
            {
                icacMode.Add("AC");
                return new SLValue(value.asDouble());
            }
            else
            {
                throw new EvalException(ctx);
            }
        }

        // ModeIC '(' expression ')' #modeicFunctionCall
        public override SLValue VisitModeicFunctionCall(ModeicFunctionCallContext ctx)
        {
            SLValue value = this.Visit(ctx.expression());
            icacMode.Clear();

            if (value.isNumber())
            {
                icacMode.Add("IC");
                return new SLValue(value.asDouble());
            }
            else
            {
                throw new EvalException(ctx);
            }
        }

        // Sqrt '(' expression ')' #sqrtFunctionCall
        public override SLValue VisitSqrtFunctionCall(SqrtFunctionCallContext ctx)
        {
            SLValue value = this.Visit(ctx.expression());

            if (value.isNumber())
            {
                return new SLValue(Math.Sqrt(value.asDouble()));
            }
            else
            {
                throw new EvalException(ctx);
            }
        }

        // Pot '(' expression ')' #potFunctionCall

        public override SLValue VisitPotFunctionCall(PotFunctionCallContext ctx)
        {
            SLValue value = this.Visit(ctx.expression());

            if (value.isNumber())
            {
                return new SLValue(Math.Pow(value.asDouble(), 2));
            }
            else
            {
                throw new EvalException(ctx);
            }
        }


        // Trunc '(' expression ')' #truncFunctionCall
        public override SLValue VisitTruncFunctionCall(TruncFunctionCallContext ctx)
        {
            SLValue value = this.Visit(ctx.expression());

            if (value.isNumber())
            {
                return new SLValue(Math.Truncate(value.asDouble()));
            }
            else
            {
                throw new EvalException(ctx);
            }
        }

        // Size '(' expression ')'      #sizeFunctionCall
        public override SLValue VisitSizeFunctionCall(SizeFunctionCallContext ctx)
        {
            SLValue value = this.Visit(ctx.expression());

            if (value.isString())
            {
                return new SLValue(value.asString().Length);
            }

            if (value.isList())
            {
                return new SLValue(value.asList().Count);
            }

            throw new EvalException(ctx);
        }

        // ifStatement
        public override SLValue VisitIfStatement(IfStatementContext ctx)
        {

            // if ...
            if (this.Visit(ctx.ifStat().expression()).asBoolean())
            {
                for(int i=0; i < ctx.ifStat().statement().Length; i++)
                {
                    currentStatement = ctx.ifStat().statement()[i];
                    if (currentStatement.gotoStatement() != null)
                        return this.Visit(currentStatement); 

                    this.Visit(ctx.ifStat().statement()[i]);
                } 
            }
             
            // else ...
            if (ctx.elseStat() != null)
            {
                return this.Visit(ctx.elseStat());
            }

            return SLValue.VOID;
        }

        public override SLValue VisitIfGotostat(IfGotostatContext ctx)
        {
            string destination = ctx.metkaDest().GetText() + ":";

            if (this.Visit(ctx.expression()).asBoolean())
            {
                if (!string.IsNullOrEmpty(destination))
                {
                    foreach (var key in NumberedLabel.Keys)
                    {
                        if (key.Equals(destination))
                            nextStatement = NumberedLabel[key];
                    }
                }
            }
                
            return SLValue.VOID;
        }

        // block
        // : (statement | functionDecl)*
        // ;
        public override SLValue VisitBlock(BlockContext ctx)
        {
            RememberLineLabels(ctx.statement());

            scope = new Scope(scope); // create new local scope
            nextStatement = -1;

            for (int i=0; i<ctx.statement().Length; i++)
            {                
                if (nextStatement >= 0)
                {
                    i = nextStatement;
                    nextStatement = -1;

                  //  this.Visit(ctx.statement()[i]);
                }

                this.Visit(ctx.statement()[i]);
            }

            scope = scope.parent();
            return SLValue.VOID;
        }

        // forStatement
        // : For Identifier '=' expression To expression OBrace block CBrace
        // ;
        public override SLValue VisitForStatement(ForStatementContext ctx)
        {
            int start = (int)this.Visit(ctx.expression(0)).asDouble();
            int stop = (int)this.Visit(ctx.expression(1)).asDouble();
            for (int i = start; i <= stop; i++)
            {
                scope.assign(ctx.Identifier().GetText(), new SLValue(i));
                SLValue returnValue = this.Visit(ctx.block());
                if (returnValue != SLValue.VOID)
                {
                    return returnValue;
                }
            }
            return SLValue.VOID;
        }

        // whileStatement
        public override SLValue VisitWhileStatement(WhileStatementContext ctx)
        {
            while (this.Visit(ctx.expression()).asBoolean())
            {
                SLValue returnValue = this.Visit(ctx.block());
                if (returnValue != SLValue.VOID)
                {
                    return returnValue;
                }
            }
            return SLValue.VOID;
        }

        // gotoStatement
        public override SLValue VisitGotoStatement(GotoStatementContext ctx)
        {
            string destination = ctx.Identifier().GetText() +  ":";

            if (!string.IsNullOrEmpty(destination))
            {
                  foreach (var key in NumberedLabel.Keys)
                  {
                        if (key.Equals(destination))
                            nextStatement = NumberedLabel[key];
                  }
                    
            }

       //       return this.Visit(ctx.Identifier());
                return SLValue.VOID;
        }

        public override SLValue VisitRotFunctionCall(RotFunctionCallContext ctx)
        {
            String id = ctx.Rot().GetText();

            lastToken.Clear();
            GcodeBuffer.Append(id + " ");

            return SLValue.VOID;
        }

        public override SLValue VisitArotFunctionCall(ArotFunctionCallContext ctx)
        {
            String id = ctx.Arot().GetText();

            lastToken.Clear();
            GcodeBuffer.Append(id + " ");

            return SLValue.VOID;
        }

        public override SLValue VisitTransFunctionCall(TransFunctionCallContext ctx)
        {
            String id = ctx.Trans().GetText();

            lastToken.Clear();
            GcodeBuffer.Append(id + " ");

            return SLValue.VOID;
        }

        public override SLValue VisitAtransFunctionCall(AtransFunctionCallContext ctx)
        {
            String id = ctx.Atrans().GetText();

            lastToken.Clear();
            GcodeBuffer.Append(id + " ");

            return SLValue.VOID;
        }

        public override SLValue VisitMirrorFunctionCall(MirrorFunctionCallContext ctx)
        {
            String id = ctx.Mirror().GetText();

            lastToken.Clear();
            GcodeBuffer.Append(id + " ");

            return SLValue.VOID;
        }

        public override SLValue VisitAmirrorFunctionCall(AmirrorFunctionCallContext ctx)
        {
            String id = ctx.Amirror().GetText();

            lastToken.Clear();
            GcodeBuffer.Append(id + " ");

            return SLValue.VOID;
        }

        public override SLValue VisitScaleFunctionCall(ScaleFunctionCallContext ctx)
        {
            String id = ctx.Scale().GetText();

            lastToken.Clear();
            GcodeBuffer.Append(id + " ");

            return SLValue.VOID;
        }

        public override SLValue VisitAscaleFunctionCall(AscaleFunctionCallContext ctx)
        {
            String id = ctx.AScale().GetText();

            lastToken.Clear();
            GcodeBuffer.Append(id + " ");

            return SLValue.VOID;
        }

        public override SLValue VisitDiamonFunctionCall(DiamonFunctionCallContext ctx)
        {
            String id = ctx.Diamon().GetText();

            lastToken.Clear();
            GcodeBuffer.Append(id + " ");

            return SLValue.VOID;
        }

        public override SLValue VisitDiamofFunctionCall(DiamofFunctionCallContext ctx)
        {
            String id = ctx.Diamof().GetText();

            lastToken.Clear();
            GcodeBuffer.Append(id + " ");

            return SLValue.VOID;
        }

        public override SLValue VisitDiam90FunctionCall(Diam90FunctionCallContext ctx)
        {
            String id = ctx.Diam90().GetText();

            lastToken.Clear();
            GcodeBuffer.Append(id + " ");

            return SLValue.VOID;
        }

        public override SLValue VisitXcoordFunctionCall(XcoordFunctionCallContext ctx)
        {
            SLValue value = this.Visit(ctx.expression());

            lastToken.Clear();
            string formatValue;

            formatValue = string.Format("{0:f9}", value.asDouble()).Replace(',', '.');
            if (icacMode.Count > 0)
                GcodeBuffer.Append("X=" + icacMode.First() + "(" + formatValue + ")");
            else
                GcodeBuffer.Append("X" + formatValue);

            icacMode.Clear();
            return SLValue.VOID;

        }

        public override SLValue VisitYcoordFunctionCall(YcoordFunctionCallContext ctx)
        {
            SLValue value = this.Visit(ctx.expression());

            lastToken.Clear();
            string formatValue;

            formatValue = string.Format("{0:f9}", value.asDouble()).Replace(',', '.');
            if (icacMode.Count > 0)
                GcodeBuffer.Append("Y=" + icacMode.First() + "(" + formatValue + ")");
            else
                GcodeBuffer.Append("Y" + formatValue);

            icacMode.Clear();
            return SLValue.VOID;

        }

        public override SLValue VisitZcoordFunctionCall(ZcoordFunctionCallContext ctx)
        {
            SLValue value = this.Visit(ctx.expression());
            
            lastToken.Clear();
            string formatValue;

            formatValue = string.Format("{0:f9}",value.asDouble()).Replace(',','.');
            if (icacMode.Count > 0)
                GcodeBuffer.Append("Z=" + icacMode.First() + "(" + formatValue + ")");
            else
                GcodeBuffer.Append("Z" + formatValue);

            icacMode.Clear();
            return SLValue.VOID;

        }

        public override SLValue VisitAcoordFunctionCall (AcoordFunctionCallContext ctx)
        {
            SLValue value = this.Visit(ctx.expression());

            lastToken.Clear();
            string formatValue;

            formatValue = string.Format("{0:f9}", value.asDouble()).Replace(',', '.');
            if (icacMode.Count > 0)
                GcodeBuffer.Append("A=" + icacMode.First() + "(" + formatValue + ")");
            else
                GcodeBuffer.Append("A" + formatValue);


            icacMode.Clear();
            return SLValue.VOID;

        }

        public override SLValue VisitBcoordFunctionCall(BcoordFunctionCallContext ctx)
        {
            SLValue value = this.Visit(ctx.expression());

            lastToken.Clear();
            string formatValue;

            formatValue = string.Format("{0:f9}", value.asDouble()).Replace(',', '.');
            if (icacMode.Count > 0)
                GcodeBuffer.Append("B=" + icacMode.First() + "(" + formatValue + ")");
            else
                GcodeBuffer.Append("B" + formatValue);


            icacMode.Clear();
            return SLValue.VOID;

        }

        public override SLValue VisitCcoordFunctionCall(CcoordFunctionCallContext ctx)
        {
            SLValue value = this.Visit(ctx.expression());

            lastToken.Clear();
            string formatValue;

            formatValue = string.Format("{0:f9}", value.asDouble()).Replace(',', '.');
            if (icacMode.Count > 0)
                GcodeBuffer.Append("C=" + icacMode.First() + "(" + formatValue + ")");
            else
                GcodeBuffer.Append("C" + formatValue);


            icacMode.Clear();
            return SLValue.VOID;

        }

        public override SLValue VisitUcoordFunctionCall(UcoordFunctionCallContext ctx)
        {
            SLValue value = this.Visit(ctx.expression());

            lastToken.Clear();
            string formatValue;

            formatValue = string.Format("{0:f9}", value.asDouble()).Replace(',', '.');
            if (icacMode.Count > 0)
                GcodeBuffer.Append("U=" + icacMode.First() + "(" + formatValue + ")");
            else
                GcodeBuffer.Append("U" + formatValue);


            icacMode.Clear();
            return SLValue.VOID;

        }

        public override SLValue VisitVcoordFunctionCall(VcoordFunctionCallContext ctx)
        {
            SLValue value = this.Visit(ctx.expression());

            lastToken.Clear();
            string formatValue;

            formatValue = string.Format("{0:f9}", value.asDouble()).Replace(',', '.');
            if (icacMode.Count > 0)
                GcodeBuffer.Append("V=" + icacMode.First() + "(" + formatValue + ")");
            else
                GcodeBuffer.Append("V" + formatValue);


            icacMode.Clear();
            return SLValue.VOID;

        }

        public override SLValue VisitWcoordFunctionCall(WcoordFunctionCallContext ctx)
        {
            SLValue value = this.Visit(ctx.expression());

            lastToken.Clear();
            string formatValue;

            formatValue = string.Format("{0:f9}", value.asDouble()).Replace(',', '.');
            if (icacMode.Count > 0)
                GcodeBuffer.Append("W=" + icacMode.First() + "(" + formatValue + ")");
            else
                GcodeBuffer.Append("W" + formatValue);


            icacMode.Clear();
            return SLValue.VOID;
        }

        public override SLValue VisitIvectFunctionCall(IvectFunctionCallContext ctx)
        {
            SLValue value = this.Visit(ctx.expression());

            lastToken.Clear();
            string formatValue;

            formatValue = string.Format("{0:f9}", value.asDouble()).Replace(',', '.');
            if (icacMode.Count > 0)
                GcodeBuffer.Append("I=" + icacMode.First() + "(" + formatValue + ")");
            else
                GcodeBuffer.Append("I" + formatValue);


            icacMode.Clear();
            return SLValue.VOID;
        }

        public override SLValue VisitJvectFunctionCall(JvectFunctionCallContext ctx)
        {
            SLValue value = this.Visit(ctx.expression());

            lastToken.Clear();
            string formatValue;

            formatValue = string.Format("{0:f9}", value.asDouble()).Replace(',', '.');
            if (icacMode.Count > 0)
                GcodeBuffer.Append("J=" + icacMode.First() + "(" + formatValue + ")");
            else
                GcodeBuffer.Append("J" + formatValue);


            icacMode.Clear();
            return SLValue.VOID;
        }

        public override SLValue VisitKvectFunctionCall(KvectFunctionCallContext ctx)
        {
            SLValue value = this.Visit(ctx.expression());

            lastToken.Clear();
            string formatValue;

            formatValue = string.Format("{0:f9}", value.asDouble()).Replace(',', '.');
            if (icacMode.Count > 0)
                GcodeBuffer.Append("K=" + icacMode.First() + "(" + formatValue + ")");
            else
                GcodeBuffer.Append("K" + formatValue);


            icacMode.Clear();
            return SLValue.VOID;
        }

        public override SLValue VisitRadiusFunctionCall(RadiusFunctionCallContext ctx)
        {
            SLValue value = this.Visit(ctx.expression());

            string formatValue;

            formatValue = string.Format("{0:f9}", value.asDouble()).Replace(',', '.');
            
            GcodeBuffer.Append("CR=" + formatValue);


            return SLValue.VOID;

        }

        public override SLValue VisitGcodeStatement(GcodeStatementContext ctx)
        {
            lastToken.Clear();
            GcodeBuffer.Append(ctx.GetText());

            return SLValue.VOID;
        }

        public override SLValue VisitMmodeFunctionCall(MmodeFunctionCallContext ctx)
        {
            String id = ctx.MFunc().GetText();

            lastToken.Clear();
            GcodeBuffer.Append(id);

            return SLValue.VOID;
        }

        public override SLValue VisitToolNumberFunctionCall(ToolNumberFunctionCallContext ctx)
        {
            String id = ctx.TFunc().GetText();

            lastToken.Clear();
            GcodeBuffer.Append(id);

            return SLValue.VOID;
        }

        public override SLValue VisitToolIDFunctionCall(ToolIDFunctionCallContext ctx)
        {
            String id = ctx.DFunc().GetText();

            lastToken.Clear();
            GcodeBuffer.Append(id);

            return SLValue.VOID;
        }

        public override SLValue VisitSpeedFunctionCall(SpeedFunctionCallContext ctx)
        {
            SLValue value = this.Visit(ctx.expression());

            lastToken.Clear();
            string formatValue;

            formatValue = string.Format("{0:f9}", value.asDouble()).Replace(',', '.');
            
            GcodeBuffer.Append("S" + formatValue);
            return SLValue.VOID;
        }

        public override SLValue VisitFeedFunctionCall(FeedFunctionCallContext ctx)
        {
            SLValue value = this.Visit(ctx.expression());

            lastToken.Clear();
            string formatValue;

            formatValue = string.Format("{0:f9}", value.asDouble()).Replace(',', '.');
            GcodeBuffer.Append("F" + formatValue);
            return SLValue.VOID;
        }

        public override SLValue VisitCrlfStatement(CrlfStatementContext ctx)
        {
            string token = ctx.CR().GetText();
            
            if (lastToken.Contains(token))
            {
                return SLValue.VOID;
            }

            GcodeBuffer.Append(Environment.NewLine);
            lastToken.Add(token);

            return SLValue.VOID;
        }

    }
}
