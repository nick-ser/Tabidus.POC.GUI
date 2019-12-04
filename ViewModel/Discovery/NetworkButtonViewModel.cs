namespace Tabidus.POC.GUI.ViewModel.Discovery
{
    public class NetworkButtonViewModel : ViewModelBase
    {
        private string _subnetMark;
        private int _totalEndpoint;

        public string SubnetMark
        {
            get { return _subnetMark; }
            set
            {
                _subnetMark = value;
                OnPropertyChanged("SubnetMark");
            }
        }

        public int TotalEndpoint
        {
            get { return _totalEndpoint; }
            set
            {
                _totalEndpoint = value;
                OnPropertyChanged("TotalEndpoint");
            }
        }
       
    }
}