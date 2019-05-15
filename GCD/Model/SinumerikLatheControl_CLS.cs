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
	public class SinumerikLatheControl_CLS : MachineControl_CLS
	{
		
				ISet<String> listOffset = new HashSet<String>() ;
				private double Tolerance {get ; set ;}
				private Matrix3D mcsData ;
				int startIndex, endIndex ;
		
				public SinumerikLatheControl_CLS(): base()
				{
				//	SCM_CW = new StringBuilder();
				}
		
				public override void startBlock(GCodeParser parser, MachineStatus machineStatus, IDictionary<String, ParsedWord> currentBlock)
				{
						base.machine.setMachineStatus(machineStatus);
				}
				
				public override void endBlock(GCodeParser parser, MachineStatus machineStatus, IDictionary<String, ParsedWord> currentBlock)
				{
					String currentLine = parser.getCurrentLine() ;
					const String MachiningPlane = "G18";
					String motionMode = base.machine.getMotionMode() ;
					String coordinateSystem = base.machine.getCoordinateSystems() ;
			
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
								
						
						base.StartNXPathSettings(currentBlock) ;
						base.LinearMotion(motionMode, toolAx, false);
						base.CircularMotion(machineStatus, currentBlock, motionMode, MachiningPlane, false) ;
																		
						base.EndNXPathSettings(currentBlock) ;
						
					}
					
				}
				
				public override void end(GCodeParser parser, MachineStatus machineStatus)				
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
				
				public override void ToolChange()
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
