using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;

namespace SinumerikLanguage.Antlr4
{
    public class Scope
    {
        private Scope _parent;

        private Dictionary<String, SLValue> variables;
        private static Dictionary<String, SLValue> globalVariables;


        static Scope()
        {
            globalVariables = new Dictionary<string, SLValue>();
            for(int i=0; i<200; i++)
            {
                globalVariables["R" + i] = new SLValue(default(double));
            }

        }

        public Scope():this(null)
        {
            //globalVariables = new Dictionary<string, SLValue>();
            // only for the global scope, the parent is null
        }

        public Scope(Scope p)
        {
            _parent = p;
            variables = new Dictionary<string, SLValue>();

        }

        public void assignParam(String var, SLValue value)
        {
            variables[var] = value;
        }

        public void assign(String var, SLValue value)
        {
            if (resolve(var) != null)
            {
                // There is already such a variable, re-assign it
                this.reAssign(var, value);
            }
            else
            {
                // A newly declared variable
                variables[var] = value;
            }
        }

        public void GlobalAssign(String var, SLValue value)
        {
            globalVariables[var] = value;
        }


        private bool isParentScope()
        {
            return _parent == null;
        }

        public Scope parent()
        {
            return _parent;
        }

        private void reAssign(String identifier, SLValue value)
        {
            if (variables.ContainsKey(identifier))
            {
                // The variable is declared in this scope
                variables[identifier] = value;
            }
            else if (_parent != null)
            {
                // The variable was not declared in this scope, so let
                // the parent scope re-assign it
                _parent.reAssign(identifier, value);
            }
        }

        public SLValue resolve(String var)
        {

            if (variables.ContainsKey(var))
            {
                // The variable resides in this scope
                return variables[var];
            }
            else if (!isParentScope())
            {
                // Let the parent scope look for the variable
                return _parent.resolve(var);
            }
            else if (globalVariables.ContainsKey(var))
            {
                variables[var] = globalVariables[var];        
                return variables[var];
            }
            else
            {
                // Unknown variable
                return null;
            }    
        }

        public SLValue GetDefaultValue(string typeValue)
        {

            switch(typeValue)
            {
                case "REAL":
                    return new SLValue(default(double));
                case "INT":
                    return new SLValue(default(int));
                case "BOOL":
                    return new SLValue(default(bool));
                case "STRING":
                    return new SLValue(default(string));
                case "CHAR":
                    return new SLValue(default(char));
                default:
                    return SLValue.NULL;

            }
        }

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach(KeyValuePair<String, SLValue> var in variables)
            {
                sb.Append(var.Key).Append("->").Append(var.Value).Append(",");
            }
            return sb.ToString();
        }

    }
}
