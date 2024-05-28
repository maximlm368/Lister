﻿using ContentAssembler;
using Lister.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static QuestPDF.Helpers.Colors;

namespace Lister.ViewModels
{
    public class PersonSourceViewModel : ViewModelBase
    {
        private IUniformDocumentAssembler _uniformAssembler;
        private PersonChoosingViewModel _personChoosingVM;

        private string sFP;
        internal string SourceFilePath
        {
            get { return sFP; }
            set
            {
                string path = SetPersonsFilePath ( value );
                this.RaiseAndSetIfChanged ( ref sFP , path , nameof ( SourceFilePath ) );
            }
        }

        
        public PersonSourceViewModel ( IUniformDocumentAssembler singleTypeDocumentAssembler
                                       , ContentAssembler.Size pageSize, PersonChoosingViewModel personChoosing ) 
        {
            _uniformAssembler = singleTypeDocumentAssembler;
            _personChoosingVM = personChoosing;
        }


        internal string SetPersonsFilePath ( string value )
        {
            bool valueIsSuitable = ( value != null ) && ( value != string.Empty );

            if ( valueIsSuitable )
            {
                _personChoosingVM.VisiblePeople.Clear ( );
                _personChoosingVM.People.Clear ( );

                try
                {
                    List<Person> persons = _uniformAssembler.GetPersons ( value );

                    foreach ( var person in persons )
                    {
                        _personChoosingVM.VisiblePeople.Add ( person );
                        _personChoosingVM.People.Add ( person );
                    }

                    return value;
                }
                catch ( IOException ex )
                {
                    int idOk = Winapi.MessageBox ( 0 , "Выбраный файл открыт в другом приложении. Закройте его." , "" , 0 );
                    return string.Empty;
                }
            }

            return string.Empty;
        }
    }
}
