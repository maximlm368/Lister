﻿using ContentAssembler;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.ObjectModel;
using Avalonia.Media;
using System.Globalization;
using System.Reflection.Metadata;
using ExtentionsAndAuxiliary;
using Microsoft.VisualBasic;
using Avalonia.Media.Imaging;
using Lister.Extentions;
using System.Linq.Expressions;
using Lister.Views;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using System.Reactive.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Avalonia.Threading;
using System.Reflection;
using ExCSS;
using System;
using AvaloniaEdit.Utils;
using System.Collections.Immutable;
using ReactiveUI;
using Avalonia;

namespace Lister.ViewModels
{
    public partial class BadgeEditorViewModel : ViewModelBase
    {
        private readonly double _switcherWidth = 32;
        private readonly double _filterLabelWidth = 70;
        private readonly string _allLabel;
        private readonly string _incorrectLabel;
        private readonly string _correctLabel;
        private readonly string _allTip;
        private readonly string _correctTip;
        private readonly string _incorrectTip;

        private readonly double _narrowCorrectnessWidthLimit = 155;
        private readonly int _narrowMinCorrectnessTextLength = 14;
        private readonly int _narrowMaxCorrectnessTextLength = 20;

        private readonly double _wideCorrectnessWidthLimit = 165;
        private readonly int _wideMinCorrectnessTextLength = 17;
        private readonly int _wideMaxCorrectnessTextLength = 23;

        private string _correctnessIcon;
        private string _incorrectnessIcon;
        private FilterChoosing _filterState = FilterChoosing.All;
        private double _correctnessWidthLimit;
        private int _minCorrectnessTextLength;
        private int _maxCorrectnessTextLength;

        private ScrollWideness _scrollWideness = ScrollWideness.Wide;

        private bool _isDropOpen;
        internal bool IsDropDownOpen
        {
            get { return _isDropOpen; }
            set
            {
                this.RaiseAndSetIfChanged (ref _isDropOpen, value, nameof (IsDropDownOpen));
            }
        }

        private bool _isComboboxEnabled;
        internal bool IsComboboxEnabled
        {
            get { return _isComboboxEnabled; }
            set
            {
                this.RaiseAndSetIfChanged (ref _isComboboxEnabled, value, nameof (IsComboboxEnabled));
            }
        }

        private double _switcherWidt;
        internal double SwitcherWidth
        {
            get { return _switcherWidt; }
            set
            {
                this.RaiseAndSetIfChanged (ref _switcherWidt, value, nameof (SwitcherWidth));
            }
        }

        private double _filterLableWidth;
        internal double FilterLabelWidth
        {
            get { return _filterLableWidth; }
            set
            {
                this.RaiseAndSetIfChanged (ref _filterLableWidth, value, nameof (FilterLabelWidth));
            }
        }

        private int _comboboxSelectedIndex;
        internal int ComboboxSelectedIndex
        {
            get { return _comboboxSelectedIndex; }
            set
            {
                this.RaiseAndSetIfChanged (ref _comboboxSelectedIndex, value, nameof (ComboboxSelectedIndex));
            }
        }

        private string _switcherTip;
        internal string SwitcherTip
        {
            get { return _switcherTip; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _switcherTip, value, nameof (SwitcherTip));
            }
        }


        internal bool IsProcessableChangedInSpecificFilter ( int filterableNumber )
        {
            bool filterOccured = false;

            if ( _filterState == FilterChoosing.All )
            {
                return false;
            }
            else if ( _filterState == FilterChoosing.Corrects )
            {
                if ( ! BeingProcessedBadge.IsCorrect )
                {
                    filterOccured = true;
                }
            }
            else if ( _filterState == FilterChoosing.Incorrects )
            {
                if ( BeingProcessedBadge.IsCorrect )
                {
                    filterOccured = true;
                }
            }

            return filterOccured;
        }


        internal void Filter ()
        {
            _runnerHasWalked = 0;

            if ( _filterState == FilterChoosing.All )
            {
                _filterState = FilterChoosing.Corrects;
                
                SwitcherBackground = new SolidColorBrush (new Avalonia.Media.Color (155, 0, 100, 0));
                SwitcherTip = _correctTip;
                ComboboxSelectedIndex = 1;

                TryChangeSpecificLists ();

                ScrollWidth = 0;

                var imm = CorrectNumbered.ToImmutableSortedDictionary ();
                CorrectNumbered = imm.ToDictionary ();
                CurrentVisibleCollection = CorrectNumbered;
                IncorrectBadgesCount = 0;
            }
            else if ( _filterState == FilterChoosing.Corrects )
            {
                _filterState = FilterChoosing.Incorrects;

                SwitcherBackground = new SolidColorBrush (new Avalonia.Media.Color (155, 100, 0, 0));
                SwitcherTip = _incorrectTip;
                ComboboxSelectedIndex = 2;

                TryChangeSpecificLists ();

                var imm = IncorrectNumbered.ToImmutableSortedDictionary ();
                IncorrectNumbered = imm.ToDictionary ();
                CurrentVisibleCollection = IncorrectNumbered;
                IncorrectBadgesCount = CurrentVisibleCollection.Count;
            }
            else if ( _filterState == FilterChoosing.Incorrects )
            {
                _filterState = FilterChoosing.All;
                CurrentVisibleCollection = AllNumbered;

                SwitcherBackground = new SolidColorBrush (new Avalonia.Media.Color (155, 0, 0, 100));
                SwitcherTip = _allTip;
                ComboboxSelectedIndex = 0;

                TryChangeSpecificLists ();

                IncorrectBadgesCount = IncorrectNumbered. Count;
            }

            ProcessableCount = CurrentVisibleCollection.Count;
            SetSliderWideness ();
            CalcVisibleRange (CurrentVisibleCollection.Count);
            SetScroller (CurrentVisibleCollection.Count);
            SetAccordingIcons ();
            EnableNavigation ();
        }


        private void SetAccordingIcons ()
        {
            BeingProcessedBadge = null;
            VisibleIcons = new ();
            NextIsEnable = true;
            LastIsEnable = true;

            if ( _filterState == FilterChoosing.All )
            {
                SetMixedIcons ();
            }
            else if ( _filterState == FilterChoosing.Corrects )
            {
                SetIconsForCorrectFilter ();
            }
            else if ( _filterState == FilterChoosing.Incorrects )
            {
                SetIconsForIncorrectFilter ();
            }

            _numberAmongVisibleIcons = 1;
            _scrollStepIndex = 0;

            if ( BeingProcessedBadge != null )
            {
                SetToCorrectScale (BeingProcessedBadge);
                BeingProcessedNumber = 1;
                BeingProcessedBadge.Show ();
                ZeroSliderStation (VisibleIcons);
            }
            else 
            {
                BeingProcessedNumber = 0;
            }

            if ( VisibleIcons. Count == 0 )
            {
                UpDownWidth = 0;
                UpDownIsFocusable = false;
                FirstIsEnable = false;
                PreviousIsEnable = false;
                NextIsEnable = false;
                LastIsEnable = false;
            }
            else 
            {
                UpDownWidth = _upDownWidth;
                UpDownIsFocusable = true;
            }
        }


        private void SetMixedIcons ()
        {
            int counter = 0;

            foreach ( KeyValuePair <int, BadgeViewModel> badge   in   AllNumbered )
            {
                if ( counter == _visibleRange )
                {
                    break;
                }

                bool badgeIsCorrect = false;

                if ( CorrectNumbered.ContainsKey (badge.Value.Id) )
                {
                    badgeIsCorrect = true;
                }
                else if ( IncorrectNumbered.ContainsKey (badge.Value.Id) )
                {
                    badgeIsCorrect = false;
                }

                VisibleIcons.Add (new BadgeCorrectnessViewModel (badgeIsCorrect, badge.Value, _correctnessWidthLimit
                                                  , new int [2] { _minCorrectnessTextLength, _maxCorrectnessTextLength }));

                counter++;
            }

            BeingProcessedBadge = AllNumbered.ElementAt (0).Value;
            VisibleIcons [0].IconOpacity = 1;
            VisibleIcons [0].BoundFontWeight = Avalonia.Media.FontWeight.Bold;
            VisibleIcons [0].CalcStringPresentation (_correctnessWidthLimit
                                                    , new int [2] { _minCorrectnessTextLength, _maxCorrectnessTextLength });
            ActiveIcon = VisibleIcons [0];
            FilterState = null;
        }


        private void SetIconsForCorrectFilter ()
        {
            int existingCounter = 0;
            int firstExistingCommonNumber = -1;

            foreach ( KeyValuePair <int, BadgeViewModel> badge   in   CorrectNumbered )
            {
                if ( existingCounter == _visibleRange )
                {
                    break;
                }

                VisibleIcons.Add (new BadgeCorrectnessViewModel (true, badge.Value, _correctnessWidthLimit
                                                  , new int [2] { _minCorrectnessTextLength, _maxCorrectnessTextLength }));

                if ( existingCounter == 0 )
                {
                    VisibleIcons [existingCounter].IconOpacity = 1;
                    VisibleIcons [existingCounter].BoundFontWeight = Avalonia.Media.FontWeight.Bold;
                    VisibleIcons [existingCounter].CalcStringPresentation (_correctnessWidthLimit
                                                    , new int [2] { _minCorrectnessTextLength, _maxCorrectnessTextLength });
                    firstExistingCommonNumber = badge.Key;
                }

                existingCounter++;
            }

            if ( firstExistingCommonNumber > -1 )
            {
                string correctnessIcon = App.ResourceDirectoryUri + _correctnessIcon;
                Uri correctUri = new Uri (correctnessIcon);
                FilterState = ImageHelper.LoadFromResource (correctUri);
                ActiveIcon = VisibleIcons [0];
                BeingProcessedBadge = CorrectNumbered.ElementAt (0).Value;
            }
        }


        private void SetIconsForIncorrectFilter ()
        {
            int existingCounter = 0;
            int firstExistingCommonNumber = -1;

            foreach ( KeyValuePair<int, BadgeViewModel> badge   in   IncorrectNumbered )
            {
                if ( existingCounter == _visibleRange )
                {
                    break;
                }

                VisibleIcons.Add (new BadgeCorrectnessViewModel (false, badge.Value, _correctnessWidthLimit
                                                  , new int [2] { _minCorrectnessTextLength, _maxCorrectnessTextLength }));

                if ( existingCounter == 0 )
                {
                    VisibleIcons [existingCounter].IconOpacity = 1;
                    VisibleIcons [existingCounter].BoundFontWeight = Avalonia.Media.FontWeight.Bold;
                    VisibleIcons [existingCounter].CalcStringPresentation (_correctnessWidthLimit
                                                    , new int [2] { _minCorrectnessTextLength, _maxCorrectnessTextLength });
                    firstExistingCommonNumber = badge.Key;
                }

                existingCounter++;
            }

            if ( firstExistingCommonNumber > -1 )
            {
                string correctnessIcon = App.ResourceDirectoryUri + _incorrectnessIcon;
                Uri correctUri = new Uri (correctnessIcon);
                FilterState = ImageHelper.LoadFromResource (correctUri);
                ActiveIcon = VisibleIcons [0];
                BeingProcessedBadge = IncorrectNumbered.ElementAt (0).Value;
            }
        }


        private void TryChangeSpecificLists () 
        {
            if ( BeingProcessedBadge == null ) 
            {
                return;
            }

            if ( BeingProcessedBadge. IsCorrect )
            {
                try
                {
                    CorrectNumbered.Add (BeingProcessedBadge.Id, BeingProcessedBadge);
                    IncorrectNumbered.Remove (BeingProcessedBadge.Id);
                }
                catch ( Exception ex ) { }
            }
            else if ( ! BeingProcessedBadge. IsCorrect )
            {
                try
                {
                    IncorrectNumbered.Add (BeingProcessedBadge.Id, BeingProcessedBadge);
                    CorrectNumbered.Remove (BeingProcessedBadge.Id);
                }
                catch ( Exception ex ) { }
            }
        }


        private void SetSliderWideness ( )
        {
            if ( CurrentVisibleCollection.Count > _visibleRange )
            {
                _correctnessWidthLimit = _narrowCorrectnessWidthLimit;
                _minCorrectnessTextLength = _narrowMinCorrectnessTextLength;
                _maxCorrectnessTextLength = _narrowMaxCorrectnessTextLength;
            }
            else
            {
                _correctnessWidthLimit = _wideCorrectnessWidthLimit;
                _minCorrectnessTextLength = _wideMinCorrectnessTextLength;
                _maxCorrectnessTextLength = _wideMaxCorrectnessTextLength;
            }
        }


        internal void Filter ( string filterName )
        {
            ReleaseCaptured ();
            _runnerHasWalked = 0;

            bool appLoadingIs = ( AllNumbered == null ) 
                                || ( CorrectNumbered == null ) 
                                || ( IncorrectNumbered == null );

            if (appLoadingIs) 
            {
                return;
            }

            if ( filterName == _allLabel )
            {
                _filterState = FilterChoosing.All;
                CurrentVisibleCollection = AllNumbered;

                SwitcherBackground = new SolidColorBrush (new Avalonia.Media.Color (255, 0, 0, 200));
                SwitcherTip = _allTip;
                TryChangeSpecificLists ();

                IncorrectBadgesCount = IncorrectNumbered.Count;
            }
            else if ( filterName == _correctLabel )
            {
                _filterState = FilterChoosing.Corrects;

                SwitcherBackground = new SolidColorBrush (new Avalonia.Media.Color (255, 0, 200, 0));
                SwitcherTip = _correctTip;
                TryChangeSpecificLists ();

                var imm = CorrectNumbered.ToImmutableSortedDictionary ();
                CorrectNumbered = imm.ToDictionary ();
                CurrentVisibleCollection = CorrectNumbered;
                IncorrectBadgesCount = 0;
            }
            else if ( filterName == _incorrectLabel )
            {
                _filterState = FilterChoosing.Incorrects;

                SwitcherBackground = new SolidColorBrush (new Avalonia.Media.Color (255, 200, 0, 0));
                SwitcherTip = _incorrectTip;
                TryChangeSpecificLists ();

                var imm = IncorrectNumbered.ToImmutableSortedDictionary ();
                IncorrectNumbered = imm.ToDictionary ();
                CurrentVisibleCollection = IncorrectNumbered;
                IncorrectBadgesCount = CurrentVisibleCollection.Count;
            }

            ProcessableCount = CurrentVisibleCollection.Count;
            SetSliderWideness ();
            CalcVisibleRange (CurrentVisibleCollection.Count);
            SetScroller (CurrentVisibleCollection.Count);
            SetAccordingIcons ();
            EnableNavigation ();
        }


        internal void ExtendOrShrinkCollectionManagement ()
        {
            if ( _filterIsOpen )
            {
                CollectionFilterMargin = new Thickness (_collectionFilterMarginLeft, 0);
                WorkAreaWidth += _namesFilterWidth;
                _filterIsOpen = false;
                ExtenderContent = "\uF060";
                SwitcherWidth = _switcherWidth;
                FilterLabelWidth = 0;
                IsComboboxEnabled = false;
                ExtentionTip = _extentionToolTip;
            }
            else
            {
                CollectionFilterMargin = new Thickness (0, 0);
                WorkAreaWidth -= _namesFilterWidth;
                _filterIsOpen = true;
                ExtenderContent = "\uF061";
                SwitcherWidth = 0;
                FilterLabelWidth = _filterLabelWidth;
                IsComboboxEnabled = true;
                ExtentionTip = _shrinkingToolTip;
            }
        }



        private enum ScrollWideness 
        {
            Wide = 0,
            Narrow = 1
        }
    }
}