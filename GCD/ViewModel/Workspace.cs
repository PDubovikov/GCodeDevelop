using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.AvalonDock ;
using Xceed.Wpf.AvalonDock.Layout;
using GCD.ViewModel ;
using GCD.ViewModel.Base;
using GCD.Command;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using Xceed.Wpf.AvalonDock.Themes;


namespace GCD.ViewModel
{
		
  class Workspace : Base.ViewModelBase
  {
    protected Workspace()
    {
    	InitSession();
    }

    static Workspace _this = new Workspace();

    public static Workspace This
    {
    	get { return _this; }
    }
	
    ObservableCollection<FileViewModel> _files = new ObservableCollection<FileViewModel>();
    ReadOnlyObservableCollection<FileViewModel> _readonyFiles = null;
   
    public ReadOnlyObservableCollection<FileViewModel> Files
    {
      get
      {
        if (_readonyFiles == null)
          _readonyFiles = new ReadOnlyObservableCollection<FileViewModel>(_files);

        return _readonyFiles;
      }
    }
        
    ToolViewModel[] _tools = null;

    public IEnumerable<ToolViewModel> Tools
    {
      get
      {
        if (_tools == null)
          _tools = new ToolViewModel[] { FileStats, NXTools, Errors };
        return _tools;
      }
    }   

    FileStatsViewModel _fileStats = null;
    public FileStatsViewModel FileStats
    {
      get
      {
        if (_fileStats == null)
          _fileStats = new FileStatsViewModel();

        return _fileStats;
      }
    }
    
    NXToolsViewModel _nxTools = null;
    public NXToolsViewModel NXTools
    {
      get
      {
        if (_nxTools == null)
        	_nxTools = NXToolsViewModel.Instance;

        return _nxTools;
      }
    }
    
    ErrorsViewModel _errors = null;
    public ErrorsViewModel Errors
    {
      get
      {
        if (_errors == null)
          _errors = new ErrorsViewModel();

        return _errors;
      }
    }
    
   
    #region OpenCommand
    RelayCommand _openCommand = null;
    public ICommand OpenCommand
    {
      get
      {
        if (_openCommand == null)
        {
          _openCommand = new RelayCommand((p) => OnOpen(p), (p) => CanOpen(p));
        }

        return _openCommand;
      }
    }

    private bool CanOpen(object parameter)
    {
      return true;
    }

    private void OnOpen(object parameter)
    {
      var dlg = new OpenFileDialog();
      if (dlg.ShowDialog().GetValueOrDefault())
      {
      	string path = dlg.FileName ;
        var fileViewModel = Open(dlg.FileName);
        ActiveDocument = fileViewModel;
      }
    }

    public FileViewModel Open(string filepath)
    {
      var fileViewModel = _files.FirstOrDefault(fm => fm.FilePath == filepath);
      if (fileViewModel != null)
        return fileViewModel;

      fileViewModel = new FileViewModel(filepath);
      _files.Add(fileViewModel);
      ActiveDocument = fileViewModel;

      return fileViewModel;
    }

    #endregion

    #region NewCommand
    RelayCommand _newCommand = null;
    public ICommand NewCommand
    {
      get
      {
        if (_newCommand == null)
        {
          _newCommand = new RelayCommand((p) => OnNew(p), (p) => CanNew(p));
        }

        return _newCommand;
      }
    }
  	
    private bool CanNew(object parameter)
    {
      return true;
    }

    private void OnNew(object parameter)
    {
      _files.Add(new FileViewModel() { Document = new TextDocument() });
      ActiveDocument = _files.Last();
      
    }
	
    #endregion
    
    #region LoadFile
    RelayCommand _loadFile = null;
    public ICommand LoadFileCommand
    {
    	get
    	{
    		if (_loadFile == null)
    		{
    			_loadFile = new RelayCommand((p) => OnLoad(p), (p) => true) ;
    		}
    		
    		return _loadFile;
    	}
    }
    
    private void OnLoad(object parameter)
    {
    	string filePath = (string)parameter ;
		if (File.Exists(filePath))
		{
		    var fileViewModel = Open(filePath);
	      	ActiveDocument = fileViewModel;
		}    	
    }
    
    #endregion

    #region ChangeThemeCommand
    
    RelayCommand _changeThemeCommand = null;
    public ICommand ChangeThemeCommand
    {
      get
      {
        if (_changeThemeCommand == null)
        {
          _changeThemeCommand = new RelayCommand((p) => ChangeTheme(p), (p) => CanChange(p));
        }

        return _changeThemeCommand;
      }
      
    }    
    
    private bool CanChange(object parameter)
    {
      	return true;
    }
    
	private void ChangeTheme(object parameter)
	{
		string name = (string)parameter ;
		switch(name)
		{
			case "Generic":
				CurrentTheme = new GenericTheme() ;
				break ;
			case "Aero":
				CurrentTheme = new AeroTheme() ;
				break ;
			case "Metro":
				CurrentTheme = new MetroTheme() ;
				break ;
			case "VS2010":
				CurrentTheme = new VS2010Theme() ;
				break ;
			case "ExpressionDark":
				CurrentTheme = new ExpressionDarkTheme() ;
				break;
			case "ExpressionLight":
				CurrentTheme = new ExpressionLightTheme() ;
				break ;
		}
	}
	
	private Theme _currentTheme = null ;
	public Theme CurrentTheme
	{
		get{ return _currentTheme ;}
		set
		{
			if (_currentTheme != value)
			{
				_currentTheme = value ;
				RaisePropertyChanged("CurrentTheme");
			}	
		}
	}
		
    #endregion      
    
    #region ActiveDocument

    private FileViewModel _activeDocument = null;
    public FileViewModel ActiveDocument
    {
      get { return _activeDocument; }
      set
      {
        if (_activeDocument != value)
        {
          _activeDocument = value;
          RaisePropertyChanged("ActiveDocument");
          if (ActiveDocumentChanged != null) 				// привязка события к смене активного документа
            ActiveDocumentChanged(this, EventArgs.Empty);
        }
      }
    }

    public event EventHandler ActiveDocumentChanged;

    
    #endregion

    #region ADLayout
    private AvalonDockLayoutViewModel mAVLayout = null;

    /// <summary>
    /// Expose command to load/save AvalonDock layout on application startup and shut-down.
    /// </summary>
    public AvalonDockLayoutViewModel ADLayout
    {
      get
      {
        if (this.mAVLayout == null)
          this.mAVLayout = new AvalonDockLayoutViewModel();

        return this.mAVLayout;
      }
    }

    public static string LayoutFileName
    {
      get
      {
        return "Layout.config";
      }
    }
    #endregion ADLayout

    #region Application Properties
    /// <summary>
    /// Get a path to the directory where the application
    /// can persist/load user data on session exit and re-start.
    /// </summary>
    public static string DirAppData
    {
      get
      {
        string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                         System.IO.Path.DirectorySeparatorChar;

        try
        {
          if (System.IO.Directory.Exists(dirPath) == false)
            System.IO.Directory.CreateDirectory(dirPath);
        }
        catch
        {
        	
        }

        return dirPath;
      }
    }
    
	internal static void InitSession()
    {
    	HighlightingExtension.RegisterCustomHighlightingPatterns();
    }
    
	private bool topmost = false ;
	public bool Topmost
	{
		get { return topmost ;}
		set
		{
			if (topmost != value)
			{
				topmost = value ;
				RaisePropertyChanged("Topmost") ;
			}	
		}
	}
	
    #endregion Application Properties

    #region close save file handling methods
    internal void Close(FileViewModel fileToClose)
    {
      if (fileToClose.IsDirty)
      {
        var res = MessageBox.Show(string.Format("Save changes for file '{0}'?", fileToClose.FileName), "AvalonDock Test App", MessageBoxButton.YesNoCancel);
        if (res == MessageBoxResult.Cancel)
          return;
        if (res == MessageBoxResult.Yes)
        {
          Save(fileToClose);
        }

      }

      _files.Remove(fileToClose);
      	if(_files.Count.Equals(0))
      	{
             ActiveDocument = null;
             FileStats.ConsoleDocument = null ;
      	}
    }

    internal void Save(FileViewModel fileToSave, bool saveAsFlag = false)
    {
      if (fileToSave.FilePath == null || saveAsFlag)
      {
        var dlg = new SaveFileDialog();
        dlg.DefaultExt = ".txt";
        dlg.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*|mpf files (*.mpf)|*.mpf";
        dlg.FilterIndex = 1 ;
        if (dlg.ShowDialog().GetValueOrDefault())
          fileToSave.FilePath = dlg.FileName;
      }
	  
      if (!String.IsNullOrEmpty(fileToSave.FilePath))
      		File.WriteAllText(fileToSave.FilePath, fileToSave.Document.Text);
      
      ActiveDocument.IsDirty = false; 
    }
    #endregion close save file handling methods
     
    internal void Exit()
    {
    	App.Current.Shutdown();
    }
  }
}
