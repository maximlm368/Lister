﻿using ContentAssembler;
using Lister.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lister.ViewModels
{
    internal class PageNavigationZoomerViewModel : ViewModelBase
    {
        private short _scalabilityDepth = 0;
        private short _maxDepth = 5;
        private short _minDepth = -5;
        //private readonly short _scalabilityStep = 25;

        private SceneViewModel sc;
        internal SceneViewModel SceneVM
        {
            get { return sc; }
            set
            {
                this.RaiseAndSetIfChanged (ref sc, value, nameof (SceneVM));
            }
        }

        private int vpN;
        internal int VisiblePageNumber
        {
            get { return vpN; }
            set
            {
                this.RaiseAndSetIfChanged (ref vpN, value, nameof (VisiblePageNumber));
            }
        }

        private bool fE;
        internal bool FirstIsEnable
        {
            set
            {
                this.RaiseAndSetIfChanged (ref fE, value, nameof (FirstIsEnable));
            }
            get
            {
                return fE;
            }
        }

        private bool pE;
        internal bool PreviousIsEnable
        {
            set
            {
                this.RaiseAndSetIfChanged (ref pE, value, nameof (PreviousIsEnable));
            }
            get
            {
                return pE;
            }
        }

        private bool nE;
        internal bool NextIsEnable
        {
            set
            {
                this.RaiseAndSetIfChanged (ref nE, value, nameof (NextIsEnable));
            }
            get
            {
                return nE;
            }
        }

        private bool lE;
        internal bool LastIsEnable
        {
            set
            {
                this.RaiseAndSetIfChanged (ref lE, value, nameof (LastIsEnable));
            }
            get
            {
                return lE;
            }
        }

        private bool zonE;
        internal bool ZoomOnIsEnable
        {
            set
            {
                this.RaiseAndSetIfChanged (ref zonE, value, nameof (ZoomOnIsEnable));
            }
            get
            {
                return zonE;
            }
        }

        private bool zoutE;
        internal bool ZoomOutIsEnable
        {
            set
            {
                this.RaiseAndSetIfChanged (ref zoutE, value, nameof (ZoomOutIsEnable));
            }
            get
            {
                return zoutE;
            }
        }


        public PageNavigationZoomerViewModel ( IUniformDocumentAssembler docAssembler, SceneViewModel sceneViewModel )
        {
            SceneVM = sceneViewModel;
        }


        internal void VisualiseNextPage ()
        {
            VisiblePageNumber = SceneVM.VisualiseNextPage ();
            SetEnablePageNavigation ();
        }


        internal void VisualisePreviousPage ()
        {
            VisiblePageNumber = SceneVM.VisualisePreviousPage ();
            SetEnablePageNavigation ();
        }


        internal void VisualiseLastPage ()
        {
            VisiblePageNumber = SceneVM.VisualiseLastPage ();
            SetEnablePageNavigation ();
        }


        internal void VisualiseFirstPage ()
        {
            VisiblePageNumber = SceneVM.VisualiseFirstPage ();
            SetEnablePageNavigation ();
        }


        internal void VisualisePageWithNumber ( int pageNumber )
        {
            VisiblePageNumber = SceneVM.VisualisePageWithNumber (pageNumber);
            SetEnablePageNavigation ();
        }


        internal int GetPageCount ()
        {
            return SceneVM.GetPageCount ();
        }


        internal void ZoomOn ()
        {
            if ( _scalabilityDepth < _maxDepth )
            {

                SceneVM.ZoomOn ();
                _scalabilityDepth++;
            }

            if ( _scalabilityDepth == _maxDepth )
            {
                ZoomOnIsEnable = false;
            }

            if ( !ZoomOutIsEnable )
            {
                ZoomOutIsEnable = true;
            }
        }


        internal void ZoomOut ()
        {
            if ( _scalabilityDepth > _minDepth )
            {
                SceneVM.ZoomOut ();
                _scalabilityDepth--;
            }

            if ( _scalabilityDepth == _minDepth )
            {
                ZoomOutIsEnable = false;
            }

            if ( !ZoomOnIsEnable )
            {
                ZoomOnIsEnable = true;
            }
        }


        internal void SetEnablePageNavigation ()
        {
            int pageCount = GetPageCount ();

            if ( pageCount > 1 )
            {
                if ( ( VisiblePageNumber > 1 ) && ( VisiblePageNumber == pageCount ) )
                {
                    FirstIsEnable = true;
                    PreviousIsEnable = true;
                    NextIsEnable = false;
                    LastIsEnable = false;
                }
                else if ( ( VisiblePageNumber > 1 ) && ( VisiblePageNumber < pageCount ) )
                {
                    FirstIsEnable = true;
                    PreviousIsEnable = true;
                    NextIsEnable = true;
                    LastIsEnable = true;
                }
                else if ( ( VisiblePageNumber == 1 ) && ( pageCount == 1 ) )
                {
                    FirstIsEnable = false;
                    PreviousIsEnable = false;
                    NextIsEnable = false;
                    LastIsEnable = false;
                }
                else if ( ( VisiblePageNumber == 1 ) && ( pageCount > 1 ) )
                {
                    FirstIsEnable = false;
                    PreviousIsEnable = false;
                    NextIsEnable = true;
                    LastIsEnable = true;
                }
            }
        }


        internal void DisableButtons ()
        {
            ZoomOnIsEnable = false;
            ZoomOutIsEnable = false;
            FirstIsEnable = false;
            PreviousIsEnable = false;
            NextIsEnable = false;
            LastIsEnable = false;
        }


        internal void EnableZoom ()
        {
            ZoomOnIsEnable = true;
            ZoomOutIsEnable = true;
        }


        internal void RecoverPageCounterIfEmpty ()
        {
            SceneViewModel sceneVM = App.services.GetRequiredService<SceneViewModel>();
            sceneVM.RecoverPageNumber ();
        }
    }
}
