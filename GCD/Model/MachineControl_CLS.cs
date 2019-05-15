using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text ;
using System.Windows;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media.Media3D ;
using gcodeparser;
using GCD.ViewModel.Base;
using GCD.ViewModel;


namespace GCD.Model
{
	/// <summary>
	/// Description of SinumerikData.
	/// </summary>
		
	public class MachineControl_CLS : IMachineController 
	{
				
				ISet<String> listOffset = new HashSet<String>() ;
				protected MachineStatusHelper machine = new MachineStatusHelper();							
				protected MotionHelper motion = new MotionHelper();
				public virtual StringBuilder SCM_CW {get; set;}
				private double Tolerance {get ; set ;}
				private Matrix3D mcsData ;
				int startIndex, endIndex ;
				
				
				public MachineControl_CLS()
				{
					SCM_CW = new StringBuilder();
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
					String coordinateSystem = machine.getCoordinateSystems() ;
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
						
						if(!String.IsNullOrEmpty(coordinateSystem) )
						{
								int beforeAddCount = listOffset.Count;
								listOffset.Add(coordinateSystem) ;
								if(listOffset.Count > beforeAddCount)
								{
									ToolChange();
								}
						}
						
						
						StartNXPathSettings(currentBlock) ;
												
						RotAxisMotion(currentBlock, motionMode, MachiningPlane, machineStatus, Tolerance) ;
						
						LinearMotion(motionMode, toolAx, helixMode) ;
						
						CircularMotion(machineStatus, currentBlock, motionMode, MachiningPlane, helixMode) ;
																		
						EndNXPathSettings(currentBlock) ;
						
												
					}
						
				}
				
				public virtual void end(GCodeParser parser, MachineStatus machineStatus)				
				{
					
						CoordinatOffsetManager.Instance().AddValue(listOffset) ;
			//			CoordinatOffsetManager.Instance().ClearOffsetList() ;
						mcsData = NXToolsViewModel.Instance.McsData ;
						SCM_CW.Remove(startIndex, endIndex-startIndex) ;
						SCM_CW.Insert(startIndex,"MSYS/"+mcsData.OffsetX.ToString("F6")+";"+mcsData.OffsetY.ToString("F6")+
						              ";"+mcsData.OffsetZ.ToString("F6")+";"+mcsData.M11.ToString("F9")+";"+mcsData.M12.ToString("F9")+
						              ";"+mcsData.M13.ToString("F9")+";"+mcsData.M21.ToString("F9")+";"+mcsData.M22.ToString("F9")+
						              ";"+mcsData.M23.ToString("F9")).Replace(',','.') ;
//						SCM_CW.Append("END-OF-PATH") ;
//						SCM_CW.Append('\n') ;
				}
				
				public virtual void ToolChange()
				{
					
						SCM_CW.Append("TOOL PATH/MILL_USER") ;
						SCM_CW.Append('\n'); 
						startIndex = SCM_CW.Length ;
						SCM_CW.Append("MSYS/0.0000;0.0000;0.0000;1.0000000;0.0000000;0.0000000;0.0000000;1.0000000;0.0000000");
						endIndex = SCM_CW.Length ;
						SCM_CW.Append('\n');
						SCM_CW.Append("$$ centerline data") ;
						SCM_CW.Append('\n');
						SCM_CW.Append("PAINT/PATH") ;
						SCM_CW.Append('\n');
				}
				
				public void MSYSChange()
				{
					
						CoordinatOffsetManager.Instance().AddValue(listOffset) ;
			//			CoordinatOffsetManager.Instance().ClearOffsetList() ;
						mcsData = NXToolsViewModel.Instance.McsData ;
						SCM_CW.Remove(startIndex, endIndex-startIndex) ;
						SCM_CW.Insert(startIndex,"MSYS/"+mcsData.OffsetX.ToString("F6")+";"+mcsData.OffsetY.ToString("F6")+
						              ";"+mcsData.OffsetZ.ToString("F6")+";"+mcsData.M11.ToString("F9")+";"+mcsData.M12.ToString("F9")+
						              ";"+mcsData.M13.ToString("F9")+";"+mcsData.M21.ToString("F9")+";"+mcsData.M22.ToString("F9")+
						              ";"+mcsData.M23.ToString("F9")).Replace(',','.') ;					
					
				}
				
				public virtual void LinearMotion(string motionMode, Vector3D toolAx, bool helixMode)
				{
						if (motionMode == "G1" && helixMode == false )						
						{
								
							SCM_CW.Append("GOTO/" + machine.getMX().ToString("F6") + ";" + machine.getMY().ToString("F6") + ";" + machine.getMZ().ToString("F6") + ";" + toolAx.X.ToString("F6") + ";" + toolAx.Y.ToString("F6") + ";" + toolAx.Z.ToString("F6"));
							SCM_CW.Append('\n') ;
						}
						if (motionMode == "G0" && helixMode == false)
						{
							SCM_CW.Append("RAPID") ;
							SCM_CW.Append('\n') ;
							SCM_CW.Append("GOTO/" + machine.getMX().ToString("F6") + ";" + machine.getMY().ToString("F6") + ";" + machine.getMZ().ToString("F6") + ";" + toolAx.X.ToString("F6") + ";" + toolAx.Y.ToString("F6") + ";" + toolAx.Z.ToString("F6"));
							SCM_CW.Append('\n');
						}
				}
				
				public virtual void CircularMotion(MachineStatus machineStatus, IDictionary<String, ParsedWord> currentBlock, string motionMode, string MachiningPlane, bool helixMode )
				{
					
						if (motionMode == "G2" && helixMode == false)
						{
							if (currentBlock.ContainsKey("I") || currentBlock.ContainsKey("J") || currentBlock.ContainsKey("K"))
							{
								string CM_CW_string = motion.NX_circular_motion_CW(MachiningPlane, machineStatus, 0.01, MachineStatus.Axis.X, machine.getMX(), machine.getX(), MachineStatus.Axis.Y, machine.getMY(), machine.getY(), MachineStatus.Axis.Z, machine.getMZ(), machine.getZ(), machine.getI(), machine.getJ(), machine.getK(), machine.getTURN()).ToString() ;
								
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

								SCM_CW.Append(CM_CW_string ) ;
					//			SCM_CW.Append('\n') ;
								
							}	
							if (currentBlock.ContainsKey("CR") )
							{
								string CM_CW_string = motion.NX_circular_motion_CCW(MachiningPlane, machineStatus, 0.01, MachineStatus.Axis.X, machine.getMX(), machine.getX(), MachineStatus.Axis.Y, machine.getMY(), machine.getY(), MachineStatus.Axis.Z, machine.getMZ(), machine.getZ(), machine.getCR(), machine.getTURN()).ToString() ;
								SCM_CW.Append(CM_CW_string ) ;
				//				SCM_CW.Append('\n') ;							
							}						
							
						}
						
				}
				
				public virtual void RotAxisMotion(IDictionary<String, ParsedWord> currentBlock, string motionMode, string MachiningPlane, MachineStatus machineStatus, double Tolerance)
				{
					
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
					
				}
				
				public virtual void StartNXPathSettings(IDictionary<String, ParsedWord> currentBlock )
				{
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
									SCM_CW.Append("PAINT/COLOR" + ";" + 186) ;
									SCM_CW.Append('\n') ;
									break;
								case "G1":
								case "G2":
								case "G3":
									SCM_CW.Append("PAINT/COLOR" + ";" + 31) ;
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
					
				}
				
				public virtual void EndNXPathSettings(IDictionary<String, ParsedWord> currentBlock )
				{
						if (currentBlock.ContainsKey("M30") || currentBlock.ContainsKey("M2"))
						{
							SCM_CW.Append("END-OF-PATH") ;
							SCM_CW.Append('\n') ;
						}
					
				}
				
				
	}	
		
}

