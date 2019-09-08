using System;
using System.Windows.Media.Media3D ;

namespace gcodeparser
{
	/// <summary>
	/// Description of ParsedWord.
	/// </summary>
public  class ParsedCmdGroup {
    public readonly String CmdName;    // example: TRANS, ROT, AROT, MIRROR e.t.c
    public readonly String AxisName1;  // example: XYZ
    public readonly String AxisName2;
    public readonly String AxisName3;
    public readonly String parsed1;
    public readonly String parsed2;
    public readonly String parsed3;    
    public readonly Double ValueAxis1;   // example: 0 or 100.0
    public readonly Double ValueAxis2;
    public readonly Double ValueAxis3;
    public readonly String asRead;  // example: G00 or F100.00
    
    public ParsedCmdGroup(String CmdName, String asRead)
    {
    	this.CmdName = CmdName;
    	this.asRead = asRead; 
    }

    public ParsedCmdGroup(String CmdName, String AxisName1, Double ValueAxis1, String parsed1, String asRead) {
        this.CmdName = CmdName;
        this.AxisName1 = AxisName1;
        this.ValueAxis1 = ValueAxis1;
        this.asRead = asRead;   
    }
    public ParsedCmdGroup(String CmdName, String AxisName1, Double ValueAxis1, String parsed1, String AxisName2, Double ValueAxis2, String parsed2, String asRead) {
        this.CmdName = CmdName;
        this.AxisName1 = AxisName1;
        this.ValueAxis1 = ValueAxis1;
        this.AxisName2 = AxisName2;
        this.ValueAxis2 = ValueAxis2;        
        this.asRead = asRead;   
    }
    public ParsedCmdGroup(String CmdName, String AxisName1, Double ValueAxis1, String parsed1,  String AxisName2, Double ValueAxis2, String parsed2, String AxisName3, Double ValueAxis3, String parsed3, String asRead) {
        this.CmdName = CmdName;
        this.AxisName1 = AxisName1;
        this.ValueAxis1 = ValueAxis1;
        this.AxisName2 = AxisName2;
        this.ValueAxis2 = ValueAxis2;
        this.AxisName3 = AxisName3;
        this.ValueAxis3 = ValueAxis3;        
        this.asRead = asRead;   
    }
      
    public override String ToString() {
        return "ParsedWord{" +
        		", CmdName='" + CmdName + '\'' +
                ", AxisName1='" + AxisName1 + '\'' +
                ", value1=" + ValueAxis1 +
                ", AxisName2='" + AxisName2 + '\'' +
                ", value2=" + ValueAxis2 +
                ", AxisName3='" + AxisName3 + '\'' +
                ", value3=" + ValueAxis3 +        	
                '}';
    }
}
}
