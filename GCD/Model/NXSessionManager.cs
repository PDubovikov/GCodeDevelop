using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using ICSharpCode.AvalonEdit.Document;
using GCD.Model;
using GCD.Command;
using GCD.ViewModel;
using GCD.ViewModel.Base;
using NXOpen;
using NXOpen.CAM;
using NXOpen.UF;


namespace GCD.Model
{
	/// <summary>
	/// Description of NXSessionManager.
	/// </summary>
	public class NXSessionManager : ViewModelBase
	{
		private static NXSessionManager self ;
		private ListingWindow listWindow ;
		private UFSession _ufsession = null ;
		private Session _session = null ;
		private List<CAMObject> _listObject ;
		private Matrix3D mcsData ;
		private Tag partTag, programGroupTag, setupTag, currentProg, currentMcs, geomGroupTag, methodGroupTag, templateOperTag ;
		private Tag[] listProgTag, listOperTag, listGeomTag, listMethodTag ;
		private Matrix3D _mcsDataInfo ;
		private IntPtr templateObject ;
		private string _configFile ;
		private string programName ;
		private string mcsName, subMcsName ;
		private string operName ;
		private string objectName ;
		private int _countProg, _countOper, _countGeom, _countSubGeom ;
		private bool generate ;
		
		
		private NXSessionManager()
		{
			_listObject = new List<CAMObject>() ;
			
		}
		
		public void ExportClsfToNX(string pathClsf, string fileName, bool latheMode)
   		{
			
			if(_ufsession != null)
			{
				
				listWindow = _session.ListingWindow ;
				
				_ufsession.Cam.AskConfigFile(out _configFile) ;
				

				// Program view
				_ufsession.Setup.AskSetup(out setupTag) ;
				_ufsession.Setup.AskProgramRoot(setupTag, out programGroupTag) ;
				_ufsession.Ncgroup.AskMemberList(programGroupTag, out _countProg, out listProgTag) ;
				_ufsession.Obj.AskName(programGroupTag, out programName ) ;
				// Geometry view
				_ufsession.Setup.AskGeomRoot(setupTag, out geomGroupTag) ;
				_ufsession.Ncgroup.AskMemberList(geomGroupTag, out _countGeom, out listGeomTag) ;
				// Method view
				_ufsession.Setup.AskMthdRoot(setupTag, out methodGroupTag) ;
				
				// Create Lathe_user_op
				
				if(latheMode)
				{
					
					_ufsession.Oper.Create("turning", "LATHE_CONTROL", out templateOperTag) ;
					_ufsession.Oper.AskNameFromTag(templateOperTag, out operName) ;
					_ufsession.Obj.SetName(templateOperTag, "LATHE_USER_OP") ;
					
				}
				
				
				// DELETE OLD PROGRAM
				try
				{
					NXObject[] programObj = new NXObject[_countProg] ;
					
					for(int i=0; i<_countProg; i++)
					{
						_ufsession.Obj.AskName(listProgTag[i], out programName) ;
					
						if(programName.Equals(fileName.ToUpper()))
						{							
							currentProg = listProgTag[i] ;
						//	programObj[i] = workPart.CAMSetup.CAMGroupCollection.FindObject(programName) ;
							_ufsession.Obj.DeleteObject(currentProg) ;
							
						}
					}

				}
				catch(Exception ex)
				{
					MessageBox.Show(ex.Message) ;
				}				
				
//				_ufsession.Cam.UpdateSingleObjectCustomization(programGroupTag) ;
//				_ufsession.Cam.ReinitSession(_configFile) ;
				_ufsession.Disp.Refresh() ;
				// Generate program
			//	_ufsession.Param.Reinit();
				_ufsession.Clsf.Import(partTag, pathClsf);
				_ufsession.Ncgroup.AskMemberList(programGroupTag, out _countProg, out listProgTag) ;
			
				for(int i=0; i<_countProg; i++)
				{
					_ufsession.Obj.AskName(listProgTag[i], out programName) ;
					
					if(programName.Equals(fileName.ToUpper()))
					{
						currentProg = listProgTag[i] ;
						_ufsession.Param.ReplayPath(currentProg) ;
					}
				}
				
				//Refresh Manufacturing Program Navigator

				RefreshOperationNavigator();
	
			}
   	//		UFSession.Param.ReplayPath(Tag.Null) ;
   		//	UFSession.Cam.UpdateSingleObjectCustomization(PartTag) ;
   			
   			if(_session != null)
   			{

   			}
   			
   		}
			
				
		public void GetSession(Session session, UFSession ufsession)
		{
			_session = session ;
			_ufsession = ufsession ;
			
			if(_ufsession != null)
				partTag = _ufsession.Part.AskDisplayPart();
			
		}
		
		private void RefreshOperationNavigator()
		{
			UFUiOnt.TreeMode currentOntView;
			_ufsession.UiOnt.AskView(out currentOntView) ;
//			_ufsession.UiOnt.SwitchView(UFUiOnt.TreeMode.MachineMode) ; // Method view 
//			_ufsession.UiOnt.Refresh() ;
//			_ufsession.UiOnt.SwitchView(UFUiOnt.TreeMode.GeometryMode) ; // Geometry vew
//			_ufsession.UiOnt.Refresh() ;
//			_ufsession.UiOnt.SwitchView(UFUiOnt.TreeMode.MachineTool) ; // Tool view
//			_ufsession.UiOnt.Refresh() ;
			_ufsession.UiOnt.SwitchView(UFUiOnt.TreeMode.Order) ; // Group program view
			_ufsession.UiOnt.Refresh() ;
//			_ufsession.UiOnt.SwitchView(currentOntView) ;
			
			//WARNING! method SwitchView may be go to wrong way with NXSession, NX11.0.2.7 Windows7 64bit.
		}
				
		
		public static NXSessionManager Instance
       	{
       		get
       		{
				if(self == null)
       				self = new NXSessionManager();
       		
       			return self;
       		}
       	}
		
	}
}
