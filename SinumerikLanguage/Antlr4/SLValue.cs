using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;

namespace SinumerikLanguage.Antlr4
{
    public class SLValue : IComparable<SLValue>
    {

        public static readonly SLValue NULL = new SLValue();
        public static readonly SLValue VOID = new SLValue();
        private NumberFormatInfo numberInfo ;
        

        private Object value;

        private SLValue()
        {
            // private constructor: only used for NULL and VOID
            value = new Object();
        }

        public SLValue(Object v)
        {
            numberInfo = new NumberFormatInfo();
            numberInfo.NumberDecimalSeparator = ".";

            if (v == null)
            {
                throw new SystemException("v == null");
            }
            value = v;
            // only accept boolean, list, number or string types
            if (!(isBoolean() || isList() || isNumber() || isString()))
            {
                throw new SystemException("invalid data type: " + "(" + v.GetType() + ")");
            }
        }


        public bool asBoolean()
        {
            return Convert.ToBoolean(value);
        }

        public double asDouble()
        {
            return Double.Parse(value.ToString(), NumberStyles.AllowDecimalPoint|NumberStyles.AllowLeadingSign|NumberStyles.AllowExponent, CultureInfo.CurrentCulture);
        }

        public decimal asDecimal()
        {
            return Decimal.Parse(value.ToString(), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.AllowExponent, CultureInfo.CurrentCulture);
        }

        public long asLong()
        {
            return Convert.ToInt64(value);
        }

        
        public List<SLValue> asList()
        {
            return (List<SLValue>)value;
        }

        public String asString()
        {
            return value.ToString();
        }

        public int CompareTo(SLValue that)
        {
            if (this.isNumber() && that.isNumber())
            {
                if (this.Equals(that))
                {
                    return 0;
                }
                else
                {
                    return this.asDouble().CompareTo(that.asDouble());
                }
            }
            else if (this.isString() && that.isString())
            {
                return this.asString().CompareTo(that.asString());
            }
            else
            {
                throw new SystemException("illegal expression: can't compare `" + this + "` to `" + that + "`");
            }
        }


        public override bool Equals(Object o)
        {
            if (this == VOID || o == VOID)
            {
                throw new SystemException("can't use VOID: " + this + " ==/!= " + o);
            }
            if (this == o)
            {
                return true;
            }
            if (o == null || this.GetType() != o.GetType())
            {
                return false;
            }
            SLValue that = (SLValue)o;
            if (this.isNumber() && that.isNumber())
            {
                double diff = Math.Abs(this.asDouble() - that.asDouble());
                return diff < 0.00000000001;
            }
            else
            {
                return this.value.Equals(that.value);
            }
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public bool isBoolean()
        {
            return value is bool  ;
        }

        public bool isNumber()
        {
            return Numbers.IsNumber(value);
        }

        public bool isList()
        {
            return value is List<SLValue>;
        }

        public bool isNull()
        {
            return this == NULL;
        }

        public bool isVoid()
        {
            return this == VOID;
        }

        public bool isString()
        {
            return value is string;
        }

        public override String ToString()
        {
            return isNull() ? "NULL" : isVoid() ? "VOID" : value.ToString();
        }

    }

    public static class Numbers
    {
        public static bool IsNumber(this object value)
        {
            return value is sbyte
                    || value is byte
                    || value is short
                    || value is ushort
                    || value is int
                    || value is uint
                    || value is long
                    || value is ulong
                    || value is float
                    || value is double
                    || value is decimal;
        }

        //public static double ToDouble(this object value)
        //{
        //    return Convert.ToDouble(value.ToString().Replace('.', ','));
        //}
    }


}
