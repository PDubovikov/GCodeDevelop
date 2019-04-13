using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using NXOpen;
using NXOpen.CAM;
using NXOpen.Utilities;
using System.Windows.Media.Media3D;
using NXOpen.UF;
using GCD.ViewModel.Base;

namespace GCD.Model
{
	/// <summary>
	/// Description of MCSManager.
	/// </summary>
	public class MCSManager : ViewModelBase
	{
		private static MCSManager self;
        private Dictionary<String, MCSInfo> mcsList = new Dictionary<String, MCSInfo>();
		
		
		private MCSManager()
		{
			
		}
		
		internal static MCSManager Instance()
       	{
       		if(self == null)
       			self = new MCSManager();
       		
       		return self; 
       	}
		
		public void AddValue(NXMatrix nxm, String name, Point3d pt)
		{	   
				
				mcsList.Add(name, new MCSInfo(name, nxm, pt)) ;
             //  mcsList[name] = new MCSInfo(name, nxm, pt) ;
			
               RaisePropertyChanged("GetMcsInfo") ;   
		}
		
		public void McsListClear()
		{
			mcsList.Clear() ;
		}
		
					
		
		internal IEnumerable<MCSInfo> GetMcsInfo
		{
			get
			{
				return mcsList.Values.ToList() ;
			}
		}
		
	}
}
