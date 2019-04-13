/*
 * Создано в SharpDevelop.
 * Пользователь: P.Dubovikov
 * Дата: 30.09.2016
 * Время: 8:26
 * 
 * Для изменения этого шаблона используйте меню "Инструменты | Параметры | Кодирование | Стандартные заголовки".
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text ;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media.Media3D ;
using gcodeparser.gcodes;


namespace gcodeparser
{

//	public class Program
//	{
//				
//		public Program()
//		{
//			
//			StreamReader sr = new StreamReader("C:\\tmp\\ParamProg840T.txt");
//			StringBuilder strb = new StringBuilder(sr.ReadToEnd()) ;
//		//	MachineControl_S840D mcontrol = new MachineControl_S840D();
//		//	GCodeParser parser = new GCodeParser(null, strb, mcontrol);
//			MachineControl_CLS mcontrolCLS = new MachineControl_CLS() ;
//			GCodeParser parser = new GCodeParser(null, strb, mcontrolCLS) ;
//			File.WriteAllText("C:\\CS\\NXEditor\\Program_S840.cls", mcontrolCLS.SCM_CW.ToString().Replace(';',',')) ;
//		}
//
//	}
	
/*			public class MachineControl_S840D : MachineController
			{
				MachineStatusHelper machine = new MachineStatusHelper();							
				MotionHelper motion = new MotionHelper();
				private StringBuilder SCM_CW = new StringBuilder();
				private double[] toolAxis = new double[3];
		//		private enum Axis {A,B,C,U,V,W,X,Y,Z} ;
				public double Tolerance {get ; set ;}
						
			
				public virtual void startBlock(GCodeParser parser, MachineStatus machineStatus, IDictionary<String, ParsedWord> currentBlock)
				{
           			machine.setMachineStatus(machineStatus);
           			ISet<string> stats = machineStatus.getModals();
		 		}
			
				public virtual void endBlock(GCodeParser parser, MachineStatus machineStatus, IDictionary<String, ParsedWord> currentBlock) 
				{
			
					String currentLine = parser.getCurrentLine() ;
					String MachiningPlane = machine.getActivePlane()  ;
					String motionMode = machine.getMotionMode() ;
					bool helixMode = false;
					Vector3D toolAx = machineStatus.ToolAxis ;
					toolAx = motion.GetToolAxis(MachiningPlane, machineStatus);
					
					if (parser.findWordInBlock(new StringBuilder(currentLine)) != null)
					{	
						if(currentBlock.ContainsKey("A") || currentBlock.ContainsKey("B") || currentBlock.ContainsKey("C"))
						{
						 	helixMode = true;
						}
						else
						{
							helixMode = machineStatus.rotAxMode ;
						}
						
						if (currentBlock.ContainsKey("G91")||currentBlock.ContainsKey("G90"))
						{
							Console.WriteLine(machine.getDistanceMode() );
				//			SCM_CW.Append(machine.getDistanceMode() + '\n') ;
							
						}

						if (currentBlock.ContainsKey("S"))
						{
					//		Console.WriteLine("S" + machine.getSpeed() ) ;
						}
						
						if (currentBlock.ContainsKey("C") )
						{
							Tolerance = 0.03 ;
							string CM_CW_string = motion.NX_ROT_AX_C_TABLE(motionMode, MachiningPlane, machineStatus, Tolerance, MachineStatus.Axis.C, machine.getMC(), machine.getC(), MachineStatus.Axis.X, machine.getMX(), machine.getX(), machine.getOMX(), machine.getOX(), MachineStatus.Axis.Y, machine.getMY(), machine.getY(), machine.getOMY(), machine.getOY(), MachineStatus.Axis.Z, machine.getMZ(), machine.getZ(), machine.getOMZ(), machine.getOZ()).ToString() ;
									SCM_CW.Append(CM_CW_string ) ;
													
						}
						if (currentBlock.ContainsKey("A") )
						{
							Tolerance = 0.03 ;
							string CM_CW_string = motion.NX_ROT_AX_A_TABLE(motionMode, MachiningPlane, machineStatus, Tolerance, MachineStatus.Axis.A, machine.getMA(), machine.getA(), MachineStatus.Axis.X, machine.getMX(), machine.getX(), machine.getOMX(), machine.getOX(), MachineStatus.Axis.Y, machine.getMY(), machine.getY(), machine.getOMY(), machine.getOY(), MachineStatus.Axis.Z, machine.getMZ(), machine.getZ(), machine.getOMZ(), machine.getOZ()).ToString() ;
									SCM_CW.Append(CM_CW_string ) ;							
						}
						if (currentBlock.ContainsKey("B") )
						{
							Tolerance = 0.03 ;
							string CM_CW_string = motion.NX_ROT_AX_B_TABLE(motionMode, MachiningPlane, machineStatus, Tolerance, MachineStatus.Axis.B, machine.getMB(), machine.getB(), MachineStatus.Axis.X, machine.getMX(), machine.getX(), machine.getOMX(), machine.getOX(), MachineStatus.Axis.Y, machine.getMY(), machine.getY(), machine.getOMY(), machine.getOY(), MachineStatus.Axis.Z, machine.getMZ(), machine.getZ(), machine.getOMZ(), machine.getOZ()).ToString() ;
									SCM_CW.Append(CM_CW_string ) ;							
						}						
						
						if (motionMode == "G1" && helixMode == false )						
						{
								String tAxis = "tAxisX" + toolAx.X.ToString("F9") + " " + "tAxisY" + toolAx.Y.ToString("F9") + " " + "tAxisZ" + toolAx.Z.ToString("F9") ;		
							//	Console.WriteLine("ValueX: " + machineStatus.Value.X + " " + "ValueY: " + machineStatus.Value.Y + " " + "ValueZ: " + machineStatus.Value.Z )
								SCM_CW.Append(motionMode + " " + MachineStatus.Axis.X+machine.getMX().ToString("F9") + " " + MachineStatus.Axis.Y+machine.getMY().ToString("F9") + " " + MachineStatus.Axis.Z+machine.getMZ().ToString("F9") + " " + MachineStatus.ModalVars.F+machine.getFeedrate() + " " + tAxis);
								Console.WriteLine("getMX(): " + machine.getMX() + " " + "getMY(): " + machine.getMY() + " " + "getMZ(): " + machine.getMZ() ) ;
							
								SCM_CW.Append('\n').Replace(',','.') ;
	
						}
						if (motionMode == "G0" && helixMode == false)
						{
							String tAxis = "tAxisX" + toolAx.X.ToString("F9") + " " + "tAxisY" + toolAx.Y.ToString("F9") + " " + "tAxisZ" + toolAx.Z.ToString("F9") ;
	//						Console.WriteLine(machine.getMotionMode() + " " + MachineStatus.Axis.X+machine.getMX().ToString("F9") + " " + MachineStatus.Axis.Y+machine.getMY().ToString("F9") + " " + MachineStatus.Axis.Z+machine.getMZ().ToString("F9") + "\n" + toolAxis );
							SCM_CW.Append(motionMode+ " " + MachineStatus.Axis.X+machine.getMX().ToString("F9") + " " + MachineStatus.Axis.Y+machine.getMY().ToString("F9") + " " + MachineStatus.Axis.Z+machine.getMZ().ToString("F9") + " " + tAxis) ;
						
							SCM_CW.Append('\n').Replace(',','.') ;
						}
						if (motionMode == "G2" && helixMode == false)
						{
						//	string toolAxis = "tAxisX"+ machineStatus.mbase.M31.ToString("F9")+ " " + "tAxisY" + machineStatus.mbase.M32.ToString("F9") + " " + "tAxisZ" + machineStatus.mbase.M33.ToString("F9");
							if (currentBlock.ContainsKey("I") || currentBlock.ContainsKey("J") || currentBlock.ContainsKey("K"))
							{
								string CM_CW_string = motion.NX_circular_motion_CW(MachiningPlane, machineStatus, 0.01, MachineStatus.Axis.X, machine.getMX(), machine.getX(), MachineStatus.Axis.Y, machine.getMY(), machine.getY(), MachineStatus.Axis.Z, machine.getMZ(), machine.getZ(), machine.getI(), machine.getJ(), machine.getK(), machine.getTURN()).ToString() ;
						//		string Helix_string = motion.NX_helical_motion_CW(MachiningPlane, machineStatus, 0.01, MachineStatus.Axis.X, machine.getOMX(), machine.getOX(), MachineStatus.Axis.Y, machine.getOMY(), machine.getOY(), MachineStatus.Axis.Z, machine.getOMZ(), machine.getOZ(), machine.getI(), machine.getJ(), machine.getK()).ToString() ;
						//		string Helix_string = motion.NX_helical_motionCW(MachiningPlane, machineStatus, 0.01, MachineStatus.Axis.X, machine.getMX(), machine.getX(), machine.getOMX(), machine.getOX(), MachineStatus.Axis.Y, machine.getMY(), machine.getY(), machine.getOMY(), machine.getOY(), MachineStatus.Axis.Z, machine.getMZ(), machine.getZ(), machine.getOMZ(), machine.getOZ(), machine.getI(), machine.getJ(), machine.getK()).ToString() ;
//								Console.WriteLine(CM_CW_string) ;
								SCM_CW.Append(CM_CW_string) ;
						//		SCM_CW.Append(Helix_string) ;
//								SCM_CW.Append('\n') ;
								
							}	
							if (currentBlock.ContainsKey("CR") )
							{
								string CM_CW_string = motion.NX_circular_motion_CW(MachiningPlane, machineStatus, 0.01, MachineStatus.Axis.X, machine.getMX(), machine.getX(), MachineStatus.Axis.Y, machine.getMY(), machine.getY(), MachineStatus.Axis.Z, machine.getMZ(), machine.getZ(), machine.getCR(), machine.getTURN()).ToString() ;
								Console.WriteLine(CM_CW_string) ;
								SCM_CW.Append(CM_CW_string ) ;
		//						SCM_CW.Append('\n') ;							
							}
							
						}
	
						if (motionMode == "G3" && helixMode == false )
						{
		//					string toolAxis = "tAxisX"+ machineStatus.mbase.M31.ToString("F9")+ " " + "tAxisY" + machineStatus.mbase.M32.ToString("F9") + " " + "tAxisZ" + machineStatus.mbase.M33.ToString("F9");
							if (currentBlock.ContainsKey("I") || currentBlock.ContainsKey("J") || currentBlock.ContainsKey("K"))
							{
								string CM_CW_string = motion.NX_circular_motion_CCW(machine.getActivePlane(), machineStatus, 0.01, MachineStatus.Axis.X, machine.getMX(), machine.getX(), MachineStatus.Axis.Y, machine.getMY(), machine.getY(), MachineStatus.Axis.Z, machine.getMZ(), machine.getZ(), machine.getI(), machine.getJ(), machine.getK(), machine.getTURN()).ToString() ;
					//			Console.WriteLine(CM_CW_string) ;
								SCM_CW.Append(CM_CW_string ) ;
					//			SCM_CW.Append('\n') ;
								
							}	
							if (currentBlock.ContainsKey("CR") )
							{
								string CM_CW_string = motion.NX_circular_motion_CCW(machine.getActivePlane(), machineStatus, 0.01, MachineStatus.Axis.X, machine.getMX(), machine.getX(), MachineStatus.Axis.Y, machine.getMY(), machine.getY(), MachineStatus.Axis.Z, machine.getMZ(), machine.getZ(), machine.getCR(), machine.getTURN()).ToString() ;
				//				Console.WriteLine(CM_CW_string) ;
								SCM_CW.Append(CM_CW_string ) ;
				//				SCM_CW.Append('\n') ;							
							}						
							
						}
												
					File.WriteAllText("C:\\CS\\NXEditor\\Program_S840.txt", SCM_CW.ToString()) ;
					
				}
				
			}
			
			public virtual void end(GCodeParser parsers, MachineStatus machineStatus)
			{
			
			
			}
			
		}
			
*/			
			public class MachineControl_CLS : MachineController
			{
				
				MachineStatusHelper machine = new MachineStatusHelper();							
				MotionHelper motion = new MotionHelper();
				public StringBuilder SCM_CW = new StringBuilder();
				private double Tolerance {get ; set ;}
				
				public MachineControl_CLS()
				{
					SCM_CW.Append("TOOL PATH/MILL_USER") ;
					SCM_CW.Append('\n');
					SCM_CW.Append("MSYS/0.0000;0.0000;0.0000;1.0000000;0.0000000;0.0000000;0.0000000;1.0000000;0.0000000");
					SCM_CW.Append('\n');
					SCM_CW.Append("$$ centerline data") ;
					SCM_CW.Append('\n');
				//	SCM_CW.Append("$$ contact contour data") ;
				//	SCM_CW.Append('\n');
					SCM_CW.Append("PAINT/PATH") ;
					SCM_CW.Append('\n');
				}
				
				public virtual void startBlock(GCodeParser parser, MachineStatus machineStatus, IDictionary<String, ParsedWord> currentBlock)
				{
						machine.setMachineStatus(machineStatus);
				}
			
			
				public virtual void endBlock(GCodeParser parser, MachineStatus machineStatus, IDictionary<String, ParsedWord> currentBlock)
				{
					String currentLine = parser.getCurrentLine() ;
					String MachiningPlane = machine.getActivePlane()  ;
					String motionMode = machine.getMotionMode() ;
					bool helixMode = false;
					Vector3D toolAx = motion.GetToolAxis(MachiningPlane, machineStatus); 
					
					if (parser.findWordInBlock(new StringBuilder(currentLine)) != null)
					{	
						if(currentBlock.ContainsKey("A") || currentBlock.ContainsKey("B") || currentBlock.ContainsKey("C"))
						{
						 	helixMode = true;
						}
						else
						{
							helixMode = machineStatus.rotAxMode ;
						}
						
						foreach(String key in currentBlock.Keys)
						{
							switch(key)
							{
								case "G91":
									SCM_CW.Append("SET/MODE;INCR") ;
									SCM_CW.Append('\n') ;
									break;
								case "G90":
									SCM_CW.Append("SET/MODE;ABSOL") ;
									SCM_CW.Append('\n') ;
									break;
								case "G0":
									SCM_CW.Append("RAPID") ;
									SCM_CW.Append('\n') ;
									break;
								case "F":
									SCM_CW.Append("FEDRAT/MMPM" + ";" + machine.getFeedrate()) ;
									SCM_CW.Append('\n') ;
									break;
								case "M5":
									SCM_CW.Append("PAINT/TOOL;NOMORE") ;
									SCM_CW.Append('\n') ;
									break;
								case "G41":
									SCM_CW.Append("CUTCOM/LEFT") ;
									SCM_CW.Append('\n') ;
									break;
								case "G40":
									SCM_CW.Append("CUTCOM/OFF") ;
									SCM_CW.Append('\n') ;
									break;
								case "G42":
									SCM_CW.Append("CUTCOM/RIGHT") ;
									SCM_CW.Append('\n') ;
									break ;
							}	
						}
						
						if(currentBlock.ContainsKey("TURN"))
						{
					//		Console.WriteLine(machine.getTURN()) ;
							
						}
						
						if (currentBlock.ContainsKey("C") )
						{
							Tolerance = 0.03 ;
							string CM_CW_string = motion.NX_ROT_AX_C_TABLE(motionMode, MachiningPlane, machineStatus, Tolerance, MachineStatus.Axis.C, machine.getMC(), machine.getC(), MachineStatus.Axis.X, machine.getMX(), machine.getX(), machine.getOMX(), machine.getOX(), MachineStatus.Axis.Y, machine.getMY(), machine.getY(), machine.getOMY(), machine.getOY(), MachineStatus.Axis.Z, machine.getMZ(), machine.getZ(), machine.getOMZ(), machine.getOZ()).ToString() ;
							SCM_CW.Append(CM_CW_string ) ;
													
						}
						if (currentBlock.ContainsKey("A") )
						{
							Tolerance = 0.03 ;
							string CM_CW_string = motion.NX_ROT_AX_A_TABLE(motionMode, MachiningPlane, machineStatus, Tolerance, MachineStatus.Axis.A, machine.getMA(), machine.getA(), MachineStatus.Axis.X, machine.getMX(), machine.getX(), machine.getOMX(), machine.getOX(), MachineStatus.Axis.Y, machine.getMY(), machine.getY(), machine.getOMY(), machine.getOY(), MachineStatus.Axis.Z, machine.getMZ(), machine.getZ(), machine.getOMZ(), machine.getOZ()).ToString() ;
									SCM_CW.Append(CM_CW_string ) ;							
						}
						if (currentBlock.ContainsKey("B") )
						{
							Tolerance = 0.03 ;
							string CM_CW_string = motion.NX_ROT_AX_B_TABLE(motionMode, MachiningPlane, machineStatus, Tolerance, MachineStatus.Axis.B, machine.getMB(), machine.getB(), MachineStatus.Axis.X, machine.getMX(), machine.getX(), machine.getOMX(), machine.getOX(), MachineStatus.Axis.Y, machine.getMY(), machine.getY(), machine.getOMY(), machine.getOY(), MachineStatus.Axis.Z, machine.getMZ(), machine.getZ(), machine.getOMZ(), machine.getOZ()).ToString() ;
									SCM_CW.Append(CM_CW_string ) ;							
						}						
						
						if (motionMode == "G1" && helixMode == false )						
						{
								
							SCM_CW.Append("GOTO/" + machine.getMX().ToString("F6") + ";" + machine.getMY().ToString("F6") + ";" + machine.getMZ().ToString("F6") + ";" + toolAx.X.ToString("F6") + ";" + toolAx.Y.ToString("F6") + ";" + toolAx.Z.ToString("F6")).Replace(',','.');
							SCM_CW.Append('\n') ;
						}
						if (motionMode == "G0" && helixMode == false)
						{
							
							SCM_CW.Append("GOTO/" + machine.getMX().ToString("F6") + ";" + machine.getMY().ToString("F6") + ";" + machine.getMZ().ToString("F6") + ";" + toolAx.X.ToString("F6") + ";" + toolAx.Y.ToString("F6") + ";" + toolAx.Z.ToString("F6")).Replace(',','.');
							SCM_CW.Append('\n');
						}
						
						if (motionMode == "G2" && helixMode == false)
						{
							if (currentBlock.ContainsKey("I") || currentBlock.ContainsKey("J") || currentBlock.ContainsKey("K"))
							{
								string CM_CW_string = motion.NX_circular_motion_CW(MachiningPlane, machineStatus, 0.01, MachineStatus.Axis.X, machine.getMX(), machine.getX(), MachineStatus.Axis.Y, machine.getMY(), machine.getY(), MachineStatus.Axis.Z, machine.getMZ(), machine.getZ(), machine.getI(), machine.getJ(), machine.getK(), machine.getTURN()).ToString() ;
						//		string Helix_string = motion.NX_helical_motion_CW(MachiningPlane, machineStatus, 0.01, MachineStatus.Axis.X, machine.getOMX(), machine.getOX(), MachineStatus.Axis.Y, machine.getOMY(), machine.getOY(), MachineStatus.Axis.Z, machine.getOMZ(), machine.getOZ(), machine.getI(), machine.getJ(), machine.getK()).ToString() ;
						//		string Helix_string = motion.NX_helical_motionCW(MachiningPlane, machineStatus, 0.01, MachineStatus.Axis.X, machine.getMX(), machine.getX(), machine.getOMX(), machine.getOX(), MachineStatus.Axis.Y, machine.getMY(), machine.getY(), machine.getOMY(), machine.getOY(), MachineStatus.Axis.Z, machine.getMZ(), machine.getZ(), machine.getOMZ(), machine.getOZ(), machine.getI(), machine.getJ(), machine.getK()).ToString() ;
								
								SCM_CW.Append(CM_CW_string) ;
							}
							if (currentBlock.ContainsKey("CR") )
							{
								string CM_CW_string = motion.NX_circular_motion_CW(MachiningPlane, machineStatus, 0.01, MachineStatus.Axis.X, machine.getMX(), machine.getX(), MachineStatus.Axis.Y, machine.getMY(), machine.getY(), MachineStatus.Axis.Z, machine.getMZ(), machine.getZ(), machine.getCR(), machine.getTURN()).ToString() ;
								SCM_CW.Append(CM_CW_string ) ;
		//						SCM_CW.Append('\n') ;							
							}

						}
						
						if (motionMode == "G3" && helixMode == false )
						{
		//					string toolAxis = "tAxisX"+ machineStatus.mbase.M31.ToString("F9")+ " " + "tAxisY" + machineStatus.mbase.M32.ToString("F9") + " " + "tAxisZ" + machineStatus.mbase.M33.ToString("F9");
							if (currentBlock.ContainsKey("I") || currentBlock.ContainsKey("J") || currentBlock.ContainsKey("K"))
							{
								string CM_CW_string = motion.NX_circular_motion_CCW(MachiningPlane, machineStatus, 0.005, MachineStatus.Axis.X, machine.getMX(), machine.getX(), MachineStatus.Axis.Y, machine.getMY(), machine.getY(), MachineStatus.Axis.Z, machine.getMZ(), machine.getZ(), machine.getI(), machine.getJ(), machine.getK(), machine.getTURN()).ToString() ;
					//			Console.WriteLine(CM_CW_string) ;
								SCM_CW.Append(CM_CW_string ) ;
					//			SCM_CW.Append('\n') ;
								
							}	
							if (currentBlock.ContainsKey("CR") )
							{
								string CM_CW_string = motion.NX_circular_motion_CCW(MachiningPlane, machineStatus, 0.01, MachineStatus.Axis.X, machine.getMX(), machine.getX(), MachineStatus.Axis.Y, machine.getMY(), machine.getY(), MachineStatus.Axis.Z, machine.getMZ(), machine.getZ(), machine.getCR(), machine.getTURN()).ToString() ;
				//				Console.WriteLine(CM_CW_string) ;
								SCM_CW.Append(CM_CW_string ) ;
				//				SCM_CW.Append('\n') ;							
							}						
							
						}
																		
						if (currentBlock.ContainsKey("M30") || currentBlock.ContainsKey("M2"))
						{
							SCM_CW.Append("END-OF-PATH") ;
							SCM_CW.Append('\n') ;
						}
						
					}
					
						
				}
				
				public virtual void end(GCodeParser parser, MachineStatus machineStatus)				
				{
					
				}
				
			}
	
	
}