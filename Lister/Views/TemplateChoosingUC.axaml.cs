using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using ContentAssembler;
using ExtentionsAndAuxiliary;
using Lister.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Lister.Views
{
    public partial class TemplateChoosingUserControl : ReactiveUserControl <TemplateChoosingViewModel>
    {
        private static readonly string _jsonError = 
        "���������� ��������� ���� ������.���������� � ������������ �� �������� 324-708";

        private static TemplateViewModel _chosen;

        private ModernMainView _parent;
        private TemplateChoosingViewModel _vm;
        private string _theme;


        public TemplateChoosingUserControl ()
        {
            InitializeComponent ();
            DataContext = App.services.GetRequiredService<TemplateChoosingViewModel> ();
            _vm = ( TemplateChoosingViewModel ) DataContext;
            Loaded += OnLoaded;
            ActualThemeVariantChanged += ThemeChanged;
        }


        internal void OnLoaded ( object sender, RoutedEventArgs args )
        {
            _vm.ChosenTemplate = _chosen;

            if ( _chosen != null ) 
            {
                templateChoosing.PlaceholderText = _vm.ChosenTemplate. Name;
            }

            _vm.PassView (this);
            _theme = ActualThemeVariant.Key.ToString ();
            _vm.ChangeAccordingTheme (_theme);
        }


        internal void ThemeChanged ( object sender, EventArgs args )
        {
            if ( ActualThemeVariant == null ) 
            {
                return;
            }

            _theme = ActualThemeVariant.Key.ToString ();
            _vm.ChangeAccordingTheme ( _theme );
        }


        internal void CloseCustomCombobox ( object sender, GotFocusEventArgs args )
        {
            _parent = this.Parent.Parent as ModernMainView;
            PersonChoosingUserControl personChoosing = _parent. personChoosing;
            
            if ( personChoosing != null ) 
            {
                personChoosing.CloseCustomCombobox ();
            }
        }


        internal void HandleClosing ( object sender, EventArgs args )
        {
            TemplateViewModel chosen = templateChoosing.SelectedItem as TemplateViewModel;

            if ( chosen == null )
            {
                return;
            }

            bool templateIsIncorrect = (chosen.Color.Color.A == 100);

            if ( templateIsIncorrect )
            {
                _vm.ChosenTemplate = null;
                _chosen = null;
                var messegeDialog = new MessageDialog ();
                messegeDialog.Message = _jsonError;
                messegeDialog.ShowDialog (MainWindow._mainWindow);
                messegeDialog.Focusable = true;
                messegeDialog.Focus ();
            }
            else 
            {
                _vm.ChosenTemplate = chosen;
                _chosen = chosen;
            }
        }


        internal void SetCursorWait ( )
        {
            //Cursor = new Cursor (StandardCursorType.Wait);
            MainWindow._mainWindow.Cursor = new Cursor (StandardCursorType.Wait);
        }


        internal void SetCursorArrow ()
        {
            //Cursor = new Cursor (StandardCursorType.Arrow);
            MainWindow._mainWindow.Cursor = new Cursor (StandardCursorType.Arrow);
        }
    }
}
