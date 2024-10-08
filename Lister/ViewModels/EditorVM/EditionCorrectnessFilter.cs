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

namespace Lister.ViewModels
{
    public partial class BadgeEditorViewModel : ViewModelBase
    {
        private static string _correctnessIcon = "GreenCheckMarker.jpg";
        private static string _incorrectnessIcon = "RedCross.png";
        private FilterChoosing _filterState = FilterChoosing.All;


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
            if ( _filterState == FilterChoosing.All )
            {
                _filterState = FilterChoosing.Corrects;

                TryChangeSpecificLists ();

                var imm = CorrectNumbered.ToImmutableSortedDictionary ();
                CorrectNumbered = imm.ToDictionary ();
                SetCurrentVisible (CorrectNumbered);
                IncorrectBadgesCount = 0;
            }
            else if ( _filterState == FilterChoosing.Corrects )
            {
                _filterState = FilterChoosing.Incorrects;

                TryChangeSpecificLists();

                var imm = IncorrectNumbered.ToImmutableSortedDictionary ();
                IncorrectNumbered = imm.ToDictionary ();
                SetCurrentVisible (IncorrectNumbered);
                IncorrectBadgesCount = _currentVisibleCollection.Count;
            }
            else if ( _filterState == FilterChoosing.Incorrects )
            {
                _filterState = FilterChoosing.All;

                TryChangeSpecificLists ();

                SetCurrentVisible (AllNumbered);
                IncorrectBadgesCount = IncorrectNumbered. Count;
            }

            CalcVisibleRange(_currentVisibleCollection.Count);
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
                SetCommonIcons ();
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
            _scrollStepNumber = 0;

            if ( BeingProcessedBadge != null )
            {
                ScrollWidth = _upDownButtonHeightWigth;
                _runnerHasWalked = 0;
                ScrollOffset = 0;
                _numberAmongVisibleIcons = 1;
                BeingProcessedNumber = 1;
                BeingProcessedBadge.Show ();
                ZeroSliderStation (VisibleIcons);
            }
            else
            {
                ScrollWidth = 0;
            }

            if ( VisibleIcons. Count < _maxVisibleCount ) 
            {
                ScrollWidth = 0;
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


        private void SetCommonIcons ()
        {
            int counter = 0;

            foreach ( KeyValuePair<int, BadgeViewModel> badge   in   AllNumbered )
            {
                if ( counter == _visibleRange )
                {
                    break;
                }

                if ( CorrectNumbered.ContainsKey (badge.Value.Id) )
                {
                    VisibleIcons.Add (new BadgeCorrectnessViewModel (true, badge.Value));
                }
                else if ( IncorrectNumbered.ContainsKey (badge.Value.Id) )
                {
                    VisibleIcons.Add (new BadgeCorrectnessViewModel (false, badge.Value));
                }

                counter++;
            }

            BeingProcessedBadge = AllNumbered [0];
            VisibleIcons [0].IconOpacity = 1;
            VisibleIcons [0].BoundFontWeight = Avalonia.Media.FontWeight.Bold;
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

                VisibleIcons.Add (new BadgeCorrectnessViewModel (true, badge.Value));

                if ( existingCounter == 0 )
                {
                    VisibleIcons [existingCounter].IconOpacity = 1;
                    VisibleIcons [existingCounter].BoundFontWeight = Avalonia.Media.FontWeight.Bold;
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

                VisibleIcons.Add (new BadgeCorrectnessViewModel (false, badge.Value));

                if ( existingCounter == 0 )
                {
                    VisibleIcons [existingCounter].IconOpacity = 1;
                    VisibleIcons [existingCounter].BoundFontWeight = Avalonia.Media.FontWeight.Bold;
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


        private void SetCurrentVisible ( Dictionary <int, BadgeViewModel> list ) 
        {
            _currentVisibleCollection = list;
            ProcessableCount = _currentVisibleCollection.Count;
        }
    }
}