﻿using Avalonia;
using Avalonia.Media;
using Avalonia.VisualTree;
using ContentAssembler;
using Lister.Views;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lister.ViewModels
{
    public class ModernMainViewModel : ViewModelBase
    {
        public static bool MainViewIsWaiting { get; set; }

        private PersonSourceViewModel _personSourceVM;
        private PersonChoosingViewModel _personChoosingVM;
        private TemplateChoosingViewModel _templateChoosingVM;
        private ZoomNavigationViewModel _zoomNavigationVM;
        private SceneViewModel _sceneVM;
        private ModernMainView _view;

        //private bool wV;
        //public bool WaitingIsVisible
        //{
        //    get { return wV; }
        //    private set
        //    {
        //        this.RaiseAndSetIfChanged (ref wV, value, nameof (WaitingIsVisible));
        //    }
        //}


        public ModernMainViewModel ( PersonSourceViewModel personSourceVM, PersonChoosingViewModel personChoosingVM,
                                     TemplateChoosingViewModel templateChoosingVM, ZoomNavigationViewModel zoomNavigationVM,
                                     SceneViewModel sceneVM )
        {
            _personChoosingVM = personChoosingVM;
            _personSourceVM = personSourceVM;
            _templateChoosingVM = templateChoosingVM;
            _zoomNavigationVM = zoomNavigationVM;
            _sceneVM = sceneVM;
        }


        internal void HandleDialogClosing ()
        {
            WaitingViewModel waitingVM = App.services.GetRequiredService<WaitingViewModel> ();
            waitingVM.HandleDialogClosing ();
        }


        internal void ResetIncorrects ( )
        {
            _sceneVM.ResetIncorrects ();
        }


        internal void PassView ( ModernMainView view )
        {
            _view = view;

            WaitingView wv = _view. waiting;
        }


        internal void SetWaiting ( )
        {
            WaitingViewModel waitingVM = App.services.GetRequiredService <WaitingViewModel> ();
            waitingVM.Show ();
            MainViewIsWaiting = true;
        }


        internal void EndWaiting ()
        {
            _templateChoosingVM.BuildingIsPossible = true;
            WaitingViewModel waitingVM = App.services.GetRequiredService<WaitingViewModel> ();
            waitingVM.Hide ();
            MainViewIsWaiting = false;
        }


        internal void EndWaitingPdfOrPrint ()
        {
            WaitingViewModel waitingVM = App.services.GetRequiredService<WaitingViewModel> ();
            waitingVM.Hide ();
            MainViewIsWaiting = false;
        }


        internal void LayoutUpdated ( )
        {
            if ( TemplateChoosingViewModel.TappedBadgesBuildingButton == 1 )
            {
                _templateChoosingVM.BuildDuringWaiting ();
                return;
            }
            else if ( SceneViewModel.TappedPdfGenerationButton == 1 )
            {
                _sceneVM.GeneratePdfDuringWaiting ();
                return;
            }
            else if ( SceneViewModel.TappedPrintButton == 1 )
            {
                _sceneVM.PrintDuringWaiting ();
                return;
            }
            else if ( ModernMainView.TappedEditorBuildingButton == 1 )
            {
                _view.BuildEditor ();
            }
            //else if ( PrintDialogViewModel.IsClosed )
            //{
            //    _sceneVM.StartWaiting ();
            //}
        }
    }


}
//  IUniformDocumentAssembler singleTypeDocumentAssembler, ContentAssembler.Size pageSize

//public override void Send ( string message, List<object> args, LittleViewModel littleVM )
//{
//    if ( _personChoosingVM == (littleVM as PersonChoosingViewModel) ) 
//    {

//    }
//}

//public abstract class Mediator : ViewModelBase
//{
//    public abstract void Send ( string message, List<object> ? args, LittleViewModel littleVM );
//}



//public abstract class LittleViewModel : ViewModelBase
//{
//    protected Mediator _mediator;


//    public void SetMediator ( Mediator mediator )
//    {
//        this._mediator = mediator;
//    }


//    public virtual void Send ( string message, List<object> ? args )
//    {
//        _mediator.Send (message, args, this);
//    }


//    public abstract void Notify ( string message, List<object> ? args );
//}