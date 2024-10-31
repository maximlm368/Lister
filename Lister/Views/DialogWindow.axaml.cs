using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using ExCSS;
using Lister.ViewModels;
using MessageBox.Avalonia.Views;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace Lister.Views
{
    public partial class DialogWindow : Window
    {
        //internal DialogWindow ()
        //{
        //    InitializeComponent ();

        //    DataContext = new DialogViewModel ();

        //    this.CanResize = false;

        //    this.ExtendClientAreaToDecorationsHint = false;

        //    this.WhenActivated (action => action (ViewModel!.ChooseYes.Subscribe (Close)));
        //    this.WhenActivated (action => action (ViewModel!.ChooseNo.Subscribe (Close)));

        //    yes.CornerRadius = new Avalonia.CornerRadius ();
        //    no.CornerRadius = new Avalonia.CornerRadius ();
        //}

        //ReactiveWindow<DialogViewModel>

        public readonly string yes = "yes";
        public readonly string no = "no";

        private DialogViewModel _viewModel;
        private ShowingDialog _caller;

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
                    message.Text = value;
                    _message = value;
                }
            }
        }

        internal string Result { get; private set; }


        public DialogWindow ( ShowingDialog caller )
        {
            InitializeComponent ();

            _caller = caller;
            _viewModel = new DialogViewModel (this);
            DataContext = _viewModel;
            CanResize = false;
            No.Focus (NavigationMethod.Tab, KeyModifiers.None);
        }


        internal void ChooseYes ()
        {
            Result = yes;
            this.Close ();
            _caller.HandleDialogClosing ();
        }


        internal void ChooseNo ()
        {
            Result = no;
            this.Close ();
            _caller.HandleDialogClosing ();
        }



    }
}
