grammar Sinumerik;

parse
 : block EOF
 ;

block
 : ( statement | functionDecl)*
 ;

statement
 : assignment
 | vardefinition
 | gcodeStatement
 | functionCall
 | ifStatement
 | forStatement
 | whileStatement
 | ifGotostat
 | gotoStatement
 | metkaStart
 | crlfStatement
 | returnStatement
 ;

assignment
 : Identifier indexes? '=' expression
 ;

vardefinition
: Def typeDef varlist+
;

functionCall
 : Identifier '(' exprList? ')'             #identifierFunctionCall
 | Println '(' expression? ')'              #printlnFunctionCall
 | Sin '(' expression ')'                   #sinFunctionCall
 | Cos '(' expression ')'                   #cosFunctionCall
 | Tan '(' expression ')'                   #tanFunctionCall
 | ASin '(' expression ')'                  #asinFunctionCall
 | ACos '(' expression ')'                  #acosFunctionCall
 | ATan '(' expression ')'                  #atanFunctionCall
 | ATan2 '(' expression ',' expression ')'  #atan2FunctionCall
 | Sqrt '(' expression ')'                  #sqrtFunctionCall
 | Trunc '(' expression ')'                 #truncFunctionCall
 | Abs '(' expression ')'                   #absFunctionCall
 | Pot '(' expression ')'                   #potFunctionCall
 | Round '(' expression ')'                 #roundFunctionCall
 | ModeAC '(' expression ')'                #modeacFunctionCall
 | ModeIC '(' expression ')'                #modeicFunctionCall
 | Trans                                    #transFunctionCall
 | Atrans                                   #atransFunctionCall
 | Rot                                      #rotFunctionCall
 | Arot                                     #arotFunctionCall
 | Scale                                    #scaleFunctionCall
 | AScale                                   #ascaleFunctionCall
 | Mirror                                   #mirrorFunctionCall
 | Amirror                                  #amirrorFunctionCall
 | Oriwks                                   #oriwksFunctionCall
 | Oriaxes                                  #oriaxesFunctionCall
 | Traori                                   #traoriFunctionCall
 | Diamon                                   #diamonFunctionCall
 | Diamof                                   #diamofFunctionCall
 | Diam90                                   #diam90FunctionCall
 | Displof                                  #displofFunctionCall
 | Displon                                  #displonFunctionCall
 | Sblof                                    #sblofFunctionCall
 | Sblon                                    #sblonFunctionCall
 | Save                                     #saveFunctionCall
 | Xaxis '=' expression                     #xcoordFunctionCall
 | Yaxis '=' expression                     #ycoordFunctionCall
 | Zaxis '=' expression                     #zcoordFunctionCall
 | Aaxis '=' expression                     #acoordFunctionCall
 | Baxis '=' expression                     #bcoordFunctionCall
 | Caxis '=' expression                     #ccoordFunctionCall
 | Uaxis '=' expression                     #ucoordFunctionCall
 | Vaxis '=' expression                     #vcoordFunctionCall
 | Waxis '=' expression                     #wcoordFunctionCall
 | Ivect '=' expression                     #ivectFunctionCall
 | Jvect '=' expression                     #jvectFunctionCall
 | Kvect '=' expression                     #kvectFunctionCall
 | Radius '=' expression                    #radiusFunctionCall
 | Turn '=' expression                      #turnFunctionCall
// | GFunc                        #gmodeFunctionCall
 | MFunc '='? Number?                       #mmodeFunctionCall
 | FFunc '=' expression                     #feedFunctionCall
 | SFunc '=' expression                     #speedFunctionCall
 | TFunc                                    #toolNumberFunctionCall
 | TFunc '=' String                         #toolNameFunctionCall
 | DFunc                                    #toolIDFunctionCall
 | SubProg '(' exprList? ')'                #subprogramFunctionCall
 | Msg '(' printvar? ')'                    #msgFunctionCall
 | Setal '(' expression ')'                 #setalFunctionCall
 | Stopre                                   #stopreFunctionCall
 ;


ifStatement
 : ifStat elseStat? EndIf
 ;

ifStat
 : If expression statement*
 ;

 ifGotostat
 : If expression GotoB metkaDest
 | If expression GotoF metkaDest
 | If expression Goto metkaDest
 ;

elseStat
 : Else statement*
 ;

functionDecl
 : Proc Identifier '(' idList? ')' block returnStatement
 ;

forStatement
 : For Identifier '=' expression To expression block EndFor
 ;

whileStatement
 : While expression block EndWhile
 ;

 gotoStatement
 : GotoB metkaDest
 | GotoF metkaDest
 | Goto metkaDest
 ;

returnStatement
: Return
;

idList
 : typeDef Identifier ( ',' typeDef Identifier )*
 ;

exprList
 : expression ( ',' expression )*
 ;

expression
 : '-' expression                                       #unaryMinusExpression
 | 'NOT' expression                                     #notExpression
 | <assoc=right> expression '^' expression              #powerExpression
 | expression op=( '*' | '/' | '%' ) expression         #multExpression
 | expression op=( '+' | '-' ) expression               #addExpression
 | expression op=( '>=' | '<=' | '>' | '<' ) expression #compExpression
 | expression op=( '==' | '<>' ) expression             #eqExpression
 | expression And expression                            #andExpression
 | expression Or expression                             #orExpression
 | expression Mod expression                            #modExpression
 | expression Div expression                            #divExpression
 | Number                                               #numberExpression
 | Bool                                                 #boolExpression
 | Null                                                 #nullExpression
 | functionCall indexes?                                #functionCallExpression
 | list indexes?                                        #listExpression
 | Identifier indexes?                                  #identifierExpression
 | String indexes?                                      #stringExpression
 | '(' expression ')' indexes?                          #expressionExpression
 | Input '(' String? ')'                                #inputExpression
 ;

gcodeStatement
: GCodeText (GCodeText CR?)*
;

list
 : '[' exprList? ']'
 ;

varlist
   : Identifier indexes? '='? expression? ','?
   ;

typeDef
    : 'INT'| 'int'
    | 'STRING'| 'string'
    | 'REAL'| 'real'
    | 'BOOL'| 'bool'
    | 'CHAR'| 'char'
    | 'AXIS'| 'axis'
    | 'FRAME'| 'frame'
    ;

indexes
 : ( '[' expression ']' )+
 ;

printvar
 : ('<<'? expression '<<'?)+
 ;

metkaStart
  : Labelstart
  ;

metkaDest
  : Identifier
  ;

Println  : 'println';
Print    : 'print';
Input    : 'input';
Assert   : 'assert';
Size     : 'size';
Sin      : 'sin'|'SIN';
ASin     : 'asin'|'ASIN';
Cos      : 'cos'|'COS';
ACos     : 'acos'|'ACOS';
Tan      : 'tan'|'TAN';
ATan     : 'atan'|'ATAN';
ATan2    : 'atan2'|'ATAN2';
Abs      : 'abs'|'ABS';
Sqrt     : 'sqrt'|'SQRT';
Trunc    : 'trunc'|'TRUNC';
Pot      : 'pot'|'POT';
Mod      : 'mod'|'MOD';
Div      : 'div'|'DIV';
Round    : 'round'|'ROUND';
Def      : 'def'|'DEF';
Proc     : 'proc'|'PROC';
If       : 'if'|'IF';
EndIf    : 'endif'|'ENDIF';
Else     : 'else'|'ELSE';
Return   : 'ret'|'RET'|('m'|'M')('17');
For      : 'for'|'FOR';
EndFor   : 'endfor'|'ENDFOR';
While    : 'while'|'WHILE';
EndWhile : 'endwhile'|'ENDWHILE';
GotoB    : 'gotob'|'GOTOB';
GotoF    : 'gotof'|'GOTOF';
Goto     : 'goto'|'GOTO';
Trans    : 'trans'|'TRANS';
Atrans   : 'atrans'|'ATRANS';
Rot      : 'rot'|'ROT';
Arot     : 'arot'|'AROT';
Mirror   : 'mirror'|'MIRROR';
Amirror  : 'amirror'|'AMIRROR';
Scale    : 'scale'|'SCALE';
AScale   : 'ascale'|'ASCALE';
Diamon   : 'diamon'|'DIAMON';
Diamof   : 'diamof'|'DIAMOF';
Diam90   : 'diam90'|'DIAM90';
Oriwks   : 'oriwks'|'ORIWKS';
Oriaxes  : 'oriaxes'|'ORIAXES';
Traori   : 'traori'|'TRAORI';
Turn     : 'turn'|'TURN';
Msg      : 'msg'|'MSG';
Setal    : 'setal'|'SETAL';
Sblof    : 'sblof'|'SBLOF';
Sblon    : 'sblon'|'SBLON';
Save     : 'save'|'SAVE';
Displof  : 'displof'|'DISPLOF';
Displon  : 'displon'|'DISPLON';
Stopre   : 'stopre'|'STOPRE';
To       : 'to'|'TO';
End      : 'end';
SubProg  : ('l'|'L')('0'..'9')+;
Null     : 'null';
//GFunc    : ('g' | 'G')('0'..'9')+;
MFunc    : ('m' | 'M')('0'..'9')+;
FFunc    : ('f' | 'F');
SFunc    : ('s' | 'S');
TFunc    : ('t' | 'T')('0'..'9')*;
DFunc    : ('d' | 'D')('0'..'9')+;
Nnumb    : ('n' | 'N')('0'..'9')+ -> skip;
Xaxis    : 'x' | 'X';
Yaxis    : 'y' | 'Y';
Zaxis    : 'z' | 'Z';
Aaxis    : 'a' | 'A';
Baxis    : 'b' | 'B';
Caxis    : 'c' | 'C';
Uaxis    : 'u' | 'U';
Vaxis    : 'v' | 'V';
Waxis    : 'w' | 'W';
Ivect    : 'i' | 'I';
Jvect    : 'j' | 'J';
Kvect    : 'k' | 'K';
ModeAC   : 'ac'| 'AC';
ModeIC   : 'ic'| 'IC';
Radius   : 'cr'| 'CR';

Or       : 'or'|'OR';
And      : 'and'|'AND';
Equals   : '==';
NEquals  : '<>';
GTEquals : '>=';
LTEquals : '<=';
Pow      : '^';
Excl     : '!';
GT       : '>';
LT       : '<';
Add      : '+';
Subtract : '-';
Multiply : '*';
Divide   : '/';
Modulus  : '%';
OBrace   : '{';
CBrace   : '}';
OBracket : '[';
CBracket : ']';
OParen   : '(';
CParen   : ')';
//SColon   : ';';
Assign   : '=';
Comma    : ',';
QMark    : '?';
Colon    : ':';

Bool
 : 'TRUE'|'true'
 | 'FALSE'|'false'
 ;

Number
 : Int ( '.' Digit* )?
 ;

GCodeText
 : [aAbBcCgGsStTdDmMfFiIjJkKxXyYzZuUvVwW][\t]*[+-]?[0-9]+[.]?[0-9]*
 ;

Identifier
 : [_a-zA-Z][_a-zA-Z][a-zA-Z_0-9]*
 | [rR][0-9]*
 | [$][_a-zA-Z0-9]+
 ;

Labelstart
 : [_a-zA-Z][_a-zA-Z][a-zA-Z_0-9]*[:]
 ;

Mcodes
 : [mM][0-9]+
 ;

String
 : ["] ( ~["\r\n\\] | '\\' ~[\r\n] )* ["]
 | ['] ( ~['\r\n\\] | '\\' ~[\r\n] )* [']
 ;


Comment
// : ( ';' .*? (CR|EOF) ) -> skip
: [;] (~[\r\n])* -> skip
;

//Space
// : [ \t\r\n\u000C] -> skip
// ;

WHITESPACE : (' ' | '\t') -> skip ;

crlfStatement
 : CR
 ;

CR
: [\r]?[\n]
;

fragment Int
 : [1-9] Digit*
 | '0'
 ;
  
fragment Digit 
 : [0-9]
 ;