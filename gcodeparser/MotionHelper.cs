
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text ;
using System.Windows.Media.Media3D ;
using System.IO;
using gcodeparser.gcodes;


namespace gcodeparser
{
	/// <summary>
	/// Description of MotionHelper.
	/// </summary>
	public class MotionHelper
	{
			MachineStatusHelper stat = new MachineStatusHelper();
//			MachineStatus machineStatus = new MachineStatus();
			private StringBuilder CM_CW_line = new StringBuilder() ;
			
			private StringBuilder position = new StringBuilder() ;
			public Point3D ArcPtc ;
			public Point3D ArcPtcTr ;
			public Point3D currentValue ;
			public Point3D oldValue ;
			public Point3D midArcPt ;
			Point3D trPt ;
			Matrix3D mToolAx = new Matrix3D();
			public static double HelixMode ;
			public double RotSumB {get ; set ;}
			public double LengthIvector ;
			public double LengthKvector ;
			private double Phi0 {get ;set ;} 
        	private double R0 {get ;set ;}
        	private double R1 {get ;set ;}
        	private double R2 {get ;set ;}
        	private double R3 {get ;set ;}
			private double AngCW {get ;set ;}
			private double AngCCW {get ;set ;} 
        	private double Rmid {get ;set ;}
        	private double Height {get; set;}
        	private int num {get; set;}
        	private Point3D pt;
        	private double Hst {get; set;}
        	
			        			
			public Vector3D GetToolAxis(String ActPlane, MachineStatus mstat)
			{
//				Vector3D toolAxis = new Vector3D() ;
				Vector3D toolVector = new Vector3D();
				Vector3D toolVectorTransform = new Vector3D() ;
				
					if (ActPlane == "G17") {  toolVector = new Vector3D(0, 0, 1) ; toolVectorTransform = Vector3D.Multiply(toolVector, mstat.mbase) ; }
					if (ActPlane == "G18") {  toolVector = new Vector3D(0, 1, 0) ; toolVectorTransform = Vector3D.Multiply(toolVector, mstat.mbase)  ; }
					if (ActPlane == "G19") {  toolVector = new Vector3D(1, 0, 0) ; toolVectorTransform = Vector3D.Multiply(toolVector, mstat.mbase) ; }
					
					mstat.ToolAxis = toolVectorTransform ;
//					string tAxis = "tAxisX" + toolAxis.X.ToString("F9") + " " + "tAxisY" + toolVectorTransform.Y.ToString("F9") + " " + "tAxisZ" + toolVectorTransform.Z.ToString("F9") ;					
					return mstat.ToolAxis ;
			}
			
			private String ChangeToolAxis(String ActPlane, Point3D toolPt, MachineStatus mstat)
			{
				Vector3D toolAxis = new Vector3D() ;
				switch(ActPlane)
				{
					case "G19":
		            toolAxis.X = 1 ; toolAxis.Y = 0 ; toolAxis.Z = 0  ;
		            break;
		            case "G18":
		            toolAxis.X = 0 ; toolAxis.Y = toolPt.Y ; toolAxis.Z = toolPt.Z  ;
		            break ;
		           	case "G17":
		            toolAxis.X = 0 ; toolAxis.Y = toolPt.Y ; toolAxis.Z = toolPt.Z  ;
		            break;
				}
		            if (toolAxis.Length.Equals(0.0)) { toolAxis = mstat.ToolAxis ;}
		            toolAxis = Vector3D.Multiply(toolAxis, mstat.mbase) ;
		            toolAxis.Normalize();
				
		            string tAxis = "tAxisX" + toolAxis.X.ToString("F9") + " " + "tAxisY" + toolAxis.Y.ToString("F9") + " " + "tAxisZ" + toolAxis.Z.ToString("F9") ;				
		            return tAxis ;
			}
			
			public bool GetCurrentTransform(MachineStatus mstat)
			{
					Vector3D vctX = new Vector3D() ;
					Vector3D vctY = new Vector3D() ;
					Vector3D vctZ = new Vector3D() ;
        			vctX = Vector3D.Multiply(new Vector3D(1,0,0), mstat.mbase) ;
        			vctY = Vector3D.Multiply(new Vector3D(0,1,0), mstat.mbase) ;
        			vctZ = Vector3D.Multiply(new Vector3D(0,0,1), mstat.mbase) ;
        			double a = Vector3D.AngleBetween(new Vector3D(1,0,0), vctX ) ;
        			double b = Vector3D.AngleBetween(new Vector3D(0,1,0), vctY ) ;
        			double c = Vector3D.AngleBetween(new Vector3D(0,0,1), vctZ ) ;
        			if (!a.Equals(0) || !b.Equals(0) || !c.Equals(0) ) { return true ; }
        			
        			return false ;
			}
			
		
				public StringBuilder NX_circular_motion_CW(String ActPlane, MachineStatus mstat, double tolerance, MachineStatus.Axis axis1, double mx_crd, double x_crd, MachineStatus.Axis axis2, double my_crd, double y_crd, MachineStatus.Axis axis3, double mz_crd, double z_crd, double i_vct, double j_vct, double k_vct, double turn )
				{
					CM_CW_line.Clear() ;
					
					Vector3D vector1 = new Vector3D();  // вектор оси инструмента заданный в программе обработки
					Vector3D vectorI = new Vector3D() ;
					Vector3D vectorK = new Vector3D() ;
					Matrix3D mRotate = new Matrix3D() ; // matrix is default
					Point3D midPtFullArc = new Point3D() ;
					bool arcHelixMode = false ;
					double n1 = 0.06 ;
					double n2 = 0.5 ;
					double n3 = 0.0 ;
					double Dtool = 10.0 ;
					
					if (ActPlane == "G17") { vectorI = new Vector3D(0, 1, 0) ; vectorK = new Vector3D(1, 0, 0) ; vector1 = new Vector3D(0, 0, 1) ; LengthIvector = j_vct ; LengthKvector = i_vct ; }
					if (ActPlane == "G18") { vectorI = new Vector3D(1, 0, 0) ; vectorK = new Vector3D(0, 0, 1) ; vector1 = new Vector3D(0, 1, 0) ; LengthIvector = i_vct ; LengthKvector = k_vct ; }
					if (ActPlane == "G19") { vectorI = new Vector3D(0, 0, 1) ; vectorK = new Vector3D(0, 1, 0) ; vector1 = new Vector3D(1, 0, 0) ; LengthIvector = k_vct ; LengthKvector = j_vct ; }
					
					double radius = Math.Sqrt(Math.Pow((LengthIvector),2) + Math.Pow((LengthKvector),2));
					Vector3D vectorChord = new Vector3D(x_crd - mx_crd, y_crd - my_crd, z_crd - mz_crd); // вектор - хорда между начальной и конечной точкой дуги
					Vector3D hfVectorChord = Vector3D.Multiply(vectorChord, 0.5) ;
					
					Vector3D toolAxis = new Vector3D();
					toolAxis = Vector3D.Multiply(vector1,mstat.mbase);  // вектора инструмента после поворота системы координат 
					vectorI = Vector3D.Multiply(vectorI,mstat.mbase) ;
					vectorK = Vector3D.Multiply(vectorK,mstat.mbase) ;
					double ort = toolAxis.X*vectorChord.X + toolAxis.Y*vectorChord.Y + toolAxis.Z*vectorChord.Z ;  // z - coordinate along in tool axis
					ort = Math.Round(ort, 6) ;					
					Vector3D VectOrt = new Vector3D();
					Vector3D reversVectOrtMidPt = new Vector3D();
					Vector3D reversVectOrtEndPt = new Vector3D();
					Vector3D VectTransI = new Vector3D();
					Vector3D VectTransK = new Vector3D();
					Vector3D CrossVector = new Vector3D();
					Vector3D BissectVector = new Vector3D();
					Vector3D AlongShCrossVct = new Vector3D() ;
					Point3D midPtVctChord = new Point3D(hfVectorChord.X + x_crd, hfVectorChord.Y + y_crd, hfVectorChord.Z + z_crd) ;
//					Console.WriteLine("PtChrd.X" + midPtVctChord.X + " " + "PtChrd.Y" + midPtVctChord.Y + " " + "PtChrd.Z" + midPtVctChord.Z ) ;
					
					VectOrt = Vector3D.Multiply(toolAxis, ort)  ;  
					reversVectOrtMidPt = Vector3D.Multiply(Vector3D.Multiply(toolAxis, -1), ort*0.5) ;
					reversVectOrtEndPt = Vector3D.Multiply(Vector3D.Multiply(toolAxis, -1), ort) ;
					Point3D ortPt = new Point3D(VectOrt.X + mx_crd, VectOrt.Y + my_crd, VectOrt.Z + mz_crd ) ;						
					VectTransI = Vector3D.Multiply(vectorI, LengthIvector) ;
					VectTransK = Vector3D.Multiply(vectorK, LengthKvector) ;
		//			CrossVector = Vector3D.CrossProduct(new Vector3D(x_crd-ortPt.X, y_crd - ortPt.Y, z_crd - ortPt.Z), toolAxis) ;
					CrossVector = Vector3D.CrossProduct(vectorChord, toolAxis) ;
					CrossVector.Normalize() ;
					CrossVector = Vector3D.Multiply(CrossVector, radius) ;
					
					Point3D PtIvct = new Point3D(VectTransI.X + x_crd, VectTransI.Y + y_crd, VectTransI.Z + z_crd ) ;
					Point3D ptcArc = new Point3D(VectTransK.X+PtIvct.X, VectTransK.Y+PtIvct.Y, VectTransK.Z+PtIvct.Z);  // координаты точки центра окружности
					Point3D ptcHelixMid = new Point3D(reversVectOrtMidPt.X + ptcArc.X, reversVectOrtMidPt.Y + ptcArc.Y, reversVectOrtMidPt.Z + ptcArc.Z) ;
					Point3D ptcHelixEndPt = new Point3D(reversVectOrtEndPt.X + ptcArc.X, reversVectOrtEndPt.Y + ptcArc.Y, reversVectOrtEndPt.Z + ptcArc.Z) ;
					Vector3D Radius1 = new Vector3D(x_crd - ptcArc.X, y_crd - ptcArc.Y, z_crd - ptcArc.Z) ;
					Vector3D Radius2 = new Vector3D(ortPt.X - ptcArc.X, ortPt.Y - ptcArc.Y, ortPt.Z - ptcArc.Z ) ;

					double HalhOfAngArc = Vector3D.AngleBetween(Radius1, Radius2)*(0.5) ;
			//		Console.WriteLine(HalhOfAngArc) ;
					HalhOfAngArc = Math.Round(HalhOfAngArc, 6) ;
					Point3D midPtArc = new Point3D(CrossVector.X+ptcArc.X, CrossVector.Y+ptcArc.Y, CrossVector.Z+ptcArc.Z ) ; // middle point of arc
					if (HalhOfAngArc.Equals(0)) 
					{
						HalhOfAngArc = 180 ;
						mRotate.RotateAt(new Quaternion(toolAxis, -HalhOfAngArc), ptcArc) ;
						BissectVector = Vector3D.Multiply(Radius1, mRotate ) ;
						midPtFullArc = new Point3D(BissectVector.X+ptcArc.X, BissectVector.Y+ptcArc.Y, BissectVector.Z+ptcArc.Z ) ;
						midPtArc = midPtFullArc ;
					}
					
//					Vector3D helixMidPtVect = Vector3D.Multiply(toolAxis, -ort*0.5 ) ;
					Point3D helixMidPtArc = new Point3D( reversVectOrtMidPt.X + midPtArc.X, reversVectOrtMidPt.Y + midPtArc.Y, reversVectOrtMidPt.Z + midPtArc.Z ) ;
			//		Point3D helixMidPtArc = new Point3D( CrossVector.X+midPtVctChord.X, CrossVector.Y+midPtVctChord.Y, CrossVector.Z+midPtVctChord.Z ) ;			
			//		Point3D midArc = new Point3D(CrosVector.X+ptcArc.X, CrosVector.Y+ptcArc.Y, CrosVector.Z+ptcArc.Z) ;
					Vector3D r1Vect = new Vector3D(ptcArc.X-x_crd, ptcArc.Y-y_crd, ptcArc.Z-z_crd ) ;
					Vector3D r2Vect = new Vector3D(ptcArc.X - midPtArc.X, ptcArc.Y - midPtArc.Y, ptcArc.Z - midPtArc.Z ) ;
					double AngArc = Vector3D.AngleBetween(r1Vect, r2Vect) ;
			//		AngArc = Math.Round(AngArc, 6) ;
					double AngHelix = AngArc*2 ;
					double Turn = Math.Round(AngHelix, 6)/360.0 ;
					double HalfTurn = Turn*0.5 ;

					if ( !ort.Equals(0) ) // if Arc as helix
					{
						midPtArc = helixMidPtArc ; 
						arcHelixMode = true ;
						HelicalMotion(Radius1, radius, ort, tolerance, AngHelix + turn*360, ptcArc, toolAxis, -1 ) ;
					}
					
					mx_crd = Math.Round(mx_crd, 6) ; my_crd = Math.Round(my_crd, 6) ; mz_crd = Math.Round(mz_crd, 6) ;
					midPtArc.X = Math.Round(midPtArc.X, 6) ; midPtArc.Y = Math.Round(midPtArc.Y, 6) ; midPtArc.Z = Math.Round(midPtArc.Z, 6) ;
					ptcArc.X = Math.Round(ptcArc.X, 6) ; ptcArc.Y = Math.Round(ptcArc.Y, 6) ; ptcArc.Z = Math.Round(ptcArc.Z, 6) ;
					ptcHelixMid.X = Math.Round(ptcHelixMid.X, 6) ; ptcHelixMid.Y = Math.Round(ptcHelixMid.Y, 6) ; ptcHelixMid.Z = Math.Round(ptcHelixMid.Z, 6) ;
					ptcHelixEndPt.X = Math.Round(ptcHelixEndPt.X, 6) ; ptcHelixEndPt.Y = Math.Round(ptcHelixEndPt.Y, 6) ; ptcHelixEndPt.Z = Math.Round(ptcHelixEndPt.Z, 6) ;
					toolAxis.X = Math.Round(toolAxis.X, 6) ; toolAxis.Y = Math.Round(toolAxis.Y, 6) ; toolAxis.Z = Math.Round(toolAxis.Z, 6) ;
										
					if (AngArc >= 89.9)
					{
						if (!arcHelixMode)
						{
							CM_CW_line.Append("CIRCLE/" + ptcArc.X.ToString("F6") + ";" + ptcArc.Y.ToString("F6") + ";" + ptcArc.Z.ToString("F6") + ";" + toolAxis.X.ToString("F6") + ";" + toolAxis.Y.ToString("F6") + ";" + toolAxis.Z.ToString("F6") + ";" + Math.Abs(radius).ToString("F9") + ";" + n1.ToString("F6") + ";" + n2.ToString("F6") + ";" + Dtool.ToString("F6") + ";" + n3.ToString("F6"));
							CM_CW_line.Append('\n') ;
							CM_CW_line.Append("GOTO/" + midPtArc.X.ToString("F6") + ";" + midPtArc.Y.ToString("F6") + ";" + midPtArc.Z.ToString("F6")) ;
							CM_CW_line.Append('\n') ;
							CM_CW_line.Append("CIRCLE/" + ptcArc.X.ToString("F6") + ";" + ptcArc.Y.ToString("F6") + ";" + ptcArc.Z.ToString("F6") + ";" + toolAxis.X.ToString("F6") + ";" + toolAxis.Y.ToString("F6") + ";" + toolAxis.Z.ToString("F6") + ";" + Math.Abs(radius).ToString("F9") + ";" + n1.ToString("F6") + ";" + n2.ToString("F6") + ";" + Dtool.ToString("F6") + ";" + n3.ToString("F6"));
							CM_CW_line.Append('\n') ;							
							CM_CW_line.Append("GOTO/" + mx_crd.ToString("F6") + ";" + my_crd.ToString("F6") + ";" + mz_crd.ToString("F6") );
							CM_CW_line.Append('\n') ;
						}
					}
					if (AngArc < 89.9)
					{
						if (!arcHelixMode)
						{
							CM_CW_line.Append("CIRCLE/" + ptcArc.X.ToString("F6") + ";" + ptcArc.Y.ToString("F6") + ";" + ptcArc.Z.ToString("F6") + ";" + toolAxis.X.ToString("F6") + ";" + toolAxis.Y.ToString("F6") + ";" + toolAxis.Z.ToString("F6") + ";" + Math.Abs(radius).ToString("F6") + ";" + n1.ToString("F6") + ";" + n2.ToString("F6") + ";" + Dtool.ToString("F6") + ";" + n3.ToString("F6"));
							CM_CW_line.Append('\n') ;
							CM_CW_line.Append("GOTO/" + mx_crd.ToString("F6") + ";" + my_crd.ToString("F6") + ";" + mz_crd.ToString("F6") );
							CM_CW_line.Append('\n') ;							
						}
					}
					
					return CM_CW_line.Replace(',','.') ;
				}
				
				
				public StringBuilder NX_circular_motion_CW(String ActPlane, MachineStatus mstat, double tolerance, MachineStatus.Axis axis1, double mx_crd, double x_crd, MachineStatus.Axis axis2, double my_crd, double y_crd, MachineStatus.Axis axis3, double mz_crd, double z_crd, double r_value, double turn )
				{
					
						CM_CW_line.Clear() ;
						bool arcHelixMode = false ;
						double n1 = 0.06 ; double n2 = 0.5 ; double n3 = 0.0 ; double Dtool = 10.0 ;						
						Vector3D vector1 = new Vector3D() ;
						Vector3D HelixDirVect = new Vector3D() ;
						if (ActPlane == "G17") { vector1 = new Vector3D(0, 0, 1) ;}
						if (ActPlane == "G18") { vector1 = new Vector3D(0, 1, 0) ;}
						if (ActPlane == "G19") { vector1 = new Vector3D(1, 0, 0) ;}

					//	Vector3D vector1 = new Vector3D(0,0,1) ; // Z Axis
						Vector3D vector2 = new Vector3D(mx_crd-x_crd, my_crd-y_crd, mz_crd-z_crd);
						Vector3D toolAxis = new Vector3D();
						toolAxis = Vector3D.Multiply(vector1,mstat.mbase);
						double ort = toolAxis.X*vector2.X + toolAxis.Y*vector2.Y + toolAxis.Z*vector2.Z ;  // z - coordinate along in tool axis
						ort = Math.Round(ort, 6) ;
						HelixDirVect = Vector3D.Multiply(toolAxis, ort)  ;
						HelixDirVect.Negate();
						Point3D ortPt = new Point3D(HelixDirVect.X + mx_crd, HelixDirVect.Y + my_crd, HelixDirVect.Z + mz_crd ) ;
						Point3D midPtHrd = new Point3D(ortPt.X - ((ortPt.X - x_crd)/2), ortPt.Y - ((ortPt.Y - y_crd)/2), ortPt.Z - ((ortPt.Z - z_crd)/2));
						Vector3D VectHrd = new Vector3D(ortPt.X - x_crd, ortPt.Y - y_crd, ortPt.Z - z_crd ) ;

						double a = Math.Sqrt(Math.Pow((ortPt.Z - z_crd),2) + Math.Pow((ortPt.X - x_crd),2) + Math.Pow((ortPt.Y - y_crd),2));						
						double h = Math.Sqrt(Math.Abs(Math.Pow((r_value),2) - Math.Pow((a/2),2))) ;	
						
						if (r_value > 0.0)
						{
							Vector3D VectOrt = new Vector3D();
							Vector3D VectResult = new Vector3D();
							VectOrt = Vector3D.CrossProduct(VectHrd, toolAxis) ;
							VectOrt.Normalize() ;
							VectResult = Vector3D.Multiply(VectOrt, h) ;
							Point3D ptc = new Point3D(VectResult.X+midPtHrd.X, VectResult.Y+midPtHrd.Y, VectResult.Z+midPtHrd.Z);
							Vector3D R1Vect = new Vector3D(x_crd - ptc.X, y_crd - ptc.Y, z_crd - ptc.Z) ;
							Vector3D R2Vect = new Vector3D(ortPt.X - ptc.X, ortPt.Y - ptc.Y, ortPt.Z - ptc.Z ) ;	
							double AngArc = Vector3D.AngleBetween(R1Vect, R2Vect) ;
							double Turn = Math.Round(AngArc, 6)/360.0 ;
							
							if (!ort.Equals(0))
							{
								
								HelicalMotion(R1Vect, r_value, ort, tolerance, AngArc + turn*360, ptc, toolAxis, -1 ) ;
							}
							else
							{
								CM_CW_line.Append("CIRCLE/" + ptc.X.ToString("F6") + ";" + ptc.Y.ToString("F6") + ";" + ptc.Z.ToString("F6") + ";" + toolAxis.X.ToString("F6") + ";" + toolAxis.Y.ToString("F6") + ";" + toolAxis.Z.ToString("F6") + ";" + Math.Abs(r_value).ToString("F6") + ";" + n1.ToString("F6") + ";" + n2.ToString("F6") + ";" + Dtool.ToString("F6") + ";" + n3.ToString("F6"));
								CM_CW_line.Append('\n') ;							
								CM_CW_line.Append("GOTO/" + mx_crd.ToString("F6") + ";" + my_crd.ToString("F6") + ";" + mz_crd.ToString("F6"));
								CM_CW_line.Append('\n');
							}
	
						}
						if (r_value < 0.0)
						{
							
							Vector3D VectOrt = new Vector3D();
							Vector3D VectResult = new Vector3D();
							Vector3D CrosVector = new Vector3D();
							VectOrt = Vector3D.CrossProduct(VectHrd, toolAxis) ;
							VectOrt.Normalize() ;
							VectResult = Vector3D.Multiply(VectOrt, -h) ;
							Point3D ptc = new Point3D(VectResult.X+midPtHrd.X, VectResult.Y+midPtHrd.Y, VectResult.Z+midPtHrd.Z);
							CrosVector = Vector3D.Multiply(VectOrt, -(h+Math.Abs(r_value))) ;
							Point3D midArc = new Point3D(CrosVector.X+midPtHrd.X, CrosVector.Y+midPtHrd.Y, CrosVector.Z+midPtHrd.Z) ;
							Vector3D R1Vect = new Vector3D(x_crd - ptc.X, y_crd - ptc.Y, z_crd - ptc.Z) ;
							Vector3D R2Vect = new Vector3D(midArc.X - ptc.X, midArc.Y - ptc.Y, midArc.Z - ptc.Z ) ;
							double AngArc = Vector3D.AngleBetween(R1Vect, R2Vect)*2 ;
							
							if (!ort.Equals(0))
							{
								
								HelicalMotion(R1Vect, Math.Abs(r_value), ort, tolerance, AngArc+turn*360, ptc, toolAxis, -1 ) ;
							}
							else
							{
								CM_CW_line.Append("CIRCLE/" + ptc.X.ToString("F6") + ";" + ptc.Y.ToString("F6") + ";" + ptc.Z.ToString("F6") + ";" + toolAxis.X.ToString("F6") + ";" + toolAxis.Y.ToString("F6") + ";" + toolAxis.Z.ToString("F6") + ";" + Math.Abs(r_value).ToString("F9") + ";" + n1.ToString("F6") + ";" + n2.ToString("F6") + ";" + Dtool.ToString("F6") + ";" + n3.ToString("F6"));
								CM_CW_line.Append('\n') ;
								CM_CW_line.Append("GOTO/" + midArc.X.ToString("F6") + ";" + midArc.Y.ToString("F6") + ";" + midArc.Z.ToString("F6")) ;
								CM_CW_line.Append('\n') ;
								CM_CW_line.Append("CIRCLE/" + ptc.X.ToString("F6") + ";" + ptc.Y.ToString("F6") + ";" + ptc.Z.ToString("F6") + ";" + toolAxis.X.ToString("F6") + ";" + toolAxis.Y.ToString("F6") + ";" + toolAxis.Z.ToString("F6") + ";" + Math.Abs(r_value).ToString("F9") + ";" + n1.ToString("F6") + ";" + n2.ToString("F6") + ";" + Dtool.ToString("F6") + ";" + n3.ToString("F6"));
								CM_CW_line.Append('\n') ;							
								CM_CW_line.Append("GOTO/" + mx_crd.ToString("F6") + ";" + my_crd.ToString("F6") + ";" + mz_crd.ToString("F6") );
								CM_CW_line.Append('\n') ;								
							}
//							string tAxis = "tAxisX" + toolAxis.X.ToString("F9") + " " + "tAxisY" + toolAxis.Y.ToString("F9") + " " + "tAxisZ" + toolAxis.Z.ToString("F9") ;
//							string ArcPointCenter = "ptc"+axis1+ptc.X.ToString("F9") + " " + "ptc"+axis2+ptc.Y.ToString("F9") + " " + "ptc"+axis3+ptc.Z.ToString("F9") ;


						}
						
						return CM_CW_line.Replace(',','.') ;
				
				}
				
				public StringBuilder NX_circular_motion_CCW(String ActPlane, MachineStatus mstat, double tolerance, MachineStatus.Axis axis1, double mx_crd, double x_crd, MachineStatus.Axis axis2, double my_crd, double y_crd, MachineStatus.Axis axis3, double mz_crd, double z_crd, double i_vct, double j_vct, double k_vct, double turn )
				{
						CM_CW_line.Clear() ;
						
						Vector3D vector1 = new Vector3D();  // вектор оси инструмента заданный в программе обработки
						Vector3D vectorI = new Vector3D() ;
						Vector3D vectorK = new Vector3D() ;
						Matrix3D mRotate = new Matrix3D() ; // matrix is default
						Point3D midPtFullArc = new Point3D() ;
						bool arcHelixMode = false ;
						double n1 = 0.06 ;
						double n2 = 0.5 ;
						double n3 = 0.0 ;
						double Dtool = 10.0 ;
						
						if (ActPlane == "G17") { vectorI = new Vector3D(0, 1, 0) ; vectorK = new Vector3D(1, 0, 0) ; vector1 = new Vector3D(0, 0, 1) ; LengthIvector = j_vct ; LengthKvector = i_vct ; }
						if (ActPlane == "G18") { vectorI = new Vector3D(1, 0, 0) ; vectorK = new Vector3D(0, 0, 1) ; vector1 = new Vector3D(0, 1, 0) ; LengthIvector = i_vct ; LengthKvector = k_vct ; }
						if (ActPlane == "G19") { vectorI = new Vector3D(0, 0, 1) ; vectorK = new Vector3D(0, 1, 0) ; vector1 = new Vector3D(1, 0, 0) ; LengthIvector = k_vct ; LengthKvector = j_vct ; }
						
						double radius = Math.Sqrt(Math.Pow((LengthIvector),2) + Math.Pow((LengthKvector),2));
						Vector3D vectorChord = new Vector3D(x_crd-mx_crd, y_crd-my_crd, z_crd-mz_crd); // вектор - хорда между начальной и конечной точкой дуги
	
						Vector3D toolAxis = new Vector3D();
						toolAxis = Vector3D.Multiply(vector1,mstat.mbase);  // вектора инструмента после поворота системы координат
						toolAxis.Negate() ;	// becose G3
						vectorI = Vector3D.Multiply(vectorI,mstat.mbase) ;
						vectorK = Vector3D.Multiply(vectorK,mstat.mbase) ;
						double ort = toolAxis.X*vectorChord.X + toolAxis.Y*vectorChord.Y + toolAxis.Z*vectorChord.Z ;  // z - coordinate along in tool axis
						ort = Math.Round(ort, 6) ;
						
						Vector3D VectOrt = new Vector3D();
	//					Vector3D VectResult = new Vector3D();
						Vector3D VectTransI = new Vector3D();
						Vector3D VectTransK = new Vector3D();
						Vector3D CrossVector = new Vector3D();
						Vector3D BissectVector = new Vector3D();
						
						VectOrt = Vector3D.Multiply(toolAxis, ort)  ;  // вектор перпендикулярный vector2 и toolAxis
						Point3D ortPt = new Point3D(VectOrt.X + mx_crd, VectOrt.Y + my_crd, VectOrt.Z + mz_crd ) ;
						VectTransI = Vector3D.Multiply(vectorI, LengthIvector) ;
						VectTransK = Vector3D.Multiply(vectorK, LengthKvector) ;
//						Vector3D radiusVect = new Vector3D(VectTransI.X + VectTransK.X, VectTransI.Y + VectTransK.Y, VectTransI.Z + VectTransK.Z ) ;
//						radiusVect.Normalize() ;
						CrossVector = Vector3D.CrossProduct(vectorChord, toolAxis) ;
						CrossVector.Normalize() ;
						CrossVector = Vector3D.Multiply(CrossVector, radius) ;
						
						Point3D PtIvct = new Point3D(VectTransI.X + x_crd, VectTransI.Y + y_crd, VectTransI.Z + z_crd ) ;
						Point3D ptcArc = new Point3D(VectTransK.X+PtIvct.X, VectTransK.Y+PtIvct.Y, VectTransK.Z+PtIvct.Z);  // координаты точки центра окружности
						Vector3D Radius1 = new Vector3D(x_crd - ptcArc.X, y_crd - ptcArc.Y, z_crd - ptcArc.Z) ;
						Vector3D Radius2 = new Vector3D(ortPt.X - ptcArc.X, ortPt.Y - ptcArc.Y, ortPt.Z - ptcArc.Z ) ;
						double HalhOfAngArc = Vector3D.AngleBetween(Radius1, Radius2)*(0.5) ;
						HalhOfAngArc = Math.Round(HalhOfAngArc, 6) ;
						Point3D midPtArc = new Point3D(CrossVector.X+ptcArc.X, CrossVector.Y+ptcArc.Y, CrossVector.Z+ptcArc.Z ) ;
						if (HalhOfAngArc.Equals(0)) 
						{
							HalhOfAngArc = 180 ;
							mRotate.RotateAt(new Quaternion(toolAxis, -HalhOfAngArc), ptcArc) ;
							BissectVector = Vector3D.Multiply(Radius1, mRotate ) ;
							midPtFullArc = new Point3D(BissectVector.X+ptcArc.X, BissectVector.Y+ptcArc.Y, BissectVector.Z+ptcArc.Z ) ;
							midPtArc = midPtFullArc ;
						}

			//			BissectVector = Vector3D.Multiply(BissectVector, mstat.mbase ) ;
						Vector3D r1Vect = new Vector3D(x_crd - ptcArc.X, y_crd - ptcArc.Y, z_crd - ptcArc.Z ) ;
						Vector3D r2Vect = new Vector3D(midPtArc.X - ptcArc.X, midPtArc.Y - ptcArc.Y, midPtArc.Z - ptcArc.Z ) ;
						double AngArc = Vector3D.AngleBetween(r1Vect, r2Vect) ;						
						double AngHelix = AngArc*2 ;
						double Turn = Math.Round(AngHelix, 6)/360.0 ;
						double HalfTurn = Turn*0.5 ;
						
						if ( !ort.Equals(0) ) // if Arc as helix
						{
							arcHelixMode = true ;
							Vector3D toolAx = toolAxis ;
							toolAx.Negate() ;
							HelicalMotion(Radius1, radius, ort, tolerance, AngHelix + turn*360, ptcArc, toolAx, 1 ) ;
						}
	
							mx_crd = Math.Round(mx_crd, 6) ; my_crd = Math.Round(my_crd, 6) ; mz_crd = Math.Round(mz_crd, 6) ;
							midPtArc.X = Math.Round(midPtArc.X, 6) ; midPtArc.Y = Math.Round(midPtArc.Y, 6) ; midPtArc.Z = Math.Round(midPtArc.Z, 6) ;
							ptcArc.X = Math.Round(ptcArc.X, 6) ; ptcArc.Y = Math.Round(ptcArc.Y, 6) ; ptcArc.Z = Math.Round(ptcArc.Z, 6) ;
							toolAxis.X = Math.Round(toolAxis.X, 6) ; toolAxis.Y = Math.Round(toolAxis.Y, 6) ; toolAxis.Z = Math.Round(toolAxis.Z, 6) ;
						
						if (AngArc >= 89.9)
						{
							if (!arcHelixMode)
							{
								CM_CW_line.Append("CIRCLE/" + ptcArc.X.ToString("F6") + ";" + ptcArc.Y.ToString("F6") + ";" + ptcArc.Z.ToString("F6") + ";" + toolAxis.X.ToString("F6") + ";" + toolAxis.Y.ToString("F6") + ";" + toolAxis.Z.ToString("F6") + ";" + Math.Abs(radius).ToString("F6") + ";" + n1.ToString("F6") + ";" + n2.ToString("F6") + ";" + Dtool.ToString("F6") + ";" + n3.ToString("F6"));
								CM_CW_line.Append('\n') ;
								CM_CW_line.Append("GOTO/" + midPtArc.X.ToString("F6") + ";" + midPtArc.Y.ToString("F6") + ";" + midPtArc.Z.ToString("F6")) ;
								CM_CW_line.Append('\n') ;
								CM_CW_line.Append("CIRCLE/" + ptcArc.X.ToString("F6") + ";" + ptcArc.Y.ToString("F6") + ";" + ptcArc.Z.ToString("F6") + ";" + toolAxis.X.ToString("F6") + ";" + toolAxis.Y.ToString("F6") + ";" + toolAxis.Z.ToString("F6") + ";" + Math.Abs(radius).ToString("F6") + ";" + n1.ToString("F6") + ";" + n2.ToString("F6") + ";" + Dtool.ToString("F6") + ";" + n3.ToString("F6"));
								CM_CW_line.Append('\n') ;							
								CM_CW_line.Append("GOTO/" + mx_crd.ToString("F6") + ";" + my_crd.ToString("F6") + ";" + mz_crd.ToString("F6") );
								CM_CW_line.Append('\n') ;
							}
						}
						if (AngArc < 89.9)
						{
							if (!arcHelixMode)
							{
								CM_CW_line.Append("CIRCLE/" + ptcArc.X.ToString("F6") + ";" + ptcArc.Y.ToString("F6") + ";" + ptcArc.Z.ToString("F6") + ";" + toolAxis.X.ToString("F6") + ";" + toolAxis.Y.ToString("F6") + ";" + toolAxis.Z.ToString("F6") + ";" + Math.Abs(radius).ToString("F6") + ";" + n1.ToString("F6") + ";" + n2.ToString("F6") + ";" + Dtool.ToString("F6") + ";" + n3.ToString("F6"));
								CM_CW_line.Append('\n') ;
								CM_CW_line.Append("GOTO/" + mx_crd.ToString("F6") + ";" + my_crd.ToString("F6") + ";" + mz_crd.ToString("F6") );
								CM_CW_line.Append('\n') ;							
							}

						}						

							return CM_CW_line.Replace(',','.') ;
											
				}
				
				public StringBuilder NX_circular_motion_CCW(String ActPlane, MachineStatus mstat, double tolerance, MachineStatus.Axis axis1, double mx_crd, double x_crd, MachineStatus.Axis axis2, double my_crd, double y_crd, MachineStatus.Axis axis3, double mz_crd, double z_crd, double r_value, double turn )
				{
					
						CM_CW_line.Clear() ;
						double n1 = 0.06 ; double n2 = 0.5 ; double n3 = 0.0 ; double Dtool = 10.0 ;
						Vector3D vector1 = new Vector3D() ;
						Vector3D HelixDirVect = new Vector3D();
						if (ActPlane == "G17") { vector1 = new Vector3D(0, 0, 1) ;}
						if (ActPlane == "G18") { vector1 = new Vector3D(0, 1, 0) ;}
						if (ActPlane == "G19") { vector1 = new Vector3D(1, 0, 0) ;}
						
					//	Vector3D vector1 = new Vector3D(0,0,1) ; // Z Axis
						Vector3D vector2 = new Vector3D(mx_crd-x_crd, my_crd-y_crd, mz_crd-z_crd);
						Vector3D toolAxis = new Vector3D();
						toolAxis = Vector3D.Multiply(vector1,mstat.mbase);						
						toolAxis.Negate() ;	// becose G3
						double ort = toolAxis.X*vector2.X + toolAxis.Y*vector2.Y + toolAxis.Z*vector2.Z ;  // z - coordinate along in tool axis
						ort = Math.Round(ort, 6) ;
						HelixDirVect = Vector3D.Multiply(toolAxis, ort)  ;
						HelixDirVect.Negate();
						Point3D ortPt = new Point3D(HelixDirVect.X + mx_crd, HelixDirVect.Y + my_crd, HelixDirVect.Z + mz_crd ) ;
						Point3D midPtHrd = new Point3D(ortPt.X - ((ortPt.X - x_crd)/2), ortPt.Y - ((ortPt.Y - y_crd)/2), ortPt.Z - ((ortPt.Z - z_crd)/2));
						Vector3D VectHrd = new Vector3D(ortPt.X - x_crd, ortPt.Y - y_crd, ortPt.Z - z_crd ) ;

						double a = Math.Sqrt(Math.Pow((ortPt.Z - z_crd),2) + Math.Pow((ortPt.X - x_crd),2) + Math.Pow((ortPt.Y - y_crd),2));						
						double h = Math.Sqrt(Math.Abs(Math.Pow((r_value),2) - Math.Pow((a/2),2))) ;							
						
						
						if (r_value > 0.0)
						{

							Vector3D VectOrt = new Vector3D();
							Vector3D VectResult = new Vector3D();
							VectOrt = Vector3D.CrossProduct(VectHrd, toolAxis) ;
							VectOrt.Normalize() ;
							VectResult = Vector3D.Multiply(VectOrt, h) ;
							Point3D ptc = new Point3D(VectResult.X+midPtHrd.X, VectResult.Y+midPtHrd.Y, VectResult.Z+midPtHrd.Z);
							Vector3D R1Vect = new Vector3D(x_crd - ptc.X, y_crd - ptc.Y, z_crd - ptc.Z) ;
							Vector3D R2Vect = new Vector3D(ortPt.X - ptc.X, ortPt.Y - ptc.Y, ortPt.Z - ptc.Z ) ;	
							double AngArc = Vector3D.AngleBetween(R1Vect, R2Vect) ;
							double Turn = Math.Round(AngArc, 6)/360.0 ;							
							
							if (!ort.Equals(0))
							{
								toolAxis.Negate();  
								HelicalMotion(R1Vect, r_value, ort, tolerance, AngArc + turn*360, ptc, toolAxis, 1 ) ;
							}
							else
							{
								CM_CW_line.Append("CIRCLE/" + ptc.X.ToString("F6") + ";" + ptc.Y.ToString("F6") + ";" + ptc.Z.ToString("F6") + ";" + toolAxis.X.ToString("F6") + ";" + toolAxis.Y.ToString("F6") + ";" + toolAxis.Z.ToString("F6") + ";" + Math.Abs(r_value).ToString("F6") + ";" + n1.ToString("F6") + ";" + n2.ToString("F6") + ";" + Dtool.ToString("F6") + ";" + n3.ToString("F6"));
								CM_CW_line.Append('\n') ;							
								CM_CW_line.Append("GOTO/" + mx_crd.ToString("F6") + ";" + my_crd.ToString("F6") + ";" + mz_crd.ToString("F6"));
								CM_CW_line.Append('\n');
							}							
							
						}
						if (r_value < 0.0)
						{
							
							Vector3D VectOrt = new Vector3D();
							Vector3D VectResult = new Vector3D();
							Vector3D CrosVector = new Vector3D();
							VectOrt = Vector3D.CrossProduct(VectHrd, toolAxis) ;
							VectOrt.Normalize() ;
							VectResult = Vector3D.Multiply(VectOrt, -h) ;
							Point3D ptc = new Point3D(VectResult.X+midPtHrd.X, VectResult.Y+midPtHrd.Y, VectResult.Z+midPtHrd.Z);
							CrosVector = Vector3D.Multiply(VectOrt, -(h+Math.Abs(r_value))) ;
							Point3D midArc = new Point3D(CrosVector.X+midPtHrd.X, CrosVector.Y+midPtHrd.Y, CrosVector.Z+midPtHrd.Z) ;
							Vector3D R1Vect = new Vector3D(x_crd - ptc.X, y_crd - ptc.Y, z_crd - ptc.Z) ;
							Vector3D R2Vect = new Vector3D(midArc.X - ptc.X, midArc.Y - ptc.Y, midArc.Z - ptc.Z ) ;
							double AngArc = Vector3D.AngleBetween(R1Vect, R2Vect)*2 ;
							
							if (!ort.Equals(0))
							{
								toolAxis.Negate(); 
								HelicalMotion(R1Vect, Math.Abs(r_value), ort, tolerance, AngArc+turn*360, ptc, toolAxis, 1 ) ;
							}
							else
							{
								CM_CW_line.Append("CIRCLE/" + ptc.X.ToString("F6") + ";" + ptc.Y.ToString("F6") + ";" + ptc.Z.ToString("F6") + ";" + toolAxis.X.ToString("F6") + ";" + toolAxis.Y.ToString("F6") + ";" + toolAxis.Z.ToString("F6") + ";" + Math.Abs(r_value).ToString("F9") + ";" + n1.ToString("F6") + ";" + n2.ToString("F6") + ";" + Dtool.ToString("F6") + ";" + n3.ToString("F6"));
								CM_CW_line.Append('\n') ;
								CM_CW_line.Append("GOTO/" + midArc.X.ToString("F6") + ";" + midArc.Y.ToString("F6") + ";" + midArc.Z.ToString("F6")) ;
								CM_CW_line.Append('\n') ;
								CM_CW_line.Append("CIRCLE/" + ptc.X.ToString("F6") + ";" + ptc.Y.ToString("F6") + ";" + ptc.Z.ToString("F6") + ";" + toolAxis.X.ToString("F6") + ";" + toolAxis.Y.ToString("F6") + ";" + toolAxis.Z.ToString("F6") + ";" + Math.Abs(r_value).ToString("F9") + ";" + n1.ToString("F6") + ";" + n2.ToString("F6") + ";" + Dtool.ToString("F6") + ";" + n3.ToString("F6"));
								CM_CW_line.Append('\n') ;							
								CM_CW_line.Append("GOTO/" + mx_crd.ToString("F6") + ";" + my_crd.ToString("F6") + ";" + mz_crd.ToString("F6") );
								CM_CW_line.Append('\n') ;								
							}

						}				
						
						return CM_CW_line.Replace(',','.') ;
					
				}
				
				public StringBuilder NX_helical_motion_CW(String ActPlane, MachineStatus mstat, double tolerance, MachineStatus.Axis axis1, double omx_crd, double ox_crd, MachineStatus.Axis axis2, double omy_crd, double oy_crd, MachineStatus.Axis axis3, double omz_crd, double oz_crd, double i_vct, double j_vct, double k_vct )
				{
					
					CM_CW_line.Clear() ;
					
					Vector3D vector1 = new Vector3D();  // вектор оси инструмента заданный в программе обработки
					Vector3D vectorI = new Vector3D() ;
					Vector3D vectorK = new Vector3D() ;
					Vector3D VectOrt = new Vector3D() ;
					Vector3D dVectOrt = new Vector3D() ;
					Vector3D CrossVector = new Vector3D();
					Vector3D VectTransI = new Vector3D();
					Vector3D VectTransK = new Vector3D();
					Vector3D Radius = new Vector3D() ;
					Vector3D Result = new Vector3D() ;
					Matrix3D hRotate = new Matrix3D() ; // matrix is default
					Point3D midPtFullArc = new Point3D() ;
					Point3D rPoint = new Point3D() ;
					Point3D TrPt = new Point3D() ;
					Point3D ortPt = new Point3D() ;
					List<Point3D> rPointL = new List<Point3D>() ;
					bool arcHelixMode = false ;
					int nums = 10;
					
					if (ActPlane == "G17") { vectorI = new Vector3D(0, 1, 0) ; vectorK = new Vector3D(1, 0, 0) ; vector1 = new Vector3D(0, 0, 1) ; LengthIvector = j_vct ; LengthKvector = i_vct ; }
					if (ActPlane == "G18") { vectorI = new Vector3D(1, 0, 0) ; vectorK = new Vector3D(0, 0, 1) ; vector1 = new Vector3D(0, 1, 0) ; LengthIvector = i_vct ; LengthKvector = k_vct ; }
					if (ActPlane == "G19") { vectorI = new Vector3D(0, 0, 1) ; vectorK = new Vector3D(0, 1, 0) ; vector1 = new Vector3D(1, 0, 0) ; LengthIvector = k_vct ; LengthKvector = j_vct ; }
					
					double radius = Math.Sqrt(Math.Pow((LengthIvector),2) + Math.Pow((LengthKvector),2));
					Vector3D vectorChord = new Vector3D(ox_crd - omx_crd, oy_crd - omy_crd, oz_crd - omz_crd); // вектор - хорда между начальной и конечной точкой дуги
					
					Vector3D toolAxis = new Vector3D();
					toolAxis = vector1 ;
					Vector3D toolAxisTransform = new Vector3D() ;
					toolAxisTransform = Vector3D.Multiply(vector1,mstat.mbase);  // вектора инструмента после поворота системы координат 
			//		vectorI = Vector3D.Multiply(vectorI,mstat.mbase) ;
			//		vectorK = Vector3D.Multiply(vectorK,mstat.mbase) ;
			//		hRotate = mstat.mbase ;
					double ort = Math.Round(toolAxis.X*vectorChord.X + toolAxis.Y*vectorChord.Y + toolAxis.Z*vectorChord.Z, 6) ;  // z - coordinate along in tool axis
					VectOrt = Vector3D.Multiply(toolAxis, ort) ;				
					ortPt = new Point3D(VectOrt.X + omx_crd, VectOrt.Y + omy_crd, VectOrt.Z + omz_crd ) ;
					if (ort.Equals(0)) { VectOrt = toolAxis ; ortPt = new Point3D(omx_crd, omy_crd, omz_crd) ; }
					VectTransI = Vector3D.Multiply(vectorI, LengthIvector) ;
					VectTransK = Vector3D.Multiply(vectorK, LengthKvector) ;
					
					Point3D PtIvct = new Point3D(VectTransI.X + ox_crd, VectTransI.Y + oy_crd, VectTransI.Z + oz_crd ) ;
					Point3D ptcArc = new Point3D(VectTransK.X+PtIvct.X, VectTransK.Y+PtIvct.Y, VectTransK.Z+PtIvct.Z);  // координаты точки центра окружности
					Vector3D Radius1 = new Vector3D(ox_crd - ptcArc.X, oy_crd - ptcArc.Y, oz_crd - ptcArc.Z) ;
					Vector3D Radius2 = new Vector3D(ortPt.X - ptcArc.X, ortPt.Y - ptcArc.Y, ortPt.Z - ptcArc.Z ) ;
					
					CrossVector = Vector3D.CrossProduct(vectorChord, toolAxis) ;
					if (vectorChord.Length.Equals(0)) { CrossVector = Radius1 ;}
					CrossVector.Normalize() ;
					CrossVector = Vector3D.Multiply(CrossVector, radius) ;
					VectOrt.Negate();
					VectOrt.Normalize() ;
					Point3D midPtArc = new Point3D(CrossVector.X+ptcArc.X, CrossVector.Y+ptcArc.Y, CrossVector.Z+ptcArc.Z ) ; // middle point of arc
					
					double angArc = Vector3D.AngleBetween(Radius1, Radius2) ;
					angArc = Math.Round(angArc, 6) ;
					Vector3D r1Vect = new Vector3D(ptcArc.X-ox_crd, ptcArc.Y-oy_crd, ptcArc.Z-oz_crd ) ;
					Vector3D r2Vect = new Vector3D(ptcArc.X - midPtArc.X, ptcArc.Y - midPtArc.Y, ptcArc.Z - midPtArc.Z ) ;
					double AngHalfArc = Vector3D.AngleBetween(r1Vect, r2Vect) ;
			//		AngArc = Math.Round(AngArc, 6) ;
					double AngArc = AngHalfArc*2 ;
					if (angArc.Equals(0)) { AngArc = 360.0 ;}
					double Turn = Math.Round(AngArc, 6)/360.0 ;
					double AngHelix = (Math.Atan(ort/(Turn*2*Math.PI*radius)))*180/Math.PI ;  // угол подъема витка
					
					double Alpha = Math.Acos((radius-tolerance)/radius)*180/Math.PI ;		// вычисляем число отрезков дуги
					double Num = Math.Abs((AngArc)/(2*Alpha)) ;
					if (Num < 1) {nums = 1 ;}
					if (Num > 1) 
					{
						nums = (int)Math.Truncate(Num) ;
					}
					
//	            	matrix.Rotate(new Quaternion(new Vector3D(0,1,0), 0.0 )) ;
					for (int i = 1; i <= nums ; i++)
					{
						double t = (Turn*2*Math.PI*radius*i/ (nums ));
						double dOrt = t*Math.Tan(AngHelix*Math.PI/180) ;
				//		if(!dOrt.Equals(0)) { 
						hRotate.RotateAt(new Quaternion(toolAxis, -AngArc/nums), ptcArc) ;
						Radius = Vector3D.Multiply(Radius1, hRotate) ;
						dVectOrt = Vector3D.Multiply(VectOrt, dOrt) ;
						Result = Radius + dVectOrt ;
						rPoint = new Point3D(Result.X + ptcArc.X, Result.Y + ptcArc.Y, Result.Z + ptcArc.Z ) ;
						TrPt = mstat.mbase.Transform(rPoint) ; 
						string tAxis = "tAxisX" + toolAxisTransform.X.ToString("F9") + " " + "tAxisY" + toolAxisTransform.Y.ToString("F9") + " " + "tAxisZ" + toolAxisTransform.Z.ToString("F9") ;
						CM_CW_line.Append( "G1" + " " + axis1+TrPt.X.ToString("F9") + " " + axis2+TrPt.Y.ToString("F9") + " " + axis3+TrPt.Z.ToString("F9") + " " + tAxis) ;
						CM_CW_line.Append('\n');
					}
					
//							mstat.machineCoordinates[MachineStatus.Axis.X] = rPoint.X ;
//							mstat.machineCoordinates[MachineStatus.Axis.Y] = rPoint.Y ;
//							mstat.machineCoordinates[MachineStatus.Axis.Z] = rPoint.Z ;	
							
							return CM_CW_line.Replace(',','.') ;		            
							
				}
				
				public StringBuilder NX_helical_motionCW(String ActPlane, MachineStatus mstat, double tolerance, MachineStatus.Axis axis1, double mx_crd, double x_crd, double omx_crd, double ox_crd, MachineStatus.Axis axis2, double my_crd, double y_crd, double omy_crd, double oy_crd, MachineStatus.Axis axis3, double mz_crd, double z_crd, double omz_crd, double oz_crd, double i_vct, double j_vct, double k_vct )
				{
					
					CM_CW_line.Clear() ;
					
					Vector3D vector1 = new Vector3D();  // вектор оси инструмента заданный в программе обработки
					Vector3D vectorI = new Vector3D() ;
					Vector3D vectorK = new Vector3D() ;
					Vector3D VectOrt = new Vector3D() ;
					Vector3D dVectOrt = new Vector3D() ;
					Vector3D CrossVector = new Vector3D();
					Vector3D VectTransI = new Vector3D();
					Vector3D VectTransK = new Vector3D();
					Vector3D Radius = new Vector3D() ;
					Vector3D Result = new Vector3D() ;
					Matrix3D hRotate = new Matrix3D() ; // matrix is default
					Point3D midPtFullArc = new Point3D() ;
					Point3D rPoint = new Point3D() ;
					Point3D TrPt = new Point3D() ;
					Point3D ortPt = new Point3D() ;
					List<Point3D> rPointL = new List<Point3D>() ;
					bool arcHelixMode = false ;
					int nums = 10;
					
					if (ActPlane == "G17") { vectorI = new Vector3D(0, 1, 0) ; vectorK = new Vector3D(1, 0, 0) ; vector1 = new Vector3D(0, 0, 1) ; LengthIvector = j_vct ; LengthKvector = i_vct ; }
					if (ActPlane == "G18") { vectorI = new Vector3D(1, 0, 0) ; vectorK = new Vector3D(0, 0, 1) ; vector1 = new Vector3D(0, 1, 0) ; LengthIvector = i_vct ; LengthKvector = k_vct ; }
					if (ActPlane == "G19") { vectorI = new Vector3D(0, 0, 1) ; vectorK = new Vector3D(0, 1, 0) ; vector1 = new Vector3D(1, 0, 0) ; LengthIvector = k_vct ; LengthKvector = j_vct ; }
					
					double radius = Math.Sqrt(Math.Pow((LengthIvector),2) + Math.Pow((LengthKvector),2));
					Vector3D vectorChord = new Vector3D(x_crd - mx_crd, y_crd - my_crd, z_crd - mz_crd); // вектор - хорда между начальной и конечной точкой дуги
			//		Vector3D vectorChordOriginal = new Vector3D(ox_crd - omx_crd, oy_crd - omy_crd, oz_crd - omz_crd);
					
					Vector3D toolAxis = new Vector3D();
					toolAxis = Vector3D.Multiply(vector1,mstat.mbase);  // вектора инструмента после поворота системы координат 
					vectorI = Vector3D.Multiply(vectorI,mstat.mbase) ;
					vectorK = Vector3D.Multiply(vectorK,mstat.mbase) ;
			//		hRotate = mstat.mbase ;
					double ort = Math.Round(toolAxis.X*vectorChord.X + toolAxis.Y*vectorChord.Y + toolAxis.Z*vectorChord.Z, 6) ;  // z - coordinate along in tool axis
					VectOrt = Vector3D.Multiply(toolAxis, ort) ;				
					ortPt = new Point3D(VectOrt.X + mx_crd, VectOrt.Y + my_crd, VectOrt.Z + mz_crd ) ;
					if (ort.Equals(0)) { VectOrt = toolAxis ; ortPt = new Point3D(mx_crd, my_crd, mz_crd) ; }
					VectTransI = Vector3D.Multiply(vectorI, LengthIvector) ;
					VectTransK = Vector3D.Multiply(vectorK, LengthKvector) ;
					
					Point3D PtIvct = new Point3D(VectTransI.X + x_crd, VectTransI.Y + y_crd, VectTransI.Z + z_crd ) ;
					Point3D ptcArc = new Point3D(VectTransK.X+PtIvct.X, VectTransK.Y+PtIvct.Y, VectTransK.Z+PtIvct.Z);  // координаты точки центра окружности
					Vector3D Radius1 = new Vector3D(x_crd - ptcArc.X, y_crd - ptcArc.Y, z_crd - ptcArc.Z) ;
					Vector3D Radius2 = new Vector3D(ortPt.X - ptcArc.X, ortPt.Y - ptcArc.Y, ortPt.Z - ptcArc.Z ) ;
					Point3D ptcArcTransform = new Point3D() ;
					ptcArcTransform = Point3D.Multiply(ptcArc, mstat.mbase) ;
					
					CrossVector = Vector3D.CrossProduct(vectorChord, vector1) ;
					if (vectorChord.Length.Equals(0)) { CrossVector = Radius1 ;}
					CrossVector.Normalize() ;
					CrossVector = Vector3D.Multiply(CrossVector, radius) ;
					if (ort > 0) { VectOrt.Negate(); }
					VectOrt.Normalize() ;
					Point3D midPtArc = new Point3D(CrossVector.X+ptcArc.X, CrossVector.Y+ptcArc.Y, CrossVector.Z+ptcArc.Z ) ; // middle point of arc
					
					double angArc = Vector3D.AngleBetween(Radius1, Radius2) ;
					angArc = Math.Round(angArc, 6) ;
					Vector3D r1Vect = new Vector3D(ptcArc.X-x_crd, ptcArc.Y-y_crd, ptcArc.Z-z_crd ) ;
					Vector3D r2Vect = new Vector3D(ptcArc.X - midPtArc.X, ptcArc.Y - midPtArc.Y, ptcArc.Z - midPtArc.Z ) ;

					Vector3D MainVector = new Vector3D(x_crd - ptcArcTransform.X, y_crd - ptcArcTransform.Y, z_crd - ptcArcTransform.Z) ;
					double AngHalfArc = Vector3D.AngleBetween(r1Vect, r2Vect) ;
			//		AngArc = Math.Round(AngArc, 6) ;
					double AngArc = AngHalfArc*2 ;
					
					if (angArc.Equals(0)) { AngArc = 360.0 ;}
					double Turn = Math.Round(AngArc, 6)/360.0 ;
					double AngHelix = (Math.Atan(ort/(Turn*2*Math.PI*radius)))*180/Math.PI ;  // угол подъема витка
					
					double Alpha = Math.Acos((radius-tolerance)/radius)*180/Math.PI ;		// вычисляем число отрезков дуги
					double Num = Math.Abs((AngArc)/(2*Alpha)) ;
					if (Num < 1) {nums = 1 ;}
					if (Num > 1) 
					{
						nums = (int)Math.Truncate(Num) ;
					}
					
//	            	matrix.Rotate(new Quaternion(new Vector3D(0,1,0), 0.0 )) ;
					for (int i = 1; i <= nums ; i++)
					{
						double t = (Turn*2*Math.PI*radius*i/ (nums ));
						double dOrt = t*Math.Tan(AngHelix*Math.PI/180) ;
				//		if(!dOrt.Equals(0)) { 
						hRotate.RotateAt(new Quaternion(toolAxis, -AngArc/nums), ptcArc) ;
						Radius = Vector3D.Multiply(Radius1, hRotate) ;
						dVectOrt = Vector3D.Multiply(VectOrt, dOrt) ;
						Result = Radius + dVectOrt ;
						rPoint = new Point3D(Result.X + ptcArc.X, Result.Y + ptcArc.Y, Result.Z + ptcArc.Z ) ;
						string tAxis = "tAxisX" + toolAxis.X.ToString("F9") + " " + "tAxisY" + toolAxis.Y.ToString("F9") + " " + "tAxisZ" + toolAxis.Z.ToString("F9") ;
				//		CM_CW_line.Append( "G1" + " " + axis1+rPoint.X.ToString("F9") + " " + axis2+rPoint.Y.ToString("F9") + " " + axis3+rPoint.Z.ToString("F9") + " " + tAxis) ;
						CM_CW_line.Append("GOTO/" + rPoint.X.ToString("F9") + ";" + rPoint.Y.ToString("F9") + ";" + rPoint.Z.ToString("F9") + ";" + toolAxis.X.ToString("F9") + ";" + toolAxis.Y.ToString("F9") + ";" + toolAxis.Z.ToString("F9")) ;
						CM_CW_line.Append('\n');
					}
					
//							mstat.machineCoordinates[MachineStatus.Axis.X] = rPoint.X ;
//							mstat.machineCoordinates[MachineStatus.Axis.Y] = rPoint.Y ;
//							mstat.machineCoordinates[MachineStatus.Axis.Z] = rPoint.Z ;	
							
							return CM_CW_line.Replace(',','.') ;		            
							
				}				
				
				
				
				public void HelicalMotion(Vector3D Radius1, double radius, double ort, double tolerance, double AngArc, Point3D ptcArc, Vector3D toolAxis, int RotDirection)
				{
					
					int num = 10;
					var axis1 = MachineStatus.Axis.X ;
					var axis2 = MachineStatus.Axis.Y ;
					var axis3 = MachineStatus.Axis.Z ;
					Vector3D VectOrt = new Vector3D() ;
					Vector3D Radius = new Vector3D() ;
					Vector3D dVectOrt = new Vector3D() ;
					Vector3D Result = new Vector3D() ;
					Point3D rPoint = new Point3D() ;
					VectOrt = Vector3D.Multiply(toolAxis, ort) ;
					VectOrt.Negate(); 
					VectOrt.Normalize() ;
					Matrix3D hRotate = new Matrix3D() ;
					double Turn = Math.Round(AngArc, 6)/360.0 ;
					double AngHelix = (Math.Atan(ort/(Turn*2*Math.PI*radius)))*180/Math.PI ;
					double Alpha = Math.Acos((radius-tolerance)/radius)*180/Math.PI ;		// вычисляем число отрезков дуги
					double Num = Math.Abs((AngArc)/(2*Alpha)) ;
					if (Num < 1) {num = 1 ;}
					if (Num > 1) 
					{
						num = (int)Math.Truncate(Num) ;
					}
					
					for (int i = 1; i <= num ; i++)
					{
						double t = (Turn*2*Math.PI*radius*i/ (num ));
						double dOrt = t*Math.Tan(AngHelix*Math.PI/180) ;
				//		if(!dOrt.Equals(0)) { 
						hRotate.RotateAt(new Quaternion(toolAxis, RotDirection*(AngArc/num)), ptcArc) ;
						Radius = Vector3D.Multiply(Radius1, hRotate) ;
						dVectOrt = Vector3D.Multiply(VectOrt, dOrt) ;
						Result = Radius + dVectOrt ;
						rPoint = new Point3D(Result.X + ptcArc.X, Result.Y + ptcArc.Y, Result.Z + ptcArc.Z ) ;
						
				//		string tAxis = "tAxisX" + toolAxis.X.ToString("F9") + " " + "tAxisY" + toolAxis.Y.ToString("F9") + " " + "tAxisZ" + toolAxis.Z.ToString("F9") ;
				//		CM_CW_line.Append( "G1" + " " + axis1+rPoint.X.ToString("F9") + " " + axis2+rPoint.Y.ToString("F9") + " " + axis3+rPoint.Z.ToString("F9") + " " + tAxis) ;
						CM_CW_line.Append("GOTO/" + rPoint.X.ToString("F6") + ";" + rPoint.Y.ToString("F6") + ";" + rPoint.Z.ToString("F6") + ";" + toolAxis.X.ToString("F6") + ";" + toolAxis.Y.ToString("F6") + ";" + toolAxis.Z.ToString("F6")) ;
						CM_CW_line.Append('\n');
					}
					
						CM_CW_line.Replace(',','.') ;
					
				}

				public void HelicalMotion(Vector3D Radius1, double radius, double ort, double tolerance, double AngArc, Point3D ptcArc, Vector3D toolAxis, int RotDirection, double turn)
				{
					
					int num = 10;
					var axis1 = MachineStatus.Axis.X ;
					var axis2 = MachineStatus.Axis.Y ;
					var axis3 = MachineStatus.Axis.Z ;
					Vector3D VectOrt = new Vector3D() ;
					Vector3D Radius = new Vector3D() ;
					Vector3D dVectOrt = new Vector3D() ;
					Vector3D Result = new Vector3D() ;
					Point3D rPoint = new Point3D() ;
					VectOrt = Vector3D.Multiply(toolAxis, ort) ;
					VectOrt.Negate(); 
					VectOrt.Normalize() ;
					Matrix3D hRotate = new Matrix3D() ;
					double Turn = Math.Round(AngArc, 6)/360.0 ;
					if (!turn.Equals(0)) { Turn = turn ;} 
					double AngHelix = (Math.Atan(ort/(Turn*2*Math.PI*radius)))*180/Math.PI ;
					double Alpha = Math.Acos((radius-tolerance)/radius)*180/Math.PI ;		// вычисляем число отрезков дуги
					double Num = Math.Abs((AngArc)/(2*Alpha)) ;
					if (Num < 1) {num = 1 ;}
					if (Num > 1) 
					{
						num = (int)Math.Truncate(Num) ;
					}
					
					for (int i = 1; i <= num ; i++)
					{
						double t = (Turn*2*Math.PI*radius*i/ (num ));
						double dOrt = t*Math.Tan(AngHelix*Math.PI/180) ;
				//		if(!dOrt.Equals(0)) { 
						hRotate.RotateAt(new Quaternion(toolAxis, RotDirection*(AngArc/num)), ptcArc) ;
						Radius = Vector3D.Multiply(Radius1, hRotate) ;
						dVectOrt = Vector3D.Multiply(VectOrt, dOrt) ;
						Result = Radius + dVectOrt ;
						rPoint = new Point3D(Result.X + ptcArc.X, Result.Y + ptcArc.Y, Result.Z + ptcArc.Z ) ;
						
				//		string tAxis = "tAxisX" + toolAxis.X.ToString("F9") + " " + "tAxisY" + toolAxis.Y.ToString("F9") + " " + "tAxisZ" + toolAxis.Z.ToString("F9") ;
				//		CM_CW_line.Append( "G1" + " " + axis1+rPoint.X.ToString("F9") + " " + axis2+rPoint.Y.ToString("F9") + " " + axis3+rPoint.Z.ToString("F9") + " " + tAxis) ;
						CM_CW_line.Append("GOTO/" + rPoint.X.ToString("F6") + ";" + rPoint.Y.ToString("F6") + ";" + rPoint.Z.ToString("F6") + ";" + toolAxis.X.ToString("F6") + ";" + toolAxis.Y.ToString("F6") + ";" + toolAxis.Z.ToString("F6")) ;
						CM_CW_line.Append('\n');
					}
					
						CM_CW_line.Replace(',','.') ;
					
				}				
				
				public StringBuilder NX_ROT_AX_A_TABLE(String motionMode, String ActPlane, MachineStatus mstat, double tolerance, MachineStatus.Axis AxisRot, double mphi, double pphi, MachineStatus.Axis axis1 ,double mx, double px, double omx, double opx, MachineStatus.Axis axis2, double my, double py, double omy, double opy, MachineStatus.Axis axis3, double mz, double pz, double omz, double opz )
				{
					CM_CW_line.Clear() ;
					Vector3D toolAxis = mstat.ToolAxis ;
        			Point3D OPtp = new Point3D(opx,opy,opz);
        			Point3D OPtm = new Point3D(omx, omy, omz) ;
        			Point3D tmpPt = new Point3D();
        			double Ang = 0 ;
					
// calculate shortest arc 	

					double AngCW = mphi - (pphi - 360) ;
					if (AngCW > 360.0) { AngCW = AngCW - 360.0 ;}
					double AngCCW = mphi - (360.0 + pphi) ;
					if (AngCCW < -360.0) { AngCCW = AngCCW + 360.0 ;}
					if ( Math.Abs(AngCW) < Math.Abs(AngCCW) ) { Ang = AngCW ;}
					if ( Math.Abs(AngCW) > Math.Abs(AngCCW) ) { Ang = AngCCW ;}
					if (Math.Abs(AngCW).Equals(Math.Abs(AngCCW))) {Ang = AngCW ;}
					if (Math.Abs(Ang).Equals(360)) { Ang = 0.0000001 ;}
					
					double RotAng = Ang ;					
//					if (Ang >= 0 && (pphi).Equals(0.0) ) { Ang = Ang + Ang1 ; }	
					double Turn = Ang/360.0 ;					
					if (Turn.Equals(0.0)) { return CM_CW_line.Clear() ; }					
        		//	Vector3D toolAxis = new Vector3D(0,0,1) ;
				
					if (AxisRot.Equals(MachineStatus.Axis.A)) 
					{
						double Phi1 = Vector3D.AngleBetween(new Vector3D(0,OPtp.Y,OPtp.Z), new Vector3D(0,1,0)); 
						double Phi2 = Vector3D.AngleBetween(new Vector3D(0,OPtp.Y,OPtp.Z), new Vector3D(0,0,1));
						if (Phi1.Equals(Double.NaN))
						{
							Phi1 = Vector3D.AngleBetween(new Vector3D(0, OPtm.Y,OPtm.Z), new Vector3D(0,1,0));
							Phi2 = Vector3D.AngleBetween(new Vector3D(0, OPtm.Y,OPtm.Z), new Vector3D(0,0,1));							
						}
						Phi0 = Phi1 ;
						if(Phi2 > 90.0 ) { Phi0 = -Phi1 ; }
						R0 = Math.Sqrt(OPtp.Y*OPtp.Y + OPtp.Z*OPtp.Z) ;
						R1 = Math.Sqrt(OPtm.Y*OPtm.Y + OPtm.Z*OPtm.Z) ;
						Rmid = (R0+R1)/2 ;
						if (R1.Equals(0.0)) { R1 = R0 ; }
						if (R0.Equals(0.0) && R1.Equals(0.0)) 
						{
							mstat.mbase.Rotate(new Quaternion(new Vector3D(1,0,0), RotAng)) ; 
							return CM_CW_line.Clear() ; 
						}
						Height = ((OPtp.X + OPtm.X) - 2*OPtp.X) ;
						double Alpha = Math.Acos((Rmid-tolerance)/Rmid)*180/Math.PI ;
						double Num = Math.Abs((Ang)/(2*Alpha)) ;
						if (Num < 1) {num = 1 ;}
						if (Num > 1) 
						{
							num = (int)Math.Truncate(Num) ;
						}
						Hst = OPtp.X ;
						
					}

		            double a = (R1-R0)/Turn ; // шаг спирали архимеда
					double h = Height/Turn; // высота витка
			
		            for (int i = 0; i < num; i++)
		            {						
		            	double t = (Turn*2*Math.PI* i/ (num - 1));		            	// угол подъема витка единичной длины
		            	
		            	if (t.Equals(Double.NaN)) 
		            	{
		            		mstat.mbase.Rotate(new Quaternion(new Vector3D(1,0,0), RotAng)) ; // Rotate MCS after short Arc
		            		mToolAx.Rotate(new Quaternion(new Vector3D(1,0,0), RotAng)) ; // Rotate ToolAx		            		
		            		trPt = mstat.mbase.Transform(OPtm) ;
		            		tmpPt = OPtm ;
							mstat.machineCoordinates[MachineStatus.Axis.X] = trPt.X ;
							mstat.machineCoordinates[MachineStatus.Axis.Y] = trPt.Y ;
							mstat.machineCoordinates[MachineStatus.Axis.Z] = trPt.Z ;			            		
		            	}
		            	else
		            	{
		            		double q = (h*t)/(2*Math.PI) ;
		
		            		pt = new Point3D((R0+(a*t)/(2*Math.PI))*Math.Cos((Phi0*Math.PI/180)+t), (R0+(a*t)/(2*Math.PI))*Math.Sin((Phi0*Math.PI/180)+t), q+Hst);
		            		tmpPt.Z = pt.Y ; tmpPt.X = pt.Z ; tmpPt.Y = pt.X ;
		            		trPt = mstat.mbase.Transform(tmpPt) ;
		            		mToolAx.Rotate(new Quaternion(new Vector3D(1,0,0), RotAng/num)) ;

		            	}

		          			if(ActPlane == "G19") { toolAxis = Vector3D.Multiply(new Vector3D(1,0,0), mToolAx)  ; }
		           			if(ActPlane == "G18") { toolAxis = Vector3D.Multiply(new Vector3D(0,1,0), mToolAx)  ; }
		           			if(ActPlane == "G17") { toolAxis = Vector3D.Multiply(new Vector3D(0,0,1), mToolAx)  ; }
		           			if (toolAxis.Length.Equals(0.0)) { toolAxis = mstat.ToolAxis ;}
		     //       		toolAxis = Vector3D.Multiply(toolAxis, mstat.mbase);		     				
		            		toolAxis.Normalize();
							mstat.ToolAxis = toolAxis ;
		            					
		            		string tAxis = "tAxisX" + toolAxis.X.ToString("F9") + " " + "tAxisY" + toolAxis.Y.ToString("F9") + " " + "tAxisZ" + toolAxis.Z.ToString("F9") ;
		            //		CM_CW_line.Append(motionMode + " " + axis1+trPt.X.ToString("F9") + " " + axis2+trPt.Y.ToString("F9") + " " + axis3+trPt.Z.ToString("F9") + " " + tAxis) ;
		            		CM_CW_line.Append("GOTO/" + trPt.X.ToString("F9") + ";" + trPt.Y.ToString("F9") + ";" + trPt.Z.ToString("F9") + ";" + toolAxis.X.ToString("F9") + ";" + toolAxis.Y.ToString("F9") + ";" + toolAxis.Z.ToString("F9")) ;
							CM_CW_line.Append('\n');
		            }
		            
					// send your last position
							if (num > 1) 
							{
								mstat.mbase.Rotate(new Quaternion(new Vector3D(1,0,0), RotAng)) ; // Rotate MCS after Long Arc
								mstat.machineCoordinates[MachineStatus.Axis.X] = trPt.X ;
								mstat.machineCoordinates[MachineStatus.Axis.Y] = trPt.Y ;
								mstat.machineCoordinates[MachineStatus.Axis.Z] = trPt.Z ;
							}
							
							
							return CM_CW_line.Replace(',','.');
					
				}

				public StringBuilder NX_ROT_AX_B_TABLE(String motionMode, String ActPlane, MachineStatus mstat, double tolerance, MachineStatus.Axis AxisRot, double mphi, double pphi, MachineStatus.Axis axis1 ,double mx, double px, double omx, double opx, MachineStatus.Axis axis2, double my, double py, double omy, double opy, MachineStatus.Axis axis3, double mz, double pz, double omz, double opz )
				{
					CM_CW_line.Clear() ;
					
//        			double Turn = Ang/360.0 ; // число витков
					Vector3D toolAxis = mstat.ToolAxis ;
					mToolAx = mstat.mbase ;
        			Point3D OPtp = new Point3D(opx,opy,opz);
        			Point3D OPtm = new Point3D(omx, omy, omz) ;
        			Point3D Ptm = new Point3D(mx,my,mz) ;
        			Point3D toolPt = new Point3D();
        			Point3D tmpPt = new Point3D();
        			Vector3D projectVct1 ;
        			double Ang = 0 ;

// calculate shortest arc 	

					double AngCW = mphi - (pphi - 360) ;
					if (AngCW > 360.0) { AngCW = AngCW - 360.0 ;}
					double AngCCW = mphi - (360.0 + pphi) ;
					if (AngCCW < -360.0) { AngCCW = AngCCW + 360.0 ;}
					if ( Math.Abs(AngCW) < Math.Abs(AngCCW) ) { Ang = AngCW ;}
					if ( Math.Abs(AngCW) > Math.Abs(AngCCW) ) { Ang = AngCCW ;}
					if (Math.Abs(AngCW).Equals(Math.Abs(AngCCW))) { Ang = AngCW ;}   // +/- 180.0
					if (Math.Abs(Ang).Equals(360)) { Ang = 0.0000001 ;}
					
					double RotAng = Ang ;
					double Turn = Ang/360.0 ;					
					if (Turn.Equals(0.0)) { return CM_CW_line.Clear() ; }

        		//	Vector3D toolAxis = new Vector3D(0,0,1) ;      		
				
					if (AxisRot.Equals(MachineStatus.Axis.B)) 
					{
						double Phi1 = Vector3D.AngleBetween(new Vector3D(OPtp.X,0,OPtp.Z), new Vector3D(0,0,1)); 
						double Phi2 = Vector3D.AngleBetween(new Vector3D(OPtp.X,0,OPtp.Z), new Vector3D(1,0,0));
						if (Phi1.Equals(Double.NaN)) 
						{
							Phi1 = Vector3D.AngleBetween(new Vector3D(OPtm.X,0,OPtm.Z), new Vector3D(0,0,1));
							Phi2 = Vector3D.AngleBetween(new Vector3D(OPtm.X,0,OPtm.Z), new Vector3D(1,0,0));							
						}
						Phi0 = Phi1 ;
						if(Phi2 > 90.0 ) { Phi0 = -Phi1 ; }
						R0 = Math.Sqrt(OPtp.Z*OPtp.Z + OPtp.X*OPtp.X) ;
						R1 = Math.Sqrt(OPtm.Z*OPtm.Z + OPtm.X*OPtm.X) ;
						Rmid = (R0+R1)/2 ;
						if (R1.Equals(0.0)) { R1 = R0 ; }
						if (R0.Equals(0.0) && R1.Equals(0.0)) 
						{
							mstat.mbase.Rotate(new Quaternion(new Vector3D(0,1,0), RotAng)) ; 
							return CM_CW_line.Clear() ; 
						}
						Height = ((OPtp.Y + OPtm.Y) - 2*OPtp.Y) ;
						double Alpha = Math.Acos((Rmid-tolerance)/Rmid)*180/Math.PI ;
						double Num = Math.Abs((Ang)/(2*Alpha)) ;
						if (Num < 1) {num = 1 ;}
						if (Num > 1) 
						{
							num = (int)Math.Truncate(Num) ;
						}
						Hst = OPtp.Y ;
					}

		            double a = (R1-R0)/Turn ; // шаг спирали архимеда
					double h = Height/Turn; // высота витка
					
		            for (int i = 0; i < num; i++)
		            {
		            	
		            	double t = (Turn*2*Math.PI* i/ (num - 1));		            	// угол подъема витка единичной длины
		            	
		            	// если дуга задана единичным отрезком
		            	if (t.Equals(Double.NaN)) 
		            	{
		            		mstat.mbase.Rotate(new Quaternion(new Vector3D(0,1,0), RotAng)) ; // Rotate MCS after short Arc
		            		mToolAx.Rotate(new Quaternion(new Vector3D(0,1,0), RotAng)) ; // Rotate ToolAx
		            		trPt = mstat.mbase.Transform(OPtm) ;
		            		tmpPt = OPtm ;
							mstat.machineCoordinates[MachineStatus.Axis.X] = trPt.X ;
							mstat.machineCoordinates[MachineStatus.Axis.Y] = trPt.Y ;
							mstat.machineCoordinates[MachineStatus.Axis.Z] = trPt.Z ;		            		
		            		
		            	}
		            	else
		            	{
		            		double q = (h*t)/(2*Math.PI) ;
		        			
		            		pt = new Point3D((R0+(a*t)/(2*Math.PI))*Math.Cos((Phi0*Math.PI/180)+t), (R0+(a*t)/(2*Math.PI))*Math.Sin((Phi0*Math.PI/180)+t), q+Hst);
		            		tmpPt.Z = pt.X ; tmpPt.X = pt.Y ; tmpPt.Y = pt.Z ; toolPt = tmpPt ;
		            		trPt = mstat.mbase.Transform(tmpPt) ;
		            		mToolAx.Rotate(new Quaternion(new Vector3D(0,1,0), RotAng/num)) ;
		         //			trPt = tmpPt ;
		            	}
		            	
		          			if(ActPlane == "G19") { toolAxis = Vector3D.Multiply(new Vector3D(1,0,0), mToolAx)  ; }
		           			if(ActPlane == "G18") { toolAxis = Vector3D.Multiply(new Vector3D(0,1,0), mToolAx)  ; }
		           			if(ActPlane == "G17") { toolAxis = Vector3D.Multiply(new Vector3D(0,0,1), mToolAx)  ; }
		           			if (toolAxis.Length.Equals(0.0)) { toolAxis = mstat.ToolAxis ;}
		     //       		toolAxis = Vector3D.Multiply(toolAxis, mstat.mbase);		     				
		            		toolAxis.Normalize();
							mstat.ToolAxis = toolAxis ;
		            		
		            		string tAxis = "tAxisX" + toolAxis.X.ToString("F9") + " " + "tAxisY" + toolAxis.Y.ToString("F9") + " " + "tAxisZ" + toolAxis.Z.ToString("F9") ;
		          //  		CM_CW_line.Append(motionMode + " " + axis1+trPt.X.ToString("F9") + " " + axis2+trPt.Y.ToString("F9") + " " + axis3+trPt.Z.ToString("F9") + " " + tAxis) ;
		          			CM_CW_line.Append("GOTO/" + trPt.X.ToString("F9") + ";" + trPt.Y.ToString("F9") + ";" + trPt.Z.ToString("F9") + ";" + toolAxis.X.ToString("F9") + ";" + toolAxis.Y.ToString("F9") + ";" + toolAxis.Z.ToString("F9")) ;
							CM_CW_line.Append('\n');
		            }
					// send your last position
			
							if (num > 1) 
							{
								mstat.mbase.Rotate(new Quaternion(new Vector3D(0,1,0), RotAng)) ; // Rotate MCS after Long Arc
								mstat.machineCoordinates[MachineStatus.Axis.X] = trPt.X ;
								mstat.machineCoordinates[MachineStatus.Axis.Y] = trPt.Y ;
								mstat.machineCoordinates[MachineStatus.Axis.Z] = trPt.Z ;
							}
							
							return CM_CW_line.Replace(',','.') ;
					
				}				

				
				public StringBuilder NX_ROT_AX_C_TABLE(String motionMode, String ActPlane, MachineStatus mstat, double tolerance, MachineStatus.Axis AxisRot, double mphi, double pphi, MachineStatus.Axis axis1 ,double mx, double px, double omx, double opx, MachineStatus.Axis axis2, double my, double py, double omy, double opy, MachineStatus.Axis axis3, double mz, double pz, double omz, double opz)
				{
					CM_CW_line.Clear() ;
	
        		//	Point3D trPt = new Point3D() ;
        			Point3D tmpPt = new Point3D();
        			Vector3D toolAxis = mstat.ToolAxis ;
        			Point3D OPtp = new Point3D(opx,opy,opz);
        			Point3D OPtm = new Point3D(omx, omy, omz) ;
        			Vector3D projectVct1 ;
        			Vector3D vct1 = new Vector3D();
					double Ang = 0;
// calculate shortest arc 	

					double AngCW = mphi - (pphi - 360) ;
					if (AngCW > 360.0) { AngCW = AngCW - 360.0 ;}
					double AngCCW = mphi - (360.0 + pphi) ;
					if (AngCCW < -360.0) { AngCCW = AngCCW + 360.0 ;}
					if ( Math.Abs(AngCW) < Math.Abs(AngCCW) ) { Ang = AngCW ;}
					if ( Math.Abs(AngCW) > Math.Abs(AngCCW) ) { Ang = AngCCW ;}
					if (Math.Abs(AngCW).Equals(Math.Abs(AngCCW))) {Ang = AngCW ;}
					if (Math.Abs(Ang).Equals(360)) { Ang = 0.0000001 ;}					
					
//					double Ang = Ang1 + mphi - (Ang1 + pphi) ;  // текущий угол поворота нового витка от предыдущего положения
					double RotAng = Ang ;
//					if (Ang >= 0 && (pphi).Equals(0.0) ) { Ang = Ang + Ang1 ; }	
					double Turn = Ang/360.0 ; // длина витка в оборотах
					if (Turn.Equals(0.0)) { return CM_CW_line.Clear() ; }
					
 					        			
					if (AxisRot.Equals(MachineStatus.Axis.C))
					{ 

// calculate offset angle of start point
						double Phi1 = Vector3D.AngleBetween(new Vector3D(OPtp.X, OPtp.Y,0), new Vector3D(1,0,0)); 
						double Phi2 = Vector3D.AngleBetween(new Vector3D(OPtp.X, OPtp.Y,0), new Vector3D(0,1,0));
						if (Phi1.Equals(Double.NaN)) 
						{
							Phi1 = Vector3D.AngleBetween(new Vector3D(OPtm.X,OPtm.Y,0), new Vector3D(1,0,0));
							Phi2 = Vector3D.AngleBetween(new Vector3D(OPtm.X,OPtm.Y,0), new Vector3D(0,1,0));							
						}
						Phi0 = Phi1 ;
						if(Phi2 > 90.0 ) { Phi0 = -Phi1 ; }
// calculate start and finish diametr of spiral						
						R0 = Math.Sqrt((OPtp.X*OPtp.X + OPtp.Y*OPtp.Y)) ;
						R1 = Math.Sqrt((OPtm.X*OPtm.X + OPtm.Y*OPtm.Y)) ;
						Rmid = (R0+R1)/2 ;
						if (R1.Equals(0.0)) { R1 = R0 ; }
						if (R0.Equals(0.0) && R1.Equals(0.0)) 
						{
							mstat.mbase.Rotate(new Quaternion(new Vector3D(0,0,1), RotAng)) ; 
							return CM_CW_line.Clear() ; 
						}
// calculate Height and quantity lines of spiral						
						Height = ((OPtp.Z + OPtm.Z) - 2*OPtp.Z) ;
						double Alpha = Math.Acos((Rmid-tolerance)/Rmid)*180/Math.PI ;
						double Num = Math.Abs((Ang)/(2*Alpha)) ;
						if (Num < 1) {num = 1 ;}
						if (Num > 1) 
						{
							num = (int)Math.Truncate(Num) ;
						}
						
						Hst = OPtp.Z ;
					}

		            double a = (R1-R0)/Turn ; // шаг спирали архимеда
					double h = Height/Turn; // высота витка
					

		            for (int i = 0; i < num; i++)
		            {			

		            	double t = (Turn*2*Math.PI* i/ (num - 1));            	// угол подъема витка единичной длины
		            	if (t.Equals(Double.NaN)) 
		            	{
		            		mstat.mbase.Rotate(new Quaternion(new Vector3D(0,0,1), RotAng)) ; // Rotate MCS after short Arc
		            		mToolAx.Rotate(new Quaternion(new Vector3D(0,0,1), RotAng)) ; // Rotate ToolAx		            		
		            		trPt = mstat.mbase.Transform(OPtm) ;
		            		tmpPt = OPtm ;
		            		mstat.machineCoordinates[MachineStatus.Axis.X] = trPt.X ;
							mstat.machineCoordinates[MachineStatus.Axis.Y] = trPt.Y ;
							mstat.machineCoordinates[MachineStatus.Axis.Z] = trPt.Z ;
		            	}
		            	else
		            	{
		            		double q = (h*t)/(2*Math.PI) ;
		
		            		pt = new Point3D((R0+(a*t)/(2*Math.PI))*Math.Cos((Phi0*Math.PI/180)+t), (R0+(a*t)/(2*Math.PI))*Math.Sin((Phi0*Math.PI/180)+t), q+Hst);
		            		trPt = mstat.mbase.Transform(pt) ;
		            		tmpPt = pt ;
		            		mToolAx.Rotate(new Quaternion(new Vector3D(0,0,1), RotAng/num)) ;
		            	}
		            	
		          			if(ActPlane == "G19") { toolAxis = Vector3D.Multiply(new Vector3D(1,0,0), mToolAx)  ; }
		           			if(ActPlane == "G18") { toolAxis = Vector3D.Multiply(new Vector3D(0,1,0), mToolAx)  ; }
		           			if(ActPlane == "G17") { toolAxis = Vector3D.Multiply(new Vector3D(0,0,1), mToolAx)  ; }
		           			if (toolAxis.Length.Equals(0.0)) { toolAxis = mstat.ToolAxis ;}
		     //       		toolAxis = Vector3D.Multiply(toolAxis, mstat.mbase);		     				
		            		toolAxis.Normalize();
							mstat.ToolAxis = toolAxis ;
		            	
							string tAxis = "tAxisX" + toolAxis.X.ToString("F9") + " " + "tAxisY" + toolAxis.Y.ToString("F9") + " " + "tAxisZ" + toolAxis.Z.ToString("F9") ;
		          //  		CM_CW_line.Append(motionMode + " " + axis1+trPt.X.ToString("F9") + " " + axis2+trPt.Y.ToString("F9") + " " + axis3+trPt.Z.ToString("F9") + " " + tAxis) ;
		          			CM_CW_line.Append("GOTO/" + trPt.X.ToString("F9") + "," + trPt.Y.ToString("F9") + "," + trPt.Z.ToString("F9") + "," + toolAxis.X.ToString("F9") + "," + toolAxis.Y.ToString("F9") + "," + toolAxis.Z.ToString("F9")) ;
							CM_CW_line.Append('\n');
		            }
		            
					// send your last position
							if (num > 1) 
							{
								mstat.mbase.Rotate(new Quaternion(new Vector3D(0,0,1), RotAng)) ; // Rotate MCS after Long Arc
								mstat.machineCoordinates[MachineStatus.Axis.X] = trPt.X ;
								mstat.machineCoordinates[MachineStatus.Axis.Y] = trPt.Y ;
								mstat.machineCoordinates[MachineStatus.Axis.Z] = trPt.Z ;
							}
									           	 	
		           		
					//	return CM_CW_line.Replace(',','.') ;
					return CM_CW_line.Replace(',','.') ;
					
				}				
										
	}
}
