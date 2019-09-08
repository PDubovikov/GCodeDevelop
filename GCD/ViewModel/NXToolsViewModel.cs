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
using NXOpen;
using NXOpen.CAM;
using NXOpen.UF;

namespace GCD.ViewModel
{
  	class NXToolsViewModel : Base.ToolViewModel
  	{
  		
  		private static NXToolsViewModel self ;
  		private ObservableCollection<CoordinatOffsetViewModel> list;
  		private ObservableCollection<MCSViewModel> mcsList;
  		private int listValue;
  		private bool iscaminit ;
  		private Session _session ;
		private UFSession _ufsession;
		private NXSessionInfo nxSessionInfo ;
		
  		
 		
  		private NXToolsViewModel(): base("NXTools")
    	{
  			//	Workspace.This.ActiveDocumentChanged += new EventHandler(OnActiveDocumentChanged);
  			//  MCSManager.Instance().PropertyChanged += (s, e) => { RaisePropertyChanged(e.PropertyName); };
  			_session = (Session)Activator.GetObject(typeof(Session), "http://localhost:4567/NXOpenSession");
    		_ufsession = (UFSession)Activator.GetObject(typeof(UFSession), "http://localhost:4567/UFSession");

    		UpdateNXSession();
    			
  			CoordinatOffsetManager.Instance().PropertyChanged += (s, e) => {

                				//populate initial data
               		 			list = new ObservableCollection<CoordinatOffsetViewModel>();
                				foreach (Model.CoordinatOffsetInfo i in Model.CoordinatOffsetManager.Instance().GetCoordinatSystem)
                    				list.Add(new CoordinatOffsetViewModel(i));
                				
                				listValue = list.Count ;

//                				foreach(var mcs in list )
//                				{
//                					MessageBox.Show(mcs.ID.ToString() + " " + mcs.Name) ;
//                				}
								RaisePropertyChanged("List") ;
								RaisePropertyChanged("IsListValue") ;
								
  				            
  			};
    					
  		}
  		
  		public Session NXSession
		{
			get
			{	
				if(_session != null)
					return _session ;
				
				return _session;
				
			}
		}
		
		public UFSession UFSession
		{
			get
			{
				if(_session != null)
					return _ufsession;
				
				return _ufsession ;
			}
		}
		
		public bool SessionIsChecked
		{
			get
			{					
				try
				{
					_ufsession.Cam.IsSessionInitialized(out iscaminit) ;
				}
				catch(Exception ex)
				{
					return false ;
				}		

				return iscaminit ;
			}
			
		}
  		
        public ObservableCollection<CoordinatOffsetViewModel> List
        {
            get 
            { 
            	return list; 
            }
        }
        
        public ObservableCollection<MCSViewModel> McsList
        {
            get 
            { 
            	return mcsList; 
            }
        }
        
        private Matrix3D _mcsData ;
        public Matrix3D McsData 
    	{
    		get
    		{	
    			return _mcsData ;
    		}
    		set
    		{
    			_mcsData = value;  // write value from combobox selected value from view
    			RaisePropertyChanged("McsData") ;
    		}
    		
    	}
        		
    	
    	public string SessionName
    	{
    		get
    		{
    			if(nxSessionInfo != null)
    				return nxSessionInfo.PartName ;
    			
    			return String.Empty ;
    		}
    	}
    	
//    	public Tag PartTag
//    	{
//    		get
//    		{
//    			if(nxSessionInfo != null)
//    				return nxSessionInfo.PartTag ;
//    			
//    			return Tag.Null ;
//    		}
//    	}
    	
		private int _index ;
    	public int IndexSelected
    	{
    		get { return 0 ;}
    		set { _index = value ; }
    	}
    	    	
        
        RelayCommand _updateSessionCommand = null;
    	public ICommand UpdateUFSession
    	{
      		get
      		{
        		if (_updateSessionCommand == null)
        		{
        			_updateSessionCommand = new RelayCommand((p) => UpdateNXSession(), (p) => true);
        		}

        		return _updateSessionCommand;
      		}
    	}
    
    	private void UpdateNXSession()
    	{
    	//	MessageBox.Show("Update data from NXSession") ;

    		if (SessionIsChecked)
    		{
    			nxSessionInfo = new NXSessionInfo(_session, _ufsession) ;
    			NXSessionManager.Instance.GetSession(_session, _ufsession) ;

					mcsList = new ObservableCollection<MCSViewModel>();

                			foreach (Model.MCSInfo i in Model.MCSManager.Instance().GetMcsInfo)
                   				mcsList.Add(new MCSViewModel(i));
                	
                				RaisePropertyChanged("McsList") ;
                				RaisePropertyChanged("SessionName");
                				RaisePropertyChanged("IndexSelected");
                				RaisePropertyChanged("UFSession") ;
    		}

    	}
        
        
        private bool _isActive = false ;
   		public bool IsDocActive
   		{
   			get { return _isActive ;}
   			set
   			{
   				if(_isActive != value )
   				{
   					_isActive = value ;
   					RaisePropertyChanged("IsDocActive") ;
   				}
   			}
   		}
   		
   		private bool islistValue = false ;
   		public bool IsListValue
   		{
   			get 
   			{
   				if(listValue != 0)
   					return true ;
   				
   				return false ;
   			}
   		}
   		   		
   		public static NXToolsViewModel Instance
       	{
       		get
       		{	
       			if(self == null)
       				self = new NXToolsViewModel();
       		
       			return self;
       		}
       	}
   		
  	}
}