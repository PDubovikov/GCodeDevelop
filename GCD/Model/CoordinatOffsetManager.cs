using System;
using System.Collections.Generic;
using System.Collections.ObjectModel ;
using System.Windows.Media.Media3D;
using System.Linq;
using System.Text;
using GCD.ViewModel.Base;
using System.Windows ;

namespace GCD.Model
{
	/// <summary>
	/// Description of MCSInfo.
	/// </summary>
	public class CoordinatOffsetManager : ViewModelBase
	{
		private static CoordinatOffsetManager self ;
		
		private Dictionary<int, CoordinatOffsetInfo> dict ; 
		private ISet<String> listOffset ;
		
		private CoordinatOffsetManager()
		{
			dict = new Dictionary<int, CoordinatOffsetInfo>() ;
			listOffset =  new HashSet<String>() ;
		}
		
		
		public void AddValue(ISet<String> value)			
		{
			dict.Clear() ;
			bool updateProp = false;
			int i = 0;
			
			if(!value.SequenceEqual(listOffset))
			{
				listOffset = value ;
				updateProp = true;
			}
			
			foreach(var offset in value)
			{	
											
				dict.Add(i, new CoordinatOffsetInfo(){ Id = i, Name = offset} ) ;
				i++ ;
				
				if(!listOffset.Contains(offset))
					updateProp = true ;
					
				listOffset.Add(offset) ;				
				
			}
			 						
			if(updateProp)
				RaisePropertyChanged("GetCoordinatSystem") ;
			
		}
				
				
		internal static CoordinatOffsetManager Instance()
        {
            if (self == null)
                self = new CoordinatOffsetManager();
            return self;
        }
		
	
		
		internal IEnumerable<CoordinatOffsetInfo> GetCoordinatSystem
		{
			get
			{
				return dict.Values.ToList() ;

			}
			
		}
								
	}
}
