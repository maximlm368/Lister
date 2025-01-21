using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Lister.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Lister.Views
{
    public partial class MainView : ShowingDialog
    {
        private static bool _widthIsChanged;
        private static bool _heightIsChanged;
        public static bool SomeControlPressed;

        private MainViewModel _viewModel;

        internal static bool pathIsSet;
        internal static MainView Instance { get; private set; }
        internal static int TappedGoToEditorButton { get; private set; }

        internal static double ProperWidth { get; private set; }
        internal static double ProperHeight { get; private set; }

        internal BadgeEditorView EditorView { get; private set; }
        private List <BadgeViewModel> _processableBadges;
        private List <BadgeViewModel> _allPrintableBadges;
        private PageViewModel _firstPage;


        public MainView ()
        {
            InitializeComponent ();
            Instance = this;

            ProperWidth = Width;
            ProperHeight = Height;

            DataContext = App.services.GetRequiredService<MainViewModel> ();
            _viewModel = ( MainViewModel ) DataContext;
            _viewModel.PassView (this);

            FocusAdorner = null;

            Loaded += OnLoaded;
            LayoutUpdated += LayoutUpdatedHandler;
            PointerPressed += PointerIsPressed;

            this.AddHandler (UserControl.TappedEvent, PreventPasting, RoutingStrategies.Tunnel);
        }


        public override void HandleDialogClosing ()
        {
            _viewModel.HandleDialogClosing ();
        }


        internal void LayoutUpdatedHandler ( object sender, EventArgs args )
        {
            _viewModel.LayoutUpdated ();
        }


        private void PreventPasting ( object sender, TappedEventArgs args )
        {
            args.Handled = true;
        }


        private void PointerIsPressed ( object sender, PointerPressedEventArgs args )
        {
            var point = args.GetCurrentPoint (sender as Control);

            if ( point.Properties.IsRightButtonPressed )
            {
                if ( ! SomeControlPressed ) 
                {
                    Focusable = true;
                    this.Focus ();
                }

                SomeControlPressed = false;
            }
            else if ( point.Properties.IsLeftButtonPressed ) 
            {
                if ( ! SomeControlPressed )
                {
                    Focusable = true;
                    this.Focus ();
                }

                SomeControlPressed = false;
            }

            Focusable = false;
        }


        internal void OnLoaded ( object sender, RoutedEventArgs args )
        {
            _viewModel.OnLoaded ();
        }


        internal void ReleaseRunner () 
        {
            personChoosing.ReleaseScrollingLeverage ();
        }


        internal void ChangeSize ( double widthDifference, double heightDifference )
        {
            Width -= widthDifference;
            Height -= heightDifference;

            ProperWidth = Width;
            ProperHeight = Height;

            scene.Width -= widthDifference;
            scene.Height -= heightDifference;

            scene.workArea.Width -= widthDifference;
            scene.workArea.Height -= heightDifference;

            waiting.ChangeSize (heightDifference, widthDifference);

            personChoosing.AdjustComboboxWidth (widthDifference, true);
            personChoosing.CloseCustomCombobox ();
        }


        internal void SetProperSize ( double properWidth, double properHeight )
        {
            if ( properWidth != ProperWidth )
            {
                _widthIsChanged = true;
            }
            else
            {
                _widthIsChanged = false;
            }

            if ( properHeight != ProperHeight )
            {
                _heightIsChanged = true;
            }
            else
            {
                _heightIsChanged = false;
            }

            double widthDifference = Width - properWidth;
            double heightDifference = Height - properHeight;

            Width = properWidth;
            Height = properHeight;

            ProperWidth = Width;
            ProperHeight = Height;

            scene.workArea.Width -= widthDifference;
            scene.workArea.Height -= heightDifference;

            personChoosing.AdjustComboboxWidth (widthDifference, _widthIsChanged);
        }


        internal void RefreshTempateAppearences ( )
        {
            _viewModel.RefreshTemplateAppearences ();
        }


        internal void EditIncorrectBadges ( List <BadgeViewModel> processableBadges
                                          , List <BadgeViewModel> allPrintableBadges, PageViewModel firstPage )
        {
            _processableBadges = processableBadges;
            _allPrintableBadges = allPrintableBadges;
            _firstPage = firstPage;
            MainView mainView = this;
            MainWindow window = MainWindow.GetMainWindow ();

            if ( ( window != null )   &&   ( processableBadges.Count > 0 ) )
            {
                EditorView = new BadgeEditorView ( BadgesBuildingViewModel.BuildingOccured, processableBadges.Count );
                EditorView.SetProperSize (MainView.ProperWidth, MainView.ProperHeight);
                window.CancelSizeDifference ();
                TappedGoToEditorButton = 1;

                _viewModel.SetWaitingUpdatingLayout ();
            }
        }


        internal void SwitchToEditor ( )
        {
            MainWindow window = MainWindow.GetMainWindow ();

            Task task = new Task
            (
                () =>
                {
                    EditorView.PassIncorrectBadges (_processableBadges, _allPrintableBadges, _firstPage);
                    EditorView.PassBackPoint (this);

                    Dispatcher.UIThread.Invoke
                    (
                        () =>
                        {
                            _viewModel.EndWaiting ();
                            window.Content = EditorView;
                        }
                    );

                    TappedGoToEditorButton = 0;
                }
            );

            task.Start ();
        }
    }
}




