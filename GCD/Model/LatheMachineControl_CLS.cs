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
	/// Description of LatheMachineControl_CLS.
	/// </summary>
	public class LatheMachineControl_CLS : MachineController
	{
		
				private static LatheMachineControl_CLS self ;
				ISet<String> listOffset = new HashSet<String>() ;
				MachineStatusHelper machine = new MachineStatusHelper();							
				MotionHelper motion = new MotionHelper();
				public StringBuilder SCM_CW = new StringBuilder();
				private double Tolerance {get ; set ;}
				private Matrix3D mcsData ;
				int startIndex, endIndex ;
		
				public LatheMachineControl_CLS()
				{
			
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
			
					Vector3D toolAx = motion.GetToolAxis(MachiningPlane, machineStatus);
					
					if (parser.findWordInBlock(new StringBuilder(currentLine)) != null)
					{
						
						if(!String.IsNullOrEmpty(coordinateSystem) )
						{
								int beforeAddCount = listOffset.Count;
								listOffset.Add(coordinateSystem) ;
								if(listOffset.Count > beforeAddCount)
								{
									ToolChange();
								}
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
						
						if (motionMode == "G1")						
						{
								
							SCM_CW.Append("GOTO/" + machine.getMX().ToString("F6") + ";" + machine.getMY().ToString("F6") + ";" + machine.getMZ().ToString("F6") + ";" + toolAx.X.ToString("F6") + ";" + toolAx.Y.ToString("F6") + ";" + toolAx.Z.ToString("F6")).Replace(',','.');
							SCM_CW.Append('\n') ;
						}
						if (motionMode == "G0")
						{
							SCM_CW.Append("RAPID") ;
							SCM_CW.Append('\n') ;
							SCM_CW.Append("GOTO/" + machine.getMX().ToString("F6") + ";" + machine.getMY().ToString("F6") + ";" + machine.getMZ().ToString("F6") + ";" + toolAx.X.ToString("F6") + ";" + toolAx.Y.ToString("F6") + ";" + toolAx.Z.ToString("F6")).Replace(',','.');
							SCM_CW.Append('\n');
						}
						
						if (motionMode == "G2" )
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
						
						if (motionMode == "G3")
						{
	
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
				
				private void ToolChange()
				{
					
						SCM_CW.Append("TOOL PATH/LATHE_USER_OP") ;
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
				
		
	}
}
