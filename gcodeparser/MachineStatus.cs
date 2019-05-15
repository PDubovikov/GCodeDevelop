using System;
using System.Text;
using System.Collections;
using System.Windows.Media.Media3D;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using gcodeparser.gcodes;


namespace gcodeparser
{

	public class MachineStatus
	{

		// Modal vars are variables that stay the same after each block, examples are A,X,Y,Z etc
		private IDictionary<string, double?> modalVars = new Dictionary<string, double?>();
		
		// Non modal var's are variable's that will get removed after each block
		private ISet<string> modals = new HashSet<string>();
		private ISet<string> collectionsWords = new HashSet<string>();
		public ISet<string> modalsCmd = new HashSet<string>();
		private ISet<string> command = new HashSet<string>();
		public IDictionary<Axis, double?> machineCoordinates = new Dictionary<Axis, double?>();
		public IDictionary<Axis, double?> originalMachineCoordinates = new Dictionary<Axis, double?>();  // dictionary for calculate arc center point
		private IDictionary<NonModalsVars, double?> machineCoordinatesVars = new Dictionary<NonModalsVars, double?>();
		private IDictionary<Axis, double?> machineOffsets = new Dictionary<Axis, double?>();
		private IDictionary<Axis, double?> coordinatesCache = new Dictionary<Axis, double?>();
		private IDictionary<Axis, double?> originalCoordinatesCache = new Dictionary<Axis, double?>();  // dictionary for calculate arc center point
		private IDictionary<Axis, double?> coordinatesOld = new Dictionary<Axis, double?>();
		private IDictionary<Axis, double?> originalCoordinatesOld = new Dictionary<Axis, double?>(); // dictionary for calculate arc center point
		public IDictionary<Axis, double?> newPosition = new Dictionary<Axis,double?>() ;
		public StringBuilder newLine = new StringBuilder() ;
		private bool coordinatesCachesDirty = true;
		public bool rotAxMode = false ;
		public Matrix3D mbase = new Matrix3D() ;
		public Matrix3D mToolAx ;
		public Vector3D ToolAxis = new Vector3D();
		public Matrix3D origin = new Matrix3D() ;
		public Point3D Value ;
		private Point3D VarValue ;
		public Point3D ValueCash ;
		private Point3D Ptc, Ptr ;
		private Point3D RotValue ;
		private Point3D RotValueCash ;
		private double Phi0 {get ;set ;} 
        private double R0 {get ;set ;}
        private double R1 {get ;set ;}
        private double Rmid {get ;set ;}
        private double Height {get; set;}
        public double tolerance {get; set;}
        private int num {get; set;}
        public bool transformationON ;
			
		// We should consider asking this values from the MachineControoler or MachineRules ?
		public enum ModalVars
		{
			F,S
		}
		
		public enum ModalWords
    	{
    		DIAMON, DIAM90, DIAMOF
    	}
		
		public enum NonModals
		{
			G53,
			G500,
			G9
		}
		
		public enum Transformation
		{
			TRANS, ATRANS, ROT, AROT, SCALE, ASCALE, MIRROR, AMIRROR, ROTAXIS
		}

		// These variables can be set during the block, but will get cleared out when a new block starts
		public enum NonModalsVars
		{
			P,I,K,J,L,R,CR,T,TURN
		}

		// Axis words to be copied in the coordinates
		public enum Axis
		{
			A,B,C,U,V,W,X,Y,Z
		}
		
		public enum MachPlane
		{
			G17,G18,G19
		}
		
		public MachineStatus()
		{
			foreach (Axis word in Enum.GetValues(typeof(Axis)))
			{
				machineCoordinates[word] = 0.0;
				originalMachineCoordinates[word] = 0.0;
				machineOffsets[word] = 0.0;
				coordinatesCache[word] = 0.0;
				originalCoordinatesCache[word] = 0.0;
				newPosition[word] = 0.0 ;
				switch(word)
				{
					case Axis.X:
						Value.X = 0.0 ;
						break ;
					case Axis.Y:
						Value.Y = 0.0 ;
						break ;
					case Axis.Z:
						Value.Z = 0.0 ;
						break ;
					case Axis.A:
						RotValue.X = 0.0 ;
						break;
					case Axis.B:
						RotValue.Y = 0.0 ;
						break;
					case Axis.C:
						RotValue.Z = 0.0 ;
						break ;
				}
			}
			foreach (ModalWords modalwords in Enum.GetValues(typeof(ModalWords)))
			{			         	
			     collectionsWords.Add(modalwords.ToString());
        	}
			
			origin.M11 = 1; origin.M12 = 0; origin.M13 = 0;  // x axis
			origin.M21 = 0; origin.M22 = 1; origin.M23 = 0;  // y axis
			origin.M31 = 0; origin.M32 = 0; origin.M33 = 1;  // z axis
			origin.OffsetX = 0.0; origin.OffsetY = 0.0; origin.OffsetZ = 0.0; // offset
			
			mToolAx.M11 = 1; mToolAx.M12 = 0; mToolAx.M13 = 0;  // x axis
			mToolAx.M21 = 0; mToolAx.M22 = 1; mToolAx.M23 = 0;  // y axis
			mToolAx.M31 = 0; mToolAx.M32 = 0; mToolAx.M33 = 1;  // z axis
			mToolAx.OffsetX = 0.0; mToolAx.OffsetY = 0.0; mToolAx.OffsetZ = 0.0; // offset			
			
			mbase.M11 = 1; mbase.M12 = 0; mbase.M13 = 0;  // x axis
			mbase.M21 = 0; mbase.M22 = 1; mbase.M23 = 0;  // y axis
			mbase.M31 = 0; mbase.M32 = 0; mbase.M33 = 1;  // z axis
			mbase.OffsetX = 0.0; mbase.OffsetY = 0.0; mbase.OffsetZ = 0.0; // offset
			Ptc.X = 0.0 ; Ptc.Y = 0.0 ; Ptc.Z = 0.0 ; 	// rotate center point
			ToolAxis.X = 0.0 ; ToolAxis.Y = 0.0 ; ToolAxis.Z = 1.0 ; // G17 
			tolerance = 0.01 ;
			
			modalVars["F"] = 0.0;
			modalVars["S"] = 0.0;

			modals.Add("G80");
			modals.Add("G17");
			modals.Add("G40");
			modals.Add("G20");
			modals.Add("G90");
			modals.Add("G94");
			modals.Add("G95");
			modals.Add("G54");
			modals.Add("G49");
			modals.Add("G99");
			modals.Add("G64");
			modals.Add("G97");
			modals.Add("G91_1");
			modals.Add("G8");
			modals.Add("M5");
			modals.Add("M9");
			modals.Add("M48");
			modals.Add("M53");
			modals.Add("M0");

		}

		
		public void setBlock(IDictionary<string, ParsedWord> block)
		{
			setModals(block) ;
			GetValue(block) ;
			GetVarValue(block) ;
			setVars(block) ;
			coordinatesCachesDirty = true ;
			setAxis(block) ;
			setNonModalVars(block) ;
			
		}
		
		public void setWord(ISet<String> block) 
		{   	 
    		foreach (String str in block )
    		{	
    			foreach (ModalWords item in Enum.GetValues(typeof(ModalWords)))
    			{
    				if (block.Contains(item.ToString()))
    				{ 	 
    					modalsCmd.ExceptWith(collectionsWords);
    			
    					modalsCmd.Add(str) ;
    				}
    				else
    				{
    			//		command.Add(str) ;
    				}
    			}
    			
    		}
		}
		
		public void setCommandBlk(IDictionary<string, ParsedCmdGroup> Cmdblock, IDictionary<string, ParsedWord> block)
		{
			foreach (Transformation trans in Enum.GetValues(typeof(Transformation)))
			{
				if (block.ContainsKey(trans.ToString()))
				{
					switch(trans)
					{
						case Transformation.TRANS:
							 setCoordinateTrans(Cmdblock) ;
							 break;
						case Transformation.ATRANS:
							 setCoordinateATrans(Cmdblock) ;
							 break;
						case Transformation.ROT:
							 setCoordinateRotate(Cmdblock) ;
							 break;
						case Transformation.AROT:
							 setCoordinateARotate(Cmdblock) ;
		//					 setAxisAfterTransform() ;
							 break;
						case Transformation.SCALE:
							 setCoordinateScale(Cmdblock) ;
							 break ;
						case Transformation.ASCALE:
							 setCoordinateAScale(Cmdblock) ;
							 break ;							 
						default:
							 break;
					}
					
				}
				
			}			
		}
		

		private void GetValue(IDictionary<string, ParsedWord> block)
		{
			foreach (Axis axis in Enum.GetValues(typeof(Axis)))
			{
				if (block.ContainsKey(axis.ToString()))
				{
					ParsedWord word = block[axis.ToString()] ;
					switch(axis)
					{
						case Axis.X:
							if (word.cmode.Contains("IC") || modals.Contains("G91"))
							{
								if(modalsCmd.Contains("DIAMON") )
									Value.X = Value.X + word.value*0.5 ;
								else
									Value.X = Value.X + word.value ;
							}
							else
							{	
								if(modalsCmd.Contains("DIAMON") || modalsCmd.Contains("DIAM90"))
									Value.X = word.value*0.5 ;
								else
									Value.X = word.value ;
							}
							break;
						case Axis.Y:
							if (word.cmode.Contains("IC") || modals.Contains("G91"))
								Value.Y = Value.Y + word.value ;
							else
								Value.Y = word.value ;							
							break;
						case Axis.Z:
							if (word.cmode.Contains("IC") || modals.Contains("G91"))
								Value.Z = Value.Z + word.value ;	
							else
								Value.Z = word.value ;
							break;
						case Axis.A:
							if (word.cmode.Contains("IC") || modals.Contains("G91"))
								RotValue.X = RotValue.X + word.value ;	
							else
								RotValue.X = word.value ;
							break;							
						case Axis.B:
							if (word.cmode.Contains("IC") || modals.Contains("G91"))
								RotValue.Y = RotValue.Y + word.value ;	
							else
								RotValue.Y = word.value ;
							break;							
						case Axis.C:
							if (word.cmode.Contains("IC") || modals.Contains("G91"))
								RotValue.Z = RotValue.Z + word.value ;	
							else
								RotValue.Z = word.value ;
							break;
					}
				}
			}
		}
		
		private void GetVarValue(IDictionary<string, ParsedWord> block)
		{
			foreach (NonModalsVars Vars in Enum.GetValues(typeof(NonModalsVars)))
			{
           		if (block.ContainsKey(Vars.ToString()))
           		{
           			ParsedWord word = block[Vars.ToString()];
           			
           			switch(Vars)
					{
						case NonModalsVars.I:
						 	VarValue.X = word.value ;
							break;
						case NonModalsVars.J:
							VarValue.Y = word.value ;
							break;
						case NonModalsVars.K:
							VarValue.Z = word.value ;
							break;
						default:
							break ;
					}
			
           		}
			}
		}
		
		
		private void setCoordinateTrans(IDictionary<string, ParsedCmdGroup> block)
		{	
			// Reset all transformations
			mbase.M11 = 1; mbase.M12 = 0; mbase.M13 = 0;  // x axis
			mbase.M21 = 0; mbase.M22 = 1; mbase.M23 = 0;  // y axis
			mbase.M31 = 0; mbase.M32 = 0; mbase.M33 = 1;  // z axis
			mbase.OffsetX = 0.0; mbase.OffsetY = 0.0; mbase.OffsetZ = 0.0; // offset
			Ptc.X = 0.0 ; Ptc.Y = 0.0; Ptc.Z = 0.0 ;
			
			foreach (Axis axis in Enum.GetValues(typeof(Axis)))
			{
				if (block.ContainsKey(axis.ToString()))
				{
					ParsedCmdGroup word = block[axis.ToString()] ;
						switch(axis)
						{
							case Axis.X:							
								mbase.OffsetX = word.ValueAxis1 ;
								Ptc.X = mbase.OffsetX ;
								break;
							case Axis.Y:								
								mbase.OffsetY = word.ValueAxis1 ;
								Ptc.Y = mbase.OffsetY ;
								break;
							case Axis.Z:								
								mbase.OffsetZ = word.ValueAxis1 ;
								Ptc.Z = mbase.OffsetZ ;
								break;
						}
				}
			}
		}
		
		private void setCoordinateATrans(IDictionary<string, ParsedCmdGroup> block)
		{	
			
			foreach (Axis axis in Enum.GetValues(typeof(Axis)))
			{
				if (block.ContainsKey(axis.ToString()))
				{
					ParsedCmdGroup word = block[axis.ToString()] ;
						switch(axis)
						{
							case Axis.X:							
								Ptc.X = word.ValueAxis1 ;
								mbase.TranslatePrepend(new Vector3D(Ptc.X, 0.0, 0.0) );
								Ptc.X = mbase.OffsetX ;
								Ptc.Y = mbase.OffsetY;
								Ptc.Z = mbase.OffsetZ ;
								break;
							case Axis.Y:								
								Ptc.Y = word.ValueAxis1 ;
								mbase.TranslatePrepend(new Vector3D(0.0, Ptc.Y, 0.0) );
								Ptc.X = mbase.OffsetX ;
								Ptc.Y = mbase.OffsetY;
								Ptc.Z = mbase.OffsetZ ;
								break;
							case Axis.Z:								
								Ptc.Z = word.ValueAxis1 ;
								mbase.TranslatePrepend(new Vector3D(0.0, 0.0, Ptc.Z) );
								Ptc.X = mbase.OffsetX ;
								Ptc.Y = mbase.OffsetY;
								Ptc.Z = mbase.OffsetZ ;
								break;
						}
		//			machineOffsets[axis] = word.value ;
				}
			}
		//	Console.WriteLine("OFFSET" + " " + mbase.OffsetX + " " + mbase.OffsetY + " " + mbase.OffsetZ) ;
		}		
		
		private void setCoordinateRotate(IDictionary<string, ParsedCmdGroup> block)
		{
			// Reset all transformations
 			mbase.M11 = 1; mbase.M12 = 0; mbase.M13 = 0;  // x axis
			mbase.M21 = 0; mbase.M22 = 1; mbase.M23 = 0;  // y axis
			mbase.M31 = 0; mbase.M32 = 0; mbase.M33 = 1;  // z axis
			mbase.OffsetX = 0.0; mbase.OffsetY = 0.0; mbase.OffsetZ = 0.0; // offset
			Ptc.X = 0.0 ; Ptc.Y = 0.0 ; Ptc.Z = 0.0 ; 	// rotate center point
		
			foreach (Axis axis in Enum.GetValues(typeof(Axis)))
			{
				if (block.ContainsKey(axis.ToString()))
				{
					ParsedCmdGroup word = block[axis.ToString()] ;
						switch(axis)
						{
							case Axis.X:							
								mbase.RotateAt(new Quaternion(new Vector3D(mbase.M11, mbase.M12, mbase.M13), word.ValueAxis1), Ptc);
								break;
							case Axis.Y:								
								mbase.RotateAt(new Quaternion(new Vector3D(mbase.M21, mbase.M22, mbase.M23), word.ValueAxis1), Ptc);			
								break;
							case Axis.Z:								
								mbase.RotateAt(new Quaternion(new Vector3D(mbase.M31, mbase.M32, mbase.M33), word.ValueAxis1 ), Ptc);
								break;
						}
				}
			}
			
			
		}

		
		private void setCoordinateARotate(IDictionary<string, ParsedCmdGroup> block)
		{
				
			foreach (Axis axis in Enum.GetValues(typeof(Axis)))
			{
				if (block.ContainsKey(axis.ToString()))
				{
					ParsedCmdGroup word = block[axis.ToString()] ;
						switch(axis)
						{
							case Axis.X:								
								mbase.RotateAt(new Quaternion(new Vector3D(mbase.M11, mbase.M12, mbase.M13), word.ValueAxis1), Ptc);								
								break;
							case Axis.Y:								
								mbase.RotateAt(new Quaternion(new Vector3D(mbase.M21, mbase.M22, mbase.M23), word.ValueAxis1), Ptc);								
								break;
							case Axis.Z:								
								mbase.RotateAt(new Quaternion(new Vector3D(mbase.M31, mbase.M32, mbase.M33), word.ValueAxis1 ), Ptc);								
								break;
						}
				}
			}
			
		}

		private void setCoordinateScale(IDictionary<string, ParsedCmdGroup> block)
		{	
			// Reset all transformations
			mbase.M11 = 1; mbase.M12 = 0; mbase.M13 = 0;  // x axis
			mbase.M21 = 0; mbase.M22 = 1; mbase.M23 = 0;  // y axis
			mbase.M31 = 0; mbase.M32 = 0; mbase.M33 = 1;  // z axis
			mbase.OffsetX = 0.0; mbase.OffsetY = 0.0; mbase.OffsetZ = 0.0; // offset
			Ptc.X = 0.0 ; Ptc.Y = 0.0; Ptc.Z = 0.0 ;
			
			foreach (Axis axis in Enum.GetValues(typeof(Axis)))
			{
				if (block.ContainsKey(axis.ToString()))
				{
					ParsedCmdGroup word = block[axis.ToString()] ;
						switch(axis)
						{
							case Axis.X:
								mbase.ScaleAt(new Vector3D(word.ValueAxis1, 1.0, 1.0), Ptc) ;
								Ptc.X = mbase.OffsetX ;
								break;
							case Axis.Y:
								mbase.ScaleAt(new Vector3D(1.0, word.ValueAxis1, 1.0), Ptc) ;								
								Ptc.Y = mbase.OffsetY ;
								break;
							case Axis.Z:
								mbase.ScaleAt(new Vector3D(1.0, 1.0, word.ValueAxis1), Ptc) ;								
								Ptc.Z = mbase.OffsetZ ;
								break;
						}
				}
			}
		}

		private void setCoordinateAScale(IDictionary<string, ParsedCmdGroup> block)
		{	
			
			foreach (Axis axis in Enum.GetValues(typeof(Axis)))
			{
				if (block.ContainsKey(axis.ToString()))
				{
					ParsedCmdGroup word = block[axis.ToString()] ;
						switch(axis)
						{
							case Axis.X:
								mbase.ScaleAt(new Vector3D(word.ValueAxis1, 1.0, 1.0), Ptc) ;
								Ptc.X = mbase.OffsetX ;
								break;
							case Axis.Y:
								mbase.ScaleAt(new Vector3D(1.0, word.ValueAxis1, 1.0), Ptc) ;								
								Ptc.Y = mbase.OffsetY ;
								break;
							case Axis.Z:
								mbase.ScaleAt(new Vector3D(1.0, 1.0, word.ValueAxis1), Ptc) ;								
								Ptc.Z = mbase.OffsetZ ;
								break;
						}
				}
			}
		}		

		private void setAxis(IDictionary<string, ParsedWord> block)
		{
			foreach (Axis axis in Enum.GetValues(typeof(Axis)))
			{
				if (block.ContainsKey(axis.ToString()))
				{
					ParsedWord word = block[axis.ToString()];
						
								machineCoordinates[Axis.X] = mbase.Transform(Value).X ;
								machineCoordinates[Axis.Y] = mbase.Transform(Value).Y ;
								machineCoordinates[Axis.Z] = mbase.Transform(Value).Z ;
								machineCoordinates[Axis.A] = RotValue.X ;
								machineCoordinates[Axis.B] = RotValue.Y ;
								machineCoordinates[Axis.C] = RotValue.Z ;
								originalMachineCoordinates[Axis.X] = Value.X ;
								originalMachineCoordinates[Axis.Y] = Value.Y ;
								originalMachineCoordinates[Axis.Z] = Value.Z ;
				//				Console.WriteLine("X: " + machineCoordinates[Axis.X].Value + " " + "Y: " + machineCoordinates[Axis.Y].Value + " " + "Z: " + machineCoordinates[Axis.Z].Value );

						if(modalsCmd.Contains("DIAMON"))
						{
					//		machineCoordinates[Axis.X] = mbase.Transform(Value).X*0.5 ;
						}
						
				}
			}
			
		}
		
		private void setAxisAfterTransform()
		{
			Point3D currentValue = new Point3D() ;
			currentValue.X = machineCoordinates[Axis.X].Value ;
			currentValue.Y = machineCoordinates[Axis.Y].Value ;
			currentValue.Z = machineCoordinates[Axis.Z].Value ;
			
			foreach (Axis axis in Enum.GetValues(typeof(Axis)))
			{
						machineCoordinates[Axis.X] = mbase.Transform(currentValue).X ;
						machineCoordinates[Axis.Y] = mbase.Transform(currentValue).Y ;
						machineCoordinates[Axis.Z] = mbase.Transform(currentValue).Z ;
				//	Console.WriteLine("X: " + machineCoordinates[Axis.X].Value + " " + "Y: " + machineCoordinates[Axis.Y].Value + " " + "Z: " + machineCoordinates[Axis.Z].Value );
						
			}
			
		}		
		

		private void setModals(IDictionary<String, ParsedWord> block)
		{
			foreach (ParsedWord word in block.Values)
			{
				if (word.word.Equals("G") || word.word.Equals("M"))
				{
					
					// Find the modal group and remove all modals if found
					GCodeGroups thisGroup = ModalGrouping.whatGroup(word.parsed);
					if (thisGroup != null )
					{
						if (thisGroup.ToString() != "Default")
						modals.ExceptWith(ModalGrouping.groupToModals[thisGroup]);
						
					}

					modals.Add(word.parsed);
				}
			}
		}


		private void setVars(IDictionary<string, ParsedWord> block)
		{
			foreach (ModalVars modalvar in Enum.GetValues(typeof(ModalVars)))
			{
				if (block.ContainsKey(modalvar.ToString()))
				{
					ParsedWord word = block[modalvar.ToString()];
					if (word != null)
					{
						modalVars[word.word] = word.value;
					}
				}
			}		
		}
		
   		 private void setNonModalVars(IDictionary<String, ParsedWord> block)
   		 {
			foreach (NonModalsVars Vars in Enum.GetValues(typeof(NonModalsVars)))
			{
           		if (block.ContainsKey(Vars.ToString()))
           		{
           			ParsedWord word = block[Vars.ToString()];
                	if (word.cmode.Contains("AC")) 
                	{
                		switch(Vars)
                		{
                			case NonModalsVars.I:
                				if (modalsCmd.Contains("DIAMON") || modalsCmd.Contains("DIAM90"))
                					machineCoordinatesVars[NonModalsVars.I] = mbase.Transform(VarValue).X*0.5 - coordinatesOld[Axis.X] ;
                				else
                					machineCoordinatesVars[Vars] = mbase.Transform(VarValue).X - coordinatesOld[Axis.X] ;
                				break;
                			case NonModalsVars.J:
                				machineCoordinatesVars[Vars] = mbase.Transform(VarValue).Y - coordinatesOld[Axis.Y] ;
                				break;
                			case NonModalsVars.K:
                				machineCoordinatesVars[Vars] = mbase.Transform(VarValue).Z - coordinatesOld[Axis.Z] ;
                				break;
                			default:
                				machineCoordinatesVars[Vars] = word.value ;
                				break;
                		}
                		
               	 	}
                	else
                	{
                		machineCoordinatesVars[Vars] = word.value ;
                	}

            	}
           		else
           		{
          			machineCoordinatesVars[Vars] = 0.0 ;
           		}
        	}
    	}		
		

		public void copyFrom(MachineStatus cpFrom)
		{

			this.modalVars = cpFrom.modalVars;
        	this.modals = cpFrom.modals;
        	this.machineCoordinates = cpFrom.machineCoordinates;
        	this.originalMachineCoordinates = cpFrom.originalMachineCoordinates ;
        	this.newPosition = cpFrom.newPosition ;
        	this.newLine = cpFrom.newLine ;
        	this.rotAxMode = cpFrom.rotAxMode ;
        	this.ToolAxis = cpFrom.ToolAxis ;
//        	this.coordinatesOld = cpFrom.coordinatesOld ;
        	this.machineOffsets = cpFrom.machineOffsets; 
			
		}
		
		public virtual void startBlock()
		{
			foreach (NonModalsVars item in Enum.GetValues(typeof(NonModalsVars)))
			{
				modalVars.Remove(item.ToString());
				
			}
			foreach (NonModals item in Enum.GetValues(typeof(NonModals)))
			{
				modals.Remove(item.ToString());
			}
			foreach (Axis axis in Enum.GetValues(typeof(Axis)))
			{
//					coordinatesCache[axis] = machineCoordinates[axis] - machineOffsets[axis];
					coordinatesOld[axis] = machineCoordinates[axis] ;
					originalCoordinatesOld[axis] = originalMachineCoordinates[axis] ;
					ValueCash.X = machineCoordinates[Axis.X].Value ;
					ValueCash.Y = machineCoordinates[Axis.Y].Value ;
					ValueCash.Z = machineCoordinates[Axis.Z].Value ;
					RotValueCash.X = machineCoordinates[Axis.A].Value ;
					RotValueCash.Y = machineCoordinates[Axis.B].Value ;
					RotValueCash.Z = machineCoordinates[Axis.C].Value ;
						
		//				Console.WriteLine("ValueCash.X: " + ValueCash.X + " " + "ValueCash.Y: " + ValueCash.Y + " " + "ValueCash.Z: " + ValueCash.Z ) ;
			}				
		}

/// <summary>
/// 
/// </summary>
/// <returns></returns>		
		
		private void TURN_MODE(IDictionary<string, ParsedWord> block)
		{			
//				if (block.ContainsKey(Axis.X.ToString()))
//				{					
//					machineCoordinates[Axis.X] = machineCoordinates[Axis.X]*0.5;
//		//			machineCoordinates[Axis.X] = 0.0 ;
//					
//				}
//				if (block.ContainsKey(NonModalsVars.I.ToString()))
//				{
//					ParsedWord word = block[NonModalsVars.I.ToString()];
//
//					machineCoordinatesVars[NonModalsVars.I] = word.value*0.5 - coordinatesOld[Axis.X] ;
//				}
		 }
		
/// <summary>
/// 
/// </summary>
/// <returns></returns>
		public IDictionary<string, double?> getModalVars()
		{
//			return Collections.unmodifiableMap(modalVars);

			return new ReadOnlyDictionary<string,double?>(modalVars) ;
			
		}
		

		public ISet<string> getModals()
		{
			return new HashSet<string>(modals);

		}


		public virtual void endBlock()
		{
			newLine.Clear() ;
		}
		

		private bool hasAny<T>(string value, Type enumClass) where T : IEnumerable<T>
		{
			foreach (Type c in enumClass.GetEnumValues())
			{
				if (c.Name.Equals(value))
				{
					return true;
				}
			}
			return false;
		}

		// Actual machine's absolute position
		public virtual IDictionary<Axis, double?> MachineCoordinates
		{
			get
			{
				return new ReadOnlyDictionary<Axis, double?>(machineCoordinates);
			}

		}
		
		public virtual IDictionary<Axis, double?> OriginalMachineCoordinates
		{
			get
			{
				return new ReadOnlyDictionary<Axis, double?>(originalMachineCoordinates);
			}
		}
		
		public virtual IDictionary<NonModalsVars, double?> MachineCoordinatesVars
		{
			get
			{
				return new ReadOnlyDictionary<NonModalsVars, double?>(machineCoordinatesVars);
			}
		}

		// Get relative position
		public virtual IDictionary<Axis, double?> Coordinates
		{
			get
			{
				if (coordinatesCachesDirty)
				{
					foreach (Axis axis in Enum.GetValues(typeof(Axis)))
					{
					//	coordinatesCache[axis] = machineCoordinates[axis] - machineOffsets[axis];
						coordinatesCache[axis] = coordinatesOld[axis] ;
					}
				}
    
				return new ReadOnlyDictionary<Axis, double?>(coordinatesCache);
			}
			
		}
		
	
		public virtual IDictionary<Axis, double?> OriginalCoordinates
		{
			get
			{
				if (coordinatesCachesDirty)
				{
					foreach (Axis axis in Enum.GetValues(typeof(Axis)))
					{
					//	coordinatesCache[axis] = machineCoordinates[axis] - machineOffsets[axis];
						originalCoordinatesCache[axis] = originalCoordinatesOld[axis] ;
					}
				}
    
				return new ReadOnlyDictionary<Axis, double?>(originalCoordinatesCache);
			}
		}

		// Machine offsets
		public virtual IDictionary<Axis, double?> MachineOffsets
		{
			get
			{
				return new ReadOnlyDictionary<Axis, double?>(machineOffsets);
			}
		}

		public virtual IDictionary<Axis, double?> GetNewPosition
		{
			get
			{
				return new ReadOnlyDictionary<Axis, double?>(newPosition);
			}
		}		
		
	}

}
