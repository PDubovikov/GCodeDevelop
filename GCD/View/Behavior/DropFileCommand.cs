using System;
using System.Windows;
using System.Windows.Input;
using System.IO ;
using GCD.ViewModel;

namespace GCD.View.Behavior
{
	/// <summary>
	/// Description of DropFileCommand.
	/// </summary>
	public static class DropFileCommand
	{
		
		private static readonly DependencyProperty DropCommandProperty = DependencyProperty.RegisterAttached(
		"DropCommand",
		typeof(ICommand),
		typeof(DropFileCommand),
		new PropertyMetadata(null, OnDropCommandChange));

		public static void SetDropCommand(DependencyObject source, ICommand value)
		{
			source.SetValue(DropCommandProperty, value);
		}
		
		public static ICommand GetDropCommand(DependencyObject source)
		{
			return (ICommand)source.GetValue(DropCommandProperty);
		}
		
		private static void OnDropCommandChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			UIElement uiElement = d as UIElement;	  // Remove the handler if it exist to avoid memory leaks
			uiElement.Drop -= UIElement_Drop;

            if (e.NewValue is ICommand)
            {
                ICommand command = e.NewValue as ICommand;

                // the property is attached so we attach the Drop event handler
                uiElement.Drop += UIElement_Drop;
            }
        }
		
		
		private static void UIElement_Drop(object sender, DragEventArgs e)
		{
			UIElement uiElement = sender as UIElement;

			// Sanity check just in case this was somehow send by something else
			if (uiElement == null)
				return;

			ICommand dropCommand = DropFileCommand.GetDropCommand(uiElement);

			// There may not be a command bound to this after all
			if (dropCommand == null)
				return;

			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] droppedFilePaths = e.Data.GetData(DataFormats.FileDrop, true) as string[];

				foreach (string droppedFilePath in droppedFilePaths)
				{
					// Check whether this attached behaviour is bound to a RoutedCommand
					if (dropCommand is RoutedCommand)
					{
						// Execute the routed command
						(dropCommand as RoutedCommand).Execute(droppedFilePath, uiElement);
					}
					else
					{
						// Execute the Command as bound delegate
						dropCommand.Execute(droppedFilePath);
					}
				}
			}
		}
		
			
	}
}
