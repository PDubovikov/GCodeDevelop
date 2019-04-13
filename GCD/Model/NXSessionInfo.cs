using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq ;
using NXOpen;
using NXOpen.CAM;
using NXOpen.Utilities;
using System.Windows.Media.Media3D;
using NXOpen.UF;
using GCD.ViewModel.Base;


namespace GCD.Model
{
	/// <summary>
	/// Description of NXSessionInfo.
	/// </summary>
	public class NXSessionInfo : ViewModelBase
	{
		
		private static NXSessionInfo _self ;
        private ISet<String> mcs_geometry = new HashSet<string>();
        private ISet<String> tool_list = new HashSet<string>();
        private ISet<String> workpice_geometry = new HashSet<string>();
        private ISet<String> program = new HashSet<string>();
        private ISet<String> method = new HashSet<string>();
        private ISet<String> opName = new HashSet<string>();
        private Session theSession = null;
        private UFSession theUFSession = null;
        private Part displayPart = null;
        private Part workPart = null;
        private CAMObject mcsobj = null;
        private CAMObject[] opers = null ;
        private string operation_name;
        private string path_dir;
        private string part_name;
        private string mcs_name ;
        private NXMatrix nmx;
        private Point3d origin ;
        private static Tag part_tag;
        private static Tag oper_tag;
        private NXObject programGroup = null ;
        

        
        
        private NXSessionInfo()
        {
        	
        }
		
       	public Session TheSession
       	{
       		get
       		{ 
       			return theSession ;
       		}
       		set
       		{
       			if(theSession !=null)
       				theSession = value ;
       			
       			RaisePropertyChanged("TheSession") ;
       		}
       	}
       
       	public UFSession TheUFSession
       	{
       		get
       		{ 
       			return theUFSession ;	
       		}
       		set
       		{
       			if(theUFSession !=null)
       				theUFSession = value ;
       			
       			RaisePropertyChanged("TheUFSession") ;
       		}
       		
       	}
       	
       	public String PartName
       	{
       		get { return part_name ;}
       		set { part_name = value ;}
       		
       	}
       	
       	public Tag PartTag
       	{
       		get{return part_tag;}
       		set{part_tag = value;}
       	}
       	
       	public Part GetWorkPart
       	{
       		get{return workPart ;}
       	}


       	public NXSessionInfo(Session session, UFSession ufsession)
       	{
	       	theSession = session ;
	       	theUFSession = ufsession ;

       		workPart = theSession.Parts.Work;
            displayPart = theSession.Parts.Display;
            
            part_tag = theUFSession.Part.AskDisplayPart();
            theUFSession.Part.AskPartName(part_tag, out part_name);
       		
       		OperationCollection opers = displayPart.CAMSetup.CAMOperationCollection;
			NCGroupCollection ncgrc = displayPart.CAMSetup.CAMGroupCollection;
			MCSManager.Instance().McsListClear() ;
			
			foreach (CAMObject camobj in ncgrc.ToArray())
            {
					if (camobj is OrientGeometry)
                	{
                 	   	mcsobj = camobj;
                 	   	mcs_name = camobj.Name ;
                    	mcs_geometry.Add(mcs_name);
                    	workpice_geometry.Add(mcs_name);
                    	
                    	MillOrientGeomBuilder millOrientGeom = displayPart.CAMSetup.CAMGroupCollection.CreateMillOrientGeomBuilder(mcsobj);
                    	CartesianCoordinateSystem cartesianCoordinateSystem;
                    	cartesianCoordinateSystem = millOrientGeom.Mcs;
                    	nmx = cartesianCoordinateSystem.Orientation;
                    	origin = cartesianCoordinateSystem.Origin;
                    	MCSManager.Instance().AddValue(nmx, mcs_name, origin) ;
					}
                	if (camobj is FeatureGeometry)
                	{
                    	workpice_geometry.Add(camobj.Name);
                	}
                	
                	if (camobj is NXOpen.CAM.Operation)
                	{
                	    opName.Add(camobj.Name);
                	    
               // 	    NXSessionManager.Instance.OperationList.Add(camobj) ;
                	    //  MessageBox.Show(camobj.Name);
                	}           
                
			}
			
			RaisePropertyChanged("GetMcsGeometry");
       		RaisePropertyChanged("GetWorkPiceGeometry");
       		RaisePropertyChanged("TheSession");
       	}
		       	
        
          
       	internal ISet<String> GetMcsGeometry
	   	{
		    get
			{
				return mcs_geometry;
			}
			
	   	}
        
        internal ISet<String> GetWorkPiceGeometry
		{
			get
			{
				return workpice_geometry;
			}
			
		}
                
        internal ISet<String> GetToolList
		{
			get
			{
				return tool_list;
			}
			
		}
        
		
//        internal static NXSessionInfo Instance()
//        {
//        	if(_self == null)
//        		_self = new NXSessionInfo();
//        	
//        	return _self ;
//        }
        		
	}
}
