using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;

namespace GCD.ViewModel
{

	class ErrorsViewModel : Base.ToolViewModel
	{
		public const string ToolContentId = "ErrorsTool";
		
		
		public ErrorsViewModel() : base("Errors")
		{
		//	Workspace.This.ActiveDocumentChanged += new EventHandler(OnActiveDocumentChanged);     
      //		ContentId = ToolContentId;
            
       		BitmapImage bi = new BitmapImage();
      		bi.BeginInit();
      		bi.UriSource = new Uri("pack://application:,,/Images/warn2.png");
      		bi.EndInit();
      		IconSource = bi;
      		IsDocActive = false ;
		}
		
		
		void OnActiveDocumentChanged(object sender, EventArgs e)
    	{
			if (Workspace.This.ActiveDocument != null)
			{
			//	IsDocActive = false ;
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
   		
   	    #region ConsoleDocument
    
    	private TextDocument _errorsOutputDocument = null;
    	public TextDocument ErrorsOutputDocument
    	{
    		get { return _errorsOutputDocument ;}
    		set
    		{
    			if(_errorsOutputDocument != value)
    			{
    				_errorsOutputDocument = value;
    				RaisePropertyChanged("ErrorsOutputDocument") ;
    			}
    		
    		}
    	
    	}
    	#endregion
		
	}
}
