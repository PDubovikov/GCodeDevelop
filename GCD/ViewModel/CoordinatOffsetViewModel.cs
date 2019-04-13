using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;
using System.IO;
using GCD.Model;

namespace GCD.ViewModel
{
	/// <summary>
	/// Description of MCSViewModel.
	/// </summary>
	class CoordinatOffsetViewModel: Base.ViewModelBase
	{
		private string mcsName ;
		//private ISet<MCSInfo> mcsInfo ;
		private int Id;
		private string imagePath;
		
		public int ID
        {
            get { return Id; }
            set { Id = value; }
        }
		
		public string Name
        {
            get { return mcsName; }
            set { mcsName = value; }
        }
		
		public string ImagePath
        {
            get { return imagePath; }
            set { imagePath = value; }
        }
					
		public CoordinatOffsetViewModel(CoordinatOffsetInfo i)
		{
            this.ID = i.Id;
            this.Name = i.Name;
   //         this.ImagePath = Path.GetFullPath("Images/" + "MCS2" + ".png");
		}
				

	}
}
