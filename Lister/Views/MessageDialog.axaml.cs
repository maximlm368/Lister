using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using ExtentionsAndAuxiliary;
using Lister.ViewModels;
using MessageBox.Avalonia.Views;

namespace Lister.Views
{
    public partial class MessageDialog : BaseWindow
    {
        private MessageViewModel _vm;

        private string _message;
        internal string Message 
        {
            get 
            {
                return _message; 
            }

            set 
            {
                if ( value != null ) 
                {
                    List<string> lines = value.SplitBySeparators (new List<char> () { '.' });

                    if ( lines.Count == 1 ) 
                    {
                        lines = new List<string> () { value };
                    }

                    foreach ( string line   in   lines ) 
                    {
                        line.Trim ();
                    }

                    _vm.MessageLines = lines;
                    _message = value;
                }
            }
        }


        public MessageDialog ()
        {
            InitializeComponent ();

            _vm = new MessageViewModel ();
            DataContext = _vm;
            CanResize = false;
            //ok.Focus (NavigationMethod.Tab, KeyModifiers.None);

            //BorderBrush = new SolidColorBrush (MainWindow.black);
            //BorderThickness = new Avalonia.Thickness (1,1,1,1);
        }


        internal void HandleTapped ( object sender, TappedEventArgs args )
        {
            this.Close ();
        }
    }
}
