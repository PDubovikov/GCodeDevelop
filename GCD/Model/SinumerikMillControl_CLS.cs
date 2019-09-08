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
	/// Description of SinumerikMillControl_CLS.
	/// </summary>
	public class SinumerikMillControl_CLS : MachineControl_CLS
	{
		
			ISet<String> listOffset = new HashSet<String>() ;
			private double Tolerance {get ; set ;}
			private Matrix3D mcsData ;
			private Vector3D rndFirstVector;
			private bool rndStrokeMode = false;
			int startIndex, endIndex ;
			
			public override void startBlock(GCodeParser parser, MachineStatus machineStatus, IDictionary<String, ParsedWord> currentBlock)
			{
				base.machine.setMachineStatus(machineStatus);
			}
			
			public override void endBlock(GCodeParser parser, MachineStatus machineStatus, IDictionary<String, ParsedWord> currentBlock)
			{
				String currentLine = parser.getCurrentLine() ;
				String MachiningPlane = machine.getActivePlane()  ;
				String motionMode = base.machine.getMotionMode() ;				
				String coordinateSystem = base.machine.getCoordinateSystems() ;
				bool helixMode = false;
				Vector3D toolAx = motion.GetToolAxis(MachiningPlane, machineStatus);
				if(currentLine.Contains("MSG"))
				{
						MessageBox.Show(currentLine.Substring(4,currentLine.Length-5));
				}				
				
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
								
						
						base.StartNXPathSettings(currentBlock) ;												
						base.RotAxisMotion(currentBlock, motionMode, MachiningPlane, machineStatus, Tolerance) ;						
						LinearMotion(motionMode, MachiningPlane, currentBlock, machineStatus, toolAx, helixMode) ;						
						base.CircularMotion(machineStatus, currentBlock, motionMode, MachiningPlane, helixMode) ;						
						base.ThreadTurnMotion(motionMode, currentBlock, toolAx) ;																		
						base.EndNXPathSettings(currentBlock) ;
						
				}
					
			}


			public override void end(GCodeParser parser, MachineStatus machineStatus)				
			{
					
					CoordinatOffsetManager.Instance().AddValue(listOffset) ;
					mcsData = NXToolsViewModel.Instance.McsData ;
					SCM_CW.Remove(startIndex, endIndex-startIndex) ;
					SCM_CW.Insert(startIndex,"MSYS/"+mcsData.OffsetX.ToString("F6")+";"+mcsData.OffsetY.ToString("F6")+
					              ";"+mcsData.OffsetZ.ToString("F6")+";"+mcsData.M11.ToString("F9")+";"+mcsData.M12.ToString("F9")+
					              ";"+mcsData.M13.ToString("F9")+";"+mcsData.M21.ToString("F9")+";"+mcsData.M22.ToString("F9")+
					              ";"+mcsData.M23.ToString("F9")).Replace(',','.') ;
			}

			
			public override void LinearMotion(string motionMode, string MachiningPlane, IDictionary<String, ParsedWord> currentBlock, MachineStatus mstat, Vector3D toolAx,  bool helixMode)
			{
					if ( (motionMode == "G1" || motionMode == "G0") && helixMode == false )
					{
						double rnd = machine.getRND();
						
						if(rndStrokeMode)
						{
							string linearWithRnd = motion.LinearMotionWithRND(MachiningPlane, mstat, rndFirstVector, MachineStatus.Axis.X, machine.getMX(), machine.getX(), MachineStatus.Axis.Y, machine.getMY(), machine.getY(), MachineStatus.Axis.Z, machine.getMZ(), machine.getZ(), machine.getRND() ).ToString();
							SCM_CW.Append(linearWithRnd) ;
							rndStrokeMode = false;
						}
						else if (currentBlock.ContainsKey("ANG"))
						{
							string linearWithAng = motion.NX_LinearMotionWithANG(MachiningPlane, mstat, MachineStatus.Axis.X, machine.getMX(), machine.getX(), MachineStatus.Axis.Y, machine.getMY(), machine.getY(), MachineStatus.Axis.Z, machine.getMZ(), machine.getZ(), machine.getANG() ).ToString();
							SCM_CW.Append(linearWithAng) ;
						}
						else if (currentBlock.ContainsKey("RND"))
						{
							rndFirstVector = motion.GetRndFirstVector(MachiningPlane, mstat, MachineStatus.Axis.X, machine.getMX(), machine.getX(), MachineStatus.Axis.Y, machine.getMY(), machine.getY(), MachineStatus.Axis.Z, machine.getMZ(), machine.getZ()) ;
							rndStrokeMode = true;
						}
						else
						{
							SCM_CW.Append("GOTO/" + machine.getMX().ToString("F6") + ";" + machine.getMY().ToString("F6") + ";" + machine.getMZ().ToString("F6") + ";" + toolAx.X.ToString("F6") + ";" + toolAx.Y.ToString("F6") + ";" + toolAx.Z.ToString("F6"));
							SCM_CW.Append('\n') ;
						}
					}
			}

			public override void ToolChange()
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
			
	}
}
