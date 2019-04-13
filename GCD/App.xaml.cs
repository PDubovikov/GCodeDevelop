using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using GCD.ViewModel;
using GCD.View;


namespace GCD
{

    public partial class App : Application
    {
    	  
        protected override void OnStartup(StartupEventArgs startupArgs)
        {
            base.OnStartup(startupArgs);
            
            if (startupArgs != null)
               ProcessCmdLine(startupArgs.Args);
            
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException) ;
            
            new MainWindow() { DataContext = Workspace.This }.Show();
        }

		private static void ProcessCmdLine(IEnumerable<string> args)
        {
            if (args != null)
            {
                foreach (string sPath in args)
                {
                    Workspace.This.Open(sPath);
                }
            }
        }

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{	
 	 	 	MessageBox.Show((e.ExceptionObject as Exception).Message, "Unhandled UI Exception");
 	 	 	
   			// here you can log the exception ...
		}		
    	
    }
}
