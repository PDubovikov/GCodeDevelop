using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using NXOpen;
using NXOpen.CAM;
using NXOpen.Utilities;
using System.Windows.Media.Media3D;
using NXOpen.UF;
using GCD.ViewModel.Base;

namespace GCD.Model
{
	
	public class MCSInfo : ViewModelBase		
	{
		private string _name ;
        private Matrix3D mtx;
        private int _index;
        
        public int Index
        {
        	get {return _index;}
        	set {_index = value ;}
        }
		
       	public string Name
       	{
       		get{return _name ;}
       	}
       	      	
       	public MCSInfo(String name, NXMatrix nxm, Point3d origin)
       	{
       		this._name = name ; 
       		this.mtx.M11 = Math.Round(nxm.Element.Xx,12); this.mtx.M12 = Math.Round(nxm.Element.Xy,12);
       		this.mtx.M13 = Math.Round(nxm.Element.Xz,12);
       		this.mtx.M21 = Math.Round(nxm.Element.Yx,12); this.mtx.M22 = Math.Round(nxm.Element.Yy,12); 
       		this.mtx.M23 = Math.Round(nxm.Element.Yz,12);
       		this.mtx.M31 = Math.Round(nxm.Element.Zx,12); this.mtx.M32 = Math.Round(nxm.Element.Zy,12); 
       		this.mtx.M33 = Math.Round(nxm.Element.Zz,12);
            mtx.Invert();
            this.mtx.OffsetX = origin.X; this.mtx.OffsetY = origin.Y; this.mtx.OffsetZ = origin.Z ;
            
            RaisePropertyChanged("Matrix") ;
            RaisePropertyChanged("Name") ;
       	}
       	
       	public Matrix3D Matrix
       	{
       		get {return mtx ;}

       	}
       	      	       		
	}
}
