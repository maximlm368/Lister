﻿using Avalonia;
using Avalonia.Threading;
using ContentAssembler;
using Lister.Views;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace Lister.ViewModels
{
    public partial class SceneViewModel : ViewModelBase
    {
        private static readonly int _badgeCountLimit = 3333;
        private static readonly double _scalabilityCoefficient = 1.25;

        private ConverterToPdf _converter;
        private IUniformDocumentAssembler _docAssembler;
        private double _documentScale;
        private double _zoomDegree = 100;
        private List <PageViewModel> _allPages;
        private List <PageViewModel> _printablePages;
        
        private PageViewModel _lastPage;
        private PageViewModel _lastPrintablePage;
        private SceneUserControl _view;
        private Person _chosenPerson;
        private string _procentSymbol = "%";
        private int _visiblePageNumStorage;

        private PageViewModel _visiblePage;
        internal PageViewModel VisiblePage
        {
            get { return _visiblePage; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _visiblePage, value, nameof (VisiblePage));
            }
        }

        private int _pageCount;
        internal int PageCount
        {
            get { return _pageCount; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _pageCount, value, nameof (PageCount));
            }
        }

        private int _visiblePageNumber;
        internal int VisiblePageNumber
        {
            get { return _visiblePageNumber; }
            private set
            {
                if ( value > 0 ) 
                {
                    _visiblePageNumStorage = value;
                }

                this.RaiseAndSetIfChanged (ref _visiblePageNumber, value, nameof (VisiblePageNumber));
            }
        }

        private int _badgeCount;
        internal int BadgeCount
        {
            get { return _badgeCount; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _badgeCount, value, nameof (BadgeCount));
            }
        }

        private int _incorrectBadgeCount;
        internal int IncorrectBadgeCount
        {
            get { return _incorrectBadgeCount; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _incorrectBadgeCount, value, nameof (IncorrectBadgeCount));
            }
        }

        internal List <BadgeViewModel> IncorrectBadges { get; private set; }
        internal List <BadgeViewModel> PrintableBadges { get; private set; }

        private string _zoomDegreeInView;
        internal string ZoomDegreeInView
        {
            get { return _zoomDegreeInView; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _zoomDegreeInView, value, nameof (ZoomDegreeInView));
            }
        }

        private Thickness _borderMargin;
        internal Thickness BorderMargin
        {
            get { return _borderMargin; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _borderMargin, value, nameof (BorderMargin));
            }
        }

        private double _canvasTop;
        internal double CanvasTop
        {
            get { return _canvasTop; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _canvasTop, value, nameof (CanvasTop));
            }
        }

        private bool _editionMustEnable;
        internal bool EditionMustEnable
        {
            get { return _editionMustEnable; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _editionMustEnable, value, nameof (EditionMustEnable));
            }
        }



        private bool _clearIsEnable;
        internal bool ClearIsEnable
        {
            set
            {
                this.RaiseAndSetIfChanged (ref _clearIsEnable, value, nameof (ClearIsEnable));
            }
            get
            {
                return _clearIsEnable;
            }
        }

        private bool _saveIsEnable;
        internal bool SaveIsEnable
        {
            set
            {
                this.RaiseAndSetIfChanged (ref _saveIsEnable, value, nameof (SaveIsEnable));
            }
            get
            {
                return _saveIsEnable;
            }
        }

        private bool _printIsEnable;
        internal bool PrintIsEnable
        {
            set
            {
                this.RaiseAndSetIfChanged (ref _printIsEnable, value, nameof (PrintIsEnable));
            }
            get
            {
                return _printIsEnable;
            }
        }

        private PersonChoosingViewModel _personChoosingVM;
        private TemplateChoosingViewModel _templateChoosingVM;


        public SceneViewModel ( IUniformDocumentAssembler docAssembler, PersonChoosingViewModel personChoosingVM ) 
        {
            SetButtonBlock ();
            _converter = new ConverterToPdf ();
            _docAssembler = docAssembler;
            _personChoosingVM = personChoosingVM;
            _documentScale = 1;
            _allPages = new List <PageViewModel> ();
            _printablePages = new List <PageViewModel> ();
            PageViewModel firstPage = new PageViewModel (_documentScale);
            VisiblePage = firstPage;
            _lastPage = VisiblePage;
            _allPages.Add (VisiblePage);

            _lastPrintablePage = new PageViewModel (_documentScale);
            _printablePages.Add (_lastPrintablePage);
            VisiblePageNumber = 1;
            ZoomDegreeInView = _zoomDegree.ToString () + " " + _procentSymbol;
            IncorrectBadges = new List <BadgeViewModel> ();
            PrintableBadges = new List <BadgeViewModel> ();
            EditionMustEnable = false;
            PageCount = 1; 
        }


        internal void RecoverPageNumber ()
        {
            VisiblePageNumber = 0;
        }


        internal void PassTemplateChoosing ( TemplateChoosingViewModel templateChoosingViewModel )
        {
            _templateChoosingVM = templateChoosingViewModel;
        }


        internal void PassView ( SceneUserControl view )
        {
            _view = view;
        }


        internal bool BuildBadges ( string templateName )
        {
            List <Badge> requiredBadges = _docAssembler.CreateBadgesByModel (templateName);

            if ( (BadgeCount + requiredBadges.Count) >= _badgeCountLimit ) 
            {
                return false;
            }

            if ( requiredBadges.Count > 0 )
            {
                List <BadgeViewModel> allBadges = new ();
                List <BadgeViewModel> allPrintableBadges = new ();

                for ( int index = 0;   index < requiredBadges.Count;   index++ )
                {
                    Person person = requiredBadges [index].Person;

                    if ( _personChoosingVM.IsEmpty (person) )
                    {
                        continue;
                    }

                    BadgeViewModel badge = new BadgeViewModel (requiredBadges [index], BadgeCount);
                    allBadges.Add (badge);

                    BadgeViewModel printableBadge = new BadgeViewModel (requiredBadges [index], BadgeCount);
                    allPrintableBadges.Add (printableBadge);

                    if ( ! badge.IsCorrect )
                    {
                        IncorrectBadges.Add (badge);
                        IncorrectBadgeCount++;
                    }

                    BadgeCount++;
                }

                List <PageViewModel> newPages = PageViewModel.PlaceIntoPages (allBadges, _documentScale, _lastPage);
                
                List <PageViewModel> printablePages = 
                                       PageViewModel.PlaceIntoPages (allPrintableBadges, _documentScale, _lastPrintablePage);
                
                bool placingStartedOnLastPage = ( _lastPage != null )   &&   _lastPage.Equals (newPages [0]);

                if ( placingStartedOnLastPage )
                {
                    Dispatcher.UIThread.Invoke (() => { VisiblePageNumber = _allPages.Count; });
                    newPages.RemoveAt (0);
                    printablePages.RemoveAt (0);
                }

                _allPages.AddRange (newPages);
                _printablePages.AddRange (printablePages);
                _lastPage = _allPages.Last ();
                _lastPrintablePage = _printablePages.Last ();

                PrintableBadges.AddRange (allPrintableBadges);

                Dispatcher.UIThread.Invoke 
                (() => 
                {
                    ModernMainViewModel modernMV = App.services.GetRequiredService<ModernMainViewModel> ();
                    modernMV.EndWaiting ();

                    VisiblePage = _allPages [VisiblePageNumber - 1];
                    PageCount = _allPages.Count;
                    VisiblePage.Show ();
                });
            }

            return true;
        }


        internal bool BuildSingleBadge ( string templateName )
        {
            if ( ( BadgeCount + 1 ) >= _badgeCountLimit )
            {
                return false;
            }

            Person goalPerson = _personChoosingVM.ChosenPerson;
            Badge requiredBadge = _docAssembler.CreateSingleBadgeByModel (templateName, goalPerson);
            BadgeViewModel goalVMBadge = new BadgeViewModel (requiredBadge, BadgeCount);
            BadgeViewModel printableBadge = new BadgeViewModel (requiredBadge, BadgeCount);

            if ( ! goalVMBadge.IsCorrect )
            {
                IncorrectBadges.Add (goalVMBadge);
                IncorrectBadgeCount++;
            }

            PrintableBadges.Add (printableBadge);
            bool placingStartedAfterEntireListAddition = ! _lastPage.Equals (VisiblePage);

            if ( placingStartedAfterEntireListAddition )
            {
                VisiblePage.Hide ();
                VisiblePage = _lastPage;
                VisiblePage.Show ();
            }

            PageViewModel possibleNewVisiblePage = _lastPage.AddBadge (goalVMBadge);
            PageViewModel possibleNewLastPrintablePage = _lastPrintablePage.AddBadge (printableBadge);

            bool timeToIncrementVisiblePageNumber = ! possibleNewVisiblePage.Equals (_lastPage);

            if ( timeToIncrementVisiblePageNumber )
            {
                VisiblePage.Hide ();
                VisiblePage = possibleNewVisiblePage;
                _lastPage = VisiblePage;
                _lastPrintablePage = possibleNewLastPrintablePage;
                _allPages.Add (possibleNewVisiblePage);
                _printablePages.Add (_lastPrintablePage);
                VisiblePage.Show ();
            }

            BadgeCount++;
            VisiblePageNumber = _allPages.Count;
            PageCount = _allPages.Count;
            goalVMBadge.Show ();

            return true;
        }


        internal void ClearAllPages ()
        {
            if ( _allPages.Count <= 0 )
            {
                return;
            }

            _allPages = new List <PageViewModel> ();
            VisiblePage = new PageViewModel (_documentScale);
            _lastPage = VisiblePage;
            _allPages.Add (_lastPage);

            _printablePages = new List<PageViewModel> ();
            PrintableBadges = new List <BadgeViewModel> ();
            _lastPrintablePage = new PageViewModel (_documentScale);
            _printablePages.Add (_lastPrintablePage);

            VisiblePageNumber = 1;
            PageCount = 1;
            BadgeCount = 0;
            IncorrectBadgeCount = 0;

            EditionMustEnable = false;
            IncorrectBadges = new List <BadgeViewModel> ();

            if ( _documentScale > 1 )
            {
                while ( _documentScale != 1 )
                {
                    ZoomOut ();
                }
            }
            else if ( _documentScale < 1 ) 
            {
                while ( _documentScale != 1 )
                {
                    ZoomOn ();
                }
            }
        }


        internal void ZoomOn ( )
        {
            _documentScale *= _scalabilityCoefficient;

            if ( VisiblePage != null )
            {
                for ( int pageCounter = 0;   pageCounter < _allPages.Count;   pageCounter++ )
                {
                    _allPages [pageCounter].ZoomOn (_scalabilityCoefficient);
                }

                _zoomDegree *= _scalabilityCoefficient;
                short zDegree = ( short ) _zoomDegree;
                ZoomDegreeInView = zDegree.ToString () + " " + _procentSymbol;

                CanvasTop *= _scalabilityCoefficient;
                double marginLeft = _scalabilityCoefficient * BorderMargin. Left;
                BorderMargin = new Thickness (marginLeft, 0, 0, 0);
            }
        }


        internal void ZoomOut ( )
        {
            _documentScale /= _scalabilityCoefficient;

            if ( VisiblePage != null )
            {
                for ( int pageCounter = 0;   pageCounter < _allPages.Count;   pageCounter++ )
                {
                    _allPages [pageCounter].ZoomOut (_scalabilityCoefficient);
                }

                _zoomDegree /= _scalabilityCoefficient;
                short zDegree = ( short ) _zoomDegree;
                ZoomDegreeInView = zDegree.ToString () + " " + _procentSymbol;

                CanvasTop /= _scalabilityCoefficient;
                double marginLeft = BorderMargin. Left / _scalabilityCoefficient;
                BorderMargin = new Thickness (marginLeft, 0, 0, 0);
            }
        }


        internal int VisualiseNextPage ()
        {
            if ( VisiblePageNumber < _allPages.Count )
            {
                VisiblePage.Hide ();
                VisiblePageNumber++;
                VisiblePage = _allPages [VisiblePageNumber - 1];
                VisiblePage.Show ();
            }

            return VisiblePageNumber;
        }


        internal int VisualisePreviousPage ()
        {
            if ( VisiblePageNumber > 1 )
            {
                VisiblePage.Hide ();
                VisiblePageNumber--;
                VisiblePage = _allPages [VisiblePageNumber - 1];
                VisiblePage.Show ();
            }

            return VisiblePageNumber;
        }


        internal int VisualiseLastPage ()
        {
            if ( VisiblePageNumber < _allPages.Count )
            {
                VisiblePage.Hide ();
                VisiblePageNumber = _allPages.Count;
                VisiblePage = _allPages [VisiblePageNumber - 1];
                VisiblePage.Show ();
            }

            return VisiblePageNumber;
        }


        internal int VisualiseFirstPage ()
        {
            if ( VisiblePageNumber > 1 )
            {
                VisiblePage.Hide ();
                VisiblePageNumber = 1;
                VisiblePage = _allPages [VisiblePageNumber - 1];
                VisiblePage.Show ();
            }

            return VisiblePageNumber;
        }


        internal int VisualisePageWithNumber ( int pageNumber )
        {
            bool visiblePageExists = ( VisiblePage != null );
            bool notTheSamePage = ( VisiblePageNumber != pageNumber );
            bool inRange = pageNumber <= _allPages.Count;

            if ( visiblePageExists   &&   notTheSamePage   &&   inRange )
            {
                VisiblePage.Hide ();
                VisiblePageNumber = pageNumber;
                VisiblePage = _allPages [VisiblePageNumber - 1];
                VisiblePage.Show ();
            }

            return VisiblePageNumber;
        }


        internal List <PageViewModel> GetPrintablePages ()
        {
            return _printablePages;
        }


        internal int GetPrintablePagesCount ()
        {
            return _printablePages.Count;
        }


        internal int GetPageCount ()
        {
            return _allPages.Count;
        }


        internal void ResetIncorrects ()
        {
            List <BadgeViewModel> corrects = new List <BadgeViewModel> ();

            foreach ( BadgeViewModel badge   in   IncorrectBadges )
            {
                if ( badge.IsCorrect )
                {
                    corrects.Add (badge);
                }
            }

            foreach ( BadgeViewModel correctBadge   in   corrects )
            {
                IncorrectBadges.Remove (correctBadge);
            }

            int counter = 0;

            foreach ( BadgeViewModel badge   in   IncorrectBadges )
            {
                badge.Id = counter;
                counter++;
            }

            IncorrectBadgeCount = IncorrectBadges. Count;
        }


        internal void EditIncorrectBadges ()
        {
            _view.EditIncorrectBadges (IncorrectBadges, PrintableBadges, _allPages [0]);
        }


        internal void ClearBadges ()
        {
            ClearAllPages ();
            DisableButtons ();

            PageNavigationZoomerViewModel zoomNavigationVM = App.services.GetRequiredService<PageNavigationZoomerViewModel> ();

            zoomNavigationVM.DisableButtons ();
        }


        private void DisableButtons ()
        {
            EditionMustEnable = false;
            ClearIsEnable = false;
            SaveIsEnable = false;
            PrintIsEnable = false;
        }


        internal void EnableButtons ()
        {
            EditionMustEnable = ( IncorrectBadgeCount > 0 );
            ClearIsEnable = true;
            SaveIsEnable = true;
            PrintIsEnable = true;
        }
    }
}
