using System;
using System.IO;

namespace ContentAssembler
{
    public class Person
    {
        public string Id { get; private set; }
        public string FamilyName { get; private set; }
        public string FirstName { get; private set; }
        public string PatronymicName { get; private set; }
        public string Department { get; private set; }
        public string Post { get; private set; }

        public string FullName
        {
            get
            { 
                string view = string.Empty;
                view = AppendNameInView (FamilyName, view);
                view = AppendNameInView (FirstName, view);
                view = AppendNameInView (PatronymicName, view);
                return view;
            }
        }

        public Person ( string id, string familyName, string firstName
                                           , string patronymicName, string department, string post )
        {
            Id = id;
            FamilyName = familyName;
            FirstName = firstName;
            PatronymicName = patronymicName;
            Department = department;
            Post = post;
        }


        public static Person ? Create ( string [] parts )
        {
            if ((parts == null)   ||   (parts.Length < 1)) 
            {
                return null;
            }

            bool condition = IsAllEmpty(parts);

            if ( condition )
            {
                return null;
            }

            if ( parts.Length < 6 ) 
            {
                string [] newParts = new string [6];

                for ( int index = 0;   index < newParts.Length;   index++ )
                {
                    if ( index >= parts.Length )
                    {
                        newParts [index] = string.Empty;
                    }
                    else 
                    {
                        newParts [index] = parts [index];
                    }
                }

                parts = newParts;
            }

            return new Person (parts [0], parts [1], parts [2], parts [3], parts [4], parts [5]);
        }


        private static bool IsAllEmpty ( string [] parts ) 
        {
            bool result = true;

            for ( int index = 0;   index < parts.Length;   index++ )
            {
                if ( ! string.IsNullOrWhiteSpace (parts [index]) )
                {
                    result = false;
                    break;
                }
            }

            return result;
        }


        public bool IsEmpty ( )
        {
            bool isEmpty = false;

            isEmpty = isEmpty || ( string.IsNullOrWhiteSpace (FamilyName) )
                                || ( string.IsNullOrWhiteSpace (FirstName) )
                                || ( string.IsNullOrWhiteSpace (PatronymicName) )
                                || ( string.IsNullOrWhiteSpace (Post) )
                                || ( string.IsNullOrWhiteSpace (Department) );

            return isEmpty;
        }


        public override string ToString()
        {
            string result = $"{Id} {FamilyName} {FirstName} {PatronymicName} {Department} {Post}";

            return result;
        }


        public Dictionary<string, string> GetProperties ()
        {
            Dictionary<string, string> result = new ( );

            result.Add ( "FamilyName", FamilyName );
            result.Add ( "FirstName", FirstName );
            result.Add ( "PatronymicName", PatronymicName );
            result.Add ( "Post", Post );
            result.Add ( "Department", Department );
            return result;
        }


        public bool IsMatchingTo ( string stringPresentation )
        {
            if ( string.IsNullOrWhiteSpace(stringPresentation) ) 
            {
                return false;
            }

            bool isMatching = (stringPresentation == this.FullName);
            return isMatching;
        }


        private string AppendNameInView ( string namePart, string view ) 
        {
            if ( ! string.IsNullOrWhiteSpace (namePart) )
            {
                if ( ! string.IsNullOrWhiteSpace (view) )
                {
                    view += " ";
                }

                view += namePart;
            }

            return view;
        }
    }
}