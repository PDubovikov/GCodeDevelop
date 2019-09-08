using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Xceed.Wpf.AvalonDock.Layout;
using GCD.ViewModel;

namespace GCD.View.Pane
{

  class PanesTemplateSelector : DataTemplateSelector
    {
        public PanesTemplateSelector()
        {
        
        }

        public DataTemplate FileViewTemplate
        {
            get;
            set;
        }

        public DataTemplate FileStatsViewTemplate
        {
            get;
            set;
        }
        
        public DataTemplate NXToolsViewTemplate
        {
            get;
            set;
        }
        
        public DataTemplate ErrorsViewTemplate
        {
            get;
            set;
        }

        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            var itemAsLayoutContent = item as LayoutContent;

            if (item is FileViewModel)
                return FileViewTemplate;

            if (item is FileStatsViewModel)
                return FileStatsViewTemplate;
            
            if (item is NXToolsViewModel)
                return NXToolsViewTemplate;
            
            if (item is ErrorsViewModel)
                return ErrorsViewTemplate;

            return base.SelectTemplate(item, container);
        }
    }
}
