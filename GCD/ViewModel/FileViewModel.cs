using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Threading;
using GCD.Command;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Utils;
using ICSharpCode.AvalonEdit.Highlighting;
using GCD.Model;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using SinumerikLanguage.Antlr4;
using gcodeparser ;


namespace GCD.ViewModel
{

  class FileViewModel : PaneViewModel
  {
    #region fields
    static ImageSourceConverter ISC = new ImageSourceConverter();
    #endregion fields
	CancellationTokenSource cts ;
    
    #region constructor
    public FileViewModel(string filePath)
    {
      FilePath = filePath;
      Title = FileName;
      IconSource = ISC.ConvertFromInvariantString(@"pack://application:,,/Images/document.png") as ImageSource;
    }

    public FileViewModel()
    {
      IsDirty = true;
      Title = FileName;
      WordWrap = false; 
      ShowLineNumbers = true ;
      FontSize = 14 ;
      HighlightDef = HighlightingManager.Instance.GetDefinition("Sinumerik 840D_Mill");
      
    }
    #endregion constructor

    #region FilePath
    private string _filePath = null;
    public string FilePath
    {
      get { return _filePath; }
      set
      {
        if (_filePath != value)
        {
          _filePath = value;
          RaisePropertyChanged("FilePath");
          RaisePropertyChanged("FileName");
          RaisePropertyChanged("Title");

          if (File.Exists(this._filePath))
          {
            this._document = new TextDocument();
         //   HighlightingExtension.RegisterCustomHighlightingPatterns();
            this.HighlightDef = HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(_filePath));
            this._isDirty = false;
            this.IsReadOnly = false;
            this.ShowLineNumbers = true;
            this.WordWrap = false;
            this.FontSize = 14 ;

            // Check file attributes and set to read-only if file attributes indicate that
            if ((System.IO.File.GetAttributes(this._filePath) & FileAttributes.ReadOnly) != 0)
            {
              this.IsReadOnly = true;
              this.IsReadOnlyReason = "This file cannot be edit because another process is currently writting to it.\n" +
                                      "Change the file access permissions or save the file in a different location if you want to edit it.";
            }

            using (FileStream fs = new FileStream(this._filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
              using (StreamReader reader = FileReader.OpenStream(fs, Encoding.UTF8))
              {
                this._document = new TextDocument(reader.ReadToEnd());
              }
            }

            ContentId = _filePath;
                       
          }
        }
      }
    }
    #endregion

    #region FileName
    public string FileName
    {
      get
      {
        if (FilePath == null)
          return "Noname" + (IsDirty ? "*" : "");

        return System.IO.Path.GetFileName(FilePath) + (IsDirty ? "*" : "");
        
      }
    }
    #endregion FileName
	
    public string FileNameExceptExtension
    {
    	get
    	{
    		if (FilePath == null)
    			return "Noname";
    		
    		return Path.GetFileNameWithoutExtension(FilePath) ;
    	}
    }
    
    public string FileDirectoryName
    {
    	get
    	{
    		if (FilePath == null)
    			return Path.GetTempPath();
    		
    		return Path.GetDirectoryName(FilePath) + "\\" ;
    		
    	}
    }
    
    #region TextContent

    private TextDocument _document = null;
    public TextDocument Document
    {
      get { return this._document; }
      set
      {
        if (this._document != value)
        {
          this._document = value;
          RaisePropertyChanged("Document");
          IsDirty = false;
        }
      }
    }

    #endregion

    #region HighlightingDefinition

    private IHighlightingDefinition _highlightdef = null;
    public IHighlightingDefinition HighlightDef
    {
      get { return this._highlightdef; }
      set
      {
        if (this._highlightdef != value)
        {
          this._highlightdef = value;
          RaisePropertyChanged("HighlightDef");
         // IsDirty = true;
        }
      }
    }
    
    #endregion
    private string textField = string.Empty ;
    public String TextField
    {
    	get { return textField ;	}
    	set
    	{
    		if(textField != value)
    		{
    			textField = value ;
    			RaisePropertyChanged("TextField");
    		}
    	}
    }

    #region Title
    /// <summary>
    /// Title is the string that is usually displayed - with or without dirty mark '*' - in the docking environment
    /// </summary>
    public string Title
    {
      get
      {
      	if (FilePath != null)
        	return System.IO.Path.GetFileName(this.FilePath) + (this.IsDirty == true ? "*" : string.Empty);
      	
      	return "Noname" + (IsDirty ? "*" : "");

      }

      set
      {
        base.Title = value;
      }
    }
    #endregion

    #region IsDirty

    private bool _isDirty = false;
    public bool IsDirty
    {
      get { return _isDirty; }
      set
      {
        if (_isDirty != value)
        {
          _isDirty = value;
          RaisePropertyChanged("IsDirty");
          RaisePropertyChanged("Title");
          RaisePropertyChanged("FileName");
        }
      }
    }

    #endregion

    #region IsReadOnly
    private bool mIsReadOnly = false;
    public bool IsReadOnly
    {
      get
      {
        return this.mIsReadOnly;
      }

      protected set
      {
        if (this.mIsReadOnly != value)
        {
          this.mIsReadOnly = value;
          this.RaisePropertyChanged("IsReadOnly");
        }
      }
    }

    private string mIsReadOnlyReason = string.Empty;
    public string IsReadOnlyReason
    {
      get
      {
        return this.mIsReadOnlyReason;
      }

      protected set
      {
        if (this.mIsReadOnlyReason != value)
        {
          this.mIsReadOnlyReason = value;
          this.RaisePropertyChanged("IsReadOnlyReason");
        }
      }
    }
    #endregion IsReadOnly

    #region WordWrap
    // Toggle state WordWrap
    private bool mWordWrap = false;
    public bool WordWrap
    {
      get
      {
        return this.mWordWrap;
      }

      set
      {
        if (this.mWordWrap != value)
        {
          this.mWordWrap = value;
          this.RaisePropertyChanged("WordWrap");
        }
      }
    }
    #endregion WordWrap

    #region ShowLineNumbers
    // Toggle state ShowLineNumbers
    private bool mShowLineNumbers = false;
    public bool ShowLineNumbers
    {
      get
      {
        return this.mShowLineNumbers;
      }

      set
      {
        if (this.mShowLineNumbers != value)
        {
          this.mShowLineNumbers = value;
          this.RaisePropertyChanged("ShowLineNumbers");
        }
      }
    }
    #endregion ShowLineNumbers
	
    #region TextEditorOptions
    private ICSharpCode.AvalonEdit.TextEditorOptions mTextOptions
      = new ICSharpCode.AvalonEdit.TextEditorOptions()
      {
        ConvertTabsToSpaces= false,
        IndentationSize = 2,
        EnableTextDragDrop = true
      };

    public ICSharpCode.AvalonEdit.TextEditorOptions TextOptions
    {
      get
      {
        return this.mTextOptions;
      }

      set
      {
        if (this.mTextOptions != value)
        {
          this.mTextOptions = value;
          this.RaisePropertyChanged("TextOptions");

        }
      }
    }
    #endregion TextEditorOptions
	
    private double fontSize ;
    public double FontSize
    {
    	get
    	{
    		return fontSize ;
    	}
    	set
    	{	
    			this.fontSize = value ;
    			this.RaisePropertyChanged("FontSize") ;
    	}
    }
        
    #region SaveCommand
    RelayCommand _saveCommand = null;
    public ICommand SaveCommand
    {
      get
      {
        if (_saveCommand == null)
        {
          _saveCommand = new RelayCommand((p) => OnSave(p), (p) => CanSave(p));
        }

        return _saveCommand;
      }
    }

    private bool CanSave(object parameter)
    {
         if (Document == null)
             return false;

            return IsDirty;
    }

    private void OnSave(object parameter)
    {
      Workspace.This.Save(this, false);
    }

    #endregion

    #region SaveAsCommand
    RelayCommand _saveAsCommand = null;
    public ICommand SaveAsCommand
    {
      get
      {
        if (_saveAsCommand == null)
        {
          _saveAsCommand = new RelayCommand((p) => OnSaveAs(p), (p) => CanSaveAs(p));
        }

        return _saveAsCommand;
      }
    }

    private bool CanSaveAs(object parameter)
    {
      return IsDirty;
    }

    private void OnSaveAs(object parameter)
    {
      Workspace.This.Save(this, true);
    }

    #endregion

    #region CloseCommand
    RelayCommand _closeCommand = null;
    public ICommand CloseCommand
    {
      get
      {
        if (_closeCommand == null)
        {
          _closeCommand = new RelayCommand((p) => OnClose(), (p) => CanClose());
        }

        return _closeCommand;
      }
    }

    private bool CanClose()
    {
      return true;
    }

    private void OnClose()
    {
      Workspace.This.Close(this);
    }
    #endregion
    
    RelayCommand _fontScaleCommand = null;
    public ICommand FontScaleCommand
    {
      get
      {
        if (_fontScaleCommand == null)
        {
        	_fontScaleCommand = new RelayCommand((p) => ExecuteFontScale((MouseWheelEventArgs)p), (p) => true);
        }

        return _fontScaleCommand;
      }
    }
    
    private void ExecuteFontScale(MouseWheelEventArgs e)
    {
    	if (Keyboard.PrimaryDevice.IsKeyDown(Key.LeftCtrl) )
    	{
    		
    		if(e.Delta > 0)
    		{
    			FontSize += 0.5 ;	
    		}
    		else if (e.Delta < 0)
    		{
    			FontSize -= 0.5 ;
    		}
    		
    		if (FontSize <= 6) { FontSize = 6 ;}
    		if (FontSize >= 48) { FontSize = 48 ; }
    	}
    }
    
    private bool strProcessEnabled = true;
    public bool StartProcessEnabled
    {
    	get {return strProcessEnabled ; }
    	set
    	{
    		this.strProcessEnabled = value;
    		this.RaisePropertyChanged("StartProcessEnabled") ;
    	}
    }
    
    RelayCommand _startProcess = null;
    public ICommand StartProcess
    {
      get
      {
        if (_startProcess == null)
        {
        	_startProcess = new RelayCommand((p) => Processing(p), (p) => strProcessEnabled);
        }

        return _startProcess;
      }
    }
    
    RelayCommand _stopProcess = null;
    public ICommand StopProcess
    {
    	get
    	{
    		if(_stopProcess == null)
    		{
    			_stopProcess = new RelayCommand((p) => ProcessingStop(p), (p) => true);
    		}
    		
    		return _stopProcess;
    	}
    }
    
	async void Processing(object param)
	{
			strProcessEnabled = false ; 
			System.Threading.CancellationToken cancellationToken ;
			cts = new CancellationTokenSource() ;
			cancellationToken = cts.Token ;
			string Controller = param.ToString() ;
			
			switch(Controller)
			{
				case "Sinumerik 840D_Mill":
				{
					string _fullPath = FileDirectoryName + FileNameExceptExtension + "_S840D_Mill.cls" ;	
					try 
					{
						await sin840_calculate_mill(cancellationToken);							
						if(File.Exists(_fullPath))
						{
							Workspace.This.FileStats.ConsoleDocument = new TextDocument(File.ReadAllText(_fullPath)) ;
							Workspace.This.FileStats.IsDocActive = true ;
						}
					}
					catch(Exception ex)
					{
						cts.Cancel();
						MessageBox.Show(ex.Message) ;
					}
					break;
				}
				case "Sinumerik 840D_Turn":
				{
					string __fullPath = FileDirectoryName + FileNameExceptExtension + "_S840D_Turn.cls" ;	
					try 
					{
							await sin840_calculate_turn(cancellationToken);
							
							if(File.Exists(__fullPath))
							{
								Workspace.This.FileStats.ConsoleDocument = new TextDocument(File.ReadAllText(__fullPath)) ;
								Workspace.This.FileStats.IsDocActive = true ;
							}
					}
					catch(Exception ex)
					{
						cts.Cancel();
						MessageBox.Show(ex.Message) ;
					}
					break;
				}
				case "Sinumerik 840D_Mill_Adv":
				{
					string _fullPath = FileDirectoryName + FileNameExceptExtension + "_S840D_Mill_Adv.cls" ;	
					try 
					{
						await sin840_calculate_mill_adv(cancellationToken);							
						if(File.Exists(_fullPath))
						{
							Workspace.This.FileStats.ConsoleDocument = new TextDocument(File.ReadAllText(_fullPath)) ;
							Workspace.This.FileStats.IsDocActive = true ;
						}
					}
					catch(Exception ex)
					{
						cts.Cancel();
						MessageBox.Show(ex.Message) ;
					}
					break;
				}	
				case "Sinumerik 840D_Turn_Adv":
				{
					string __fullPath = FileDirectoryName + FileNameExceptExtension + "_S840D_Turn.cls" ;	
					try 
					{
							await sin840_calculate_turn_adv(cancellationToken);
							
							if(File.Exists(__fullPath))
							{
								Workspace.This.FileStats.ConsoleDocument = new TextDocument(File.ReadAllText(__fullPath)) ;
								Workspace.This.FileStats.IsDocActive = true ;
							}
					}
					catch(Exception ex)
					{
						cts.Cancel();
						MessageBox.Show(ex.Message) ;
					}
					break;
				}
					
				case "Sinumerik 520K":
				{
		//			await sin520_calculate();
					break;
				}
				case "Mayak 600T":
				{
					string __fullPath = FileDirectoryName + FileNameExceptExtension + "_M600_Turn.cls" ;	
					try 
					{
							await mayak600_calculate_turn(cancellationToken);
							
							if(File.Exists(__fullPath))
							{
								Workspace.This.FileStats.ConsoleDocument = new TextDocument(File.ReadAllText(__fullPath)) ;
								Workspace.This.FileStats.IsDocActive = true ;
							}
					}
					catch(Exception ex)
					{
						cts.Cancel();
						MessageBox.Show(ex.Message) ;
					}
					break;
				}
				case "Okuma Mill":
				{
					string _fullPath = FileDirectoryName + FileNameExceptExtension + "_Okuma_Mill.cls" ;	
					try 
					{
						await okuma_calculate_mill(cancellationToken);							
						if(File.Exists(_fullPath))
						{
							Workspace.This.FileStats.ConsoleDocument = new TextDocument(File.ReadAllText(_fullPath)) ;
							Workspace.This.FileStats.IsDocActive = true ;
						}
					}
					catch(Exception ex)
					{
						cts.Cancel();
						MessageBox.Show(ex.Message) ;
					}					
											
					break;
				}
					
			}
				
			strProcessEnabled = true ;				
	}
	
	
	void ProcessingStop(object param)
	{
		
		if(cts != null)
		{
			cts.Cancel();

		}
	}
	
	private TextDocument _consoleText = null ;
	public TextDocument ConsoleText
	{
		get { return _consoleText ; }
		set
		{
			if (_consoleText != value)
			{
				_consoleText = value ;
				RaisePropertyChanged("ConsoleText") ;
				if (StringBufferChanged != null)
					StringBufferChanged(this, EventArgs.Empty);
			}
		}
	}
	
	public event EventHandler StringBufferChanged;
	
	Task sin840_calculate_mill(System.Threading.CancellationToken cancellationToken)
	{
			StringBuilder strb1 = new StringBuilder(Document.Text) ;
			string _fullPath = FileDirectoryName + FileNameExceptExtension + "_S840D_Mill.cls" ;
			string _fileNameAddPrefix = FileNameExceptExtension + "_S840D_Mill" ;
			
		//	File.WriteAllText("Program_S840_Text.txt", strb1.ToString()) ;
			StringBuilder paramprog = new StringBuilder() ;
			return Task.Run(() => {			    			
				MachineControl_CLS mcontrolCLS = new MachineControl_CLS();
				GCodeParser parser = new GCodeParser(null, strb1, "Sinumerik", mcontrolCLS);
				
				File.WriteAllText(_fullPath, mcontrolCLS.SCM_CW.ToString().Replace(';',',')) ;

				
				NXSessionManager.Instance.ExportClsfToNX(_fullPath, _fileNameAddPrefix, false) ;	
				
			}, cancellationToken
			);
			
	}
	
	Task sin840_calculate_mill_adv(System.Threading.CancellationToken cancellationToken)
	{
			string _fullPath = FileDirectoryName + FileNameExceptExtension + "_S840D_Mill_Adv.cls" ;
			string _fileNameAddPrefix = FileNameExceptExtension + "_S840D_Mill_Adv" ;
			SinumerikLexer lexer = new SinumerikLexer(CharStreams.fromstring(Document.Text));

		//	File.WriteAllText("Program_S840_Text.txt", strb1.ToString()) ;
			StringBuilder gcodeOutput = new StringBuilder() ;
			return Task.Run(() => {
			    	
					SinumerikParser sinuParser = new SinumerikParser(new CommonTokenStream(lexer));
					sinuParser.BuildParseTree = true;
                	IParseTree tree = sinuParser.parse();
					
                	Scope scope = new Scope();
                	var functions = new Dictionary<string, Function>();
                	SymbolVisitor symbolVisitor = new SymbolVisitor(functions);
                	symbolVisitor.Visit(tree);
                	EvalVisitor visitor = new EvalVisitor(scope, functions, gcodeOutput);
					visitor.Visit(tree);
					
					cancellationToken.ThrowIfCancellationRequested();
             //   	gcodeOutput.Append(visitor.GcodeBuffer);
					MachineControl_CLS mcontrolCLS = new MachineControl_CLS();
					GCodeParser parser = new GCodeParser(null, gcodeOutput, "Sinumerik", mcontrolCLS);
					
					File.WriteAllText(_fullPath, mcontrolCLS.SCM_CW.ToString().Replace(';',',')) ;

					NXSessionManager.Instance.ExportClsfToNX(_fullPath, _fileNameAddPrefix, false) ;
					
					
				 	
			}, cancellationToken
			);
			
	}
	
	Task sin840_calculate_turn(System.Threading.CancellationToken cancellationToken)
	{
			StringBuilder strb1 = new StringBuilder(Document.Text) ;
			string _fullPath = FileDirectoryName + FileNameExceptExtension + "_S840D_Turn.cls" ;
			string _fileNameAddPrefix = FileNameExceptExtension + "_S840D_Turn" ;
			
			StringBuilder paramprog = new StringBuilder();
			return Task.Run(() => {			    	
				SinumerikLatheControl_CLS lmcontrolCLS = new SinumerikLatheControl_CLS();
				GCodeParser parser = new GCodeParser(null, strb1, "Sinumerik", lmcontrolCLS);
				
				File.WriteAllText(_fullPath, lmcontrolCLS.SCM_CW.ToString().Replace(';',',')) ;
				
				NXSessionManager.Instance.ExportClsfToNX(_fullPath, _fileNameAddPrefix, true) ;
			//	StringBuffer = mcontrolCLS.SCM_CW ;
				
			}, cancellationToken
			);
			                
	}
	
	Task sin840_calculate_turn_adv(System.Threading.CancellationToken cancellationToken)
	{
			StringBuilder gcodeOutput = new StringBuilder() ;
			string _fullPath = FileDirectoryName + FileNameExceptExtension + "_S840D_Turn.cls" ;
			string _fileNameAddPrefix = FileNameExceptExtension + "_S840D_Turn" ;
			SinumerikLexer lexer = new SinumerikLexer(CharStreams.fromstring(Document.Text));
			
			StringBuilder paramprog = new StringBuilder();
			return Task.Run(() => {

				SinumerikParser sinuParser = new SinumerikParser(new CommonTokenStream(lexer));
				sinuParser.BuildParseTree = true;
                IParseTree tree = sinuParser.parse();
				
                Scope scope = new Scope();
                var functions = new Dictionary<string, Function>();
                SymbolVisitor symbolVisitor = new SymbolVisitor(functions);
                symbolVisitor.Visit(tree);
                EvalVisitor visitor = new EvalVisitor(scope, functions, gcodeOutput);
				visitor.Visit(tree);

                cancellationToken.ThrowIfCancellationRequested();
                
				SinumerikLatheControl_CLS lmcontrolCLS = new SinumerikLatheControl_CLS();
				GCodeParser parser = new GCodeParser(null, gcodeOutput, "Sinumerik", lmcontrolCLS);
				
				File.WriteAllText(_fullPath, lmcontrolCLS.SCM_CW.ToString().Replace(';',',')) ;
				
				NXSessionManager.Instance.ExportClsfToNX(_fullPath, _fileNameAddPrefix, true) ;
			//	StringBuffer = mcontrolCLS.SCM_CW ;
				
			}, cancellationToken
			);
			                
	}
	Task mayak600_calculate_turn(System.Threading.CancellationToken cancellationToken)
	{
			StringBuilder strb1 = new StringBuilder(Document.Text) ;
			string _fullPath = FileDirectoryName + FileNameExceptExtension + "_M600_Turn.cls" ;
			string _fileNameAddPrefix = FileNameExceptExtension + "_M600_Turn" ;
			
			StringBuilder paramprog = new StringBuilder();
			return Task.Run(() => {			    	
				MayakLatheControl_CLS lmcontrolCLS = new MayakLatheControl_CLS();
				GCodeParser parser = new GCodeParser(null, strb1, "Mayak", lmcontrolCLS);
				
				File.WriteAllText(_fullPath, lmcontrolCLS.SCM_CW.ToString().Replace(';',',')) ;
				
				NXSessionManager.Instance.ExportClsfToNX(_fullPath, _fileNameAddPrefix, true) ;
			//	StringBuffer = mcontrolCLS.SCM_CW ;
				
			}, cancellationToken
			);
			                
	}
	
	Task okuma_calculate_mill(System.Threading.CancellationToken cancellationToken)
	{
			StringBuilder strb1 = new StringBuilder(Document.Text) ;
			string _fullPath = FileDirectoryName + FileNameExceptExtension + "_Okuma_Mill.cls" ;
			string _fileNameAddPrefix = FileNameExceptExtension + "_Okuma_Mill" ;
			
		//	File.WriteAllText("Program_S840_Text.txt", strb1.ToString()) ;
			StringBuilder paramprog = new StringBuilder() ;
			return Task.Run(() => {			    			
				MachineControl_CLS mcontrolCLS = new MachineControl_CLS();
				GCodeParser parser = new GCodeParser(null, strb1, "Okuma" ,mcontrolCLS);
				
				File.WriteAllText(_fullPath, mcontrolCLS.SCM_CW.ToString().Replace(';',',')) ;

				
				NXSessionManager.Instance.ExportClsfToNX(_fullPath, _fileNameAddPrefix, false) ;
				
			}, cancellationToken
			);
			
	}
		    
  }
}
