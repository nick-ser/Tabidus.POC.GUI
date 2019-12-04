using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tabidus.POC.GUI.Common;

namespace Tabidus.POC.GUI.ViewModel
{
    public class PageViewModelBase : ViewModelBase
    {
        public PageViewModelBase()
        {
            
        }
        public virtual void Refresh()
        {
            
        }

        public RightTreeViewModel RightTreeViewModel
        {
            get
            {
                return PageNavigatorHelper.GetRightElementViewModel();
            }
        }
    }
}
