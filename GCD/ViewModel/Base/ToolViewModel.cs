﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCD.ViewModel.Base
{

  class ToolViewModel : PaneViewModel
    {
        public ToolViewModel(string name)
        {
            Name = name;
            Title = name;
        }

        public string Name {get; set;}


        #region IsVisible

        private bool _isVisible = true;
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    RaisePropertyChanged("IsVisible");
                }
            }
        }

        #endregion


    }
}
