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

                    List<string> pureLines = new List<string> ();

                    foreach ( string line   in   lines ) 
                    {
                        string pureLine = line.Trim ();
                        pureLines.Add ( pureLine );
                    }

                    _vm.MessageLines = pureLines;
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
            _vm.PassView (this);
        }


        internal void HandleTapped ( object sender, TappedEventArgs args )
        {
            this.Close ();
        }
    }
}
