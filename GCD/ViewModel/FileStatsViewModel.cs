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

  class FileStatsViewModel : Base.ToolViewModel
  {
    public FileStatsViewModel(): base("Output")
    {
      Workspace.This.ActiveDocumentChanged += new EventHandler(OnActiveDocumentChanged);     
      ContentId = ToolContentId;
      
      BitmapImage bi = new BitmapImage();
      bi.BeginInit();
      bi.UriSource = new Uri("pack://application:,,/Images/property-blue.png");
      bi.EndInit();
      IconSource = bi;
      IsDocActive = false ;
      
    }
	
    public const string ToolContentId = "FileStatsTool";

    void OnActiveDocumentChanged(object sender, EventArgs e)
    {
      if (Workspace.This.ActiveDocument != null &&
          Workspace.This.ActiveDocument.FilePath != null &&
          File.Exists(Workspace.This.ActiveDocument.FilePath))
      {
        IsDocActive = false ;
        ConsoleDocument = null ;
      }
      else
      {
      	IsDocActive = false ;
      	ConsoleDocument = null ;
      }
      
    }
    

    #region FileSize

    private long _fileSize;
    public long FileSize
    {
      get { return _fileSize; }
      set
      {
        if (_fileSize != value)
        {
          _fileSize = value;
          RaisePropertyChanged("FileSize");
        }
      }
    }

    #endregion
    
    #region ConsoleDocument
    
    private TextDocument _consoleDocument = null;
    public TextDocument ConsoleDocument
    {
    	get { return _consoleDocument ;}
    	set
    	{
    		if(_consoleDocument != value)
    		{
    			_consoleDocument = value;
    			RaisePropertyChanged("ConsoleDocument") ;
    		}
    		
    	}
    	
    }
    #endregion
	
   	#region
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
   	
    #endregion
    
    #region LastModified

    private DateTime _lastModified;
    public DateTime LastModified
    {
      get { return _lastModified; }
      set
      {
        if (_lastModified != value)
        {
          _lastModified = value;
          RaisePropertyChanged("LastModified");
        }
      }
    }

    #endregion


  }
}
