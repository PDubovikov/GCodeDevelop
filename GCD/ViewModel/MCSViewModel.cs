using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using NXOpen;
using NXOpen.CAM;
using NXOpen.Utilities;
using System.Windows.Media.Media3D;
using NXOpen.UF;
using GCD.Model;
using GCD.ViewModel.Base;

namespace GCD.ViewModel
{
	/// <summary>
	/// Description of MCSViewModel.
	/// </summary>
	public class MCSViewModel : ViewModelBase
	{
		private MCSInfo _model ;
		private string _name ;
        private Matrix3D mtx;
        private Point3d origin ;
		
        public string Name
        {
            get{ return _name; }
        }
        
        public Matrix3D Matrix
        {
        	get {return mtx; }
        	set { mtx = value; }
        }
        
        public Point3d Origin
        {
        	get { return origin ;}
        	set { origin = value ;}
        }
        		
		public MCSViewModel(MCSInfo i)
		{
			this._name = i.Name ;
			this.mtx = i.Matrix ;
			
		}
				
	}
}
