﻿using ContentAssembler;
using ExtentionsAndAuxiliary;
using Microsoft.Extensions.Configuration;
//using QuestPDF.Infrastructure;

namespace DataGateway
{
    public class BadgeAppearenceProvider : IBadgeAppearenceProvider
    {
        private string templatesFolderPath;
        private List<string> textualAtomNames;
        private Dictionary<string, string> nameAndJson;


        public BadgeAppearenceProvider ( string templatesPath )
        {
            textualAtomNames = new List<string> ( ) {"FamilyName", "FirstName", "PatronymicName", "Post", "Department"};
            this.templatesFolderPath = templatesPath;
            DirectoryInfo containingDirectory = new DirectoryInfo ( templatesFolderPath );
            FileInfo [ ] fileInfos = containingDirectory.GetFiles ( "*.json" );
            nameAndJson = new Dictionary<string , string> ( );

            foreach ( FileInfo fileInfo in fileInfos )
            {
                string jsonPath = fileInfo.FullName;
                string templateName = GetterFromJson.GetSectionValue ( new List<string> { "TemplateName" }, jsonPath );
                bool hasNameExist = ! string.IsNullOrEmpty ( templateName );

                if ( hasNameExist )
                {
                    nameAndJson.Add ( templateName, jsonPath );
                }
            }
        }


        public OrganizationalDataOfBadge GetBadgeData (string badgeTemplateName)
        {
            double badgeWidth = 350;
            double badgeHeight = 214;
            Size badgeSize = new Size (badgeWidth, badgeHeight);

            double personTextAreaWidth = 220;
            double personTextAreaHeight = 147;
            Size personTextAreaSize = new Size (personTextAreaWidth, personTextAreaHeight);

            double personTextBlockTopShiftOnBackground = 60;
            double personTextBlockLeftShiftOnBackground = 130;

            double firstLevelFontSize = 30;
            double secondLevelFontSize = 16;
            double thirdLevelFontSize = 11;

            double firstLevelTBHeight = 37;
            double secondLevelTBHeight = 20;
            double thirdLevelTBHeight = 14;

            BadgeDimensions badgeDimensions = new BadgeDimensions( badgeSize, personTextAreaSize
                                                  , personTextBlockTopShiftOnBackground, personTextBlockLeftShiftOnBackground 
                                                  , firstLevelFontSize, secondLevelFontSize, thirdLevelFontSize
                                                  , firstLevelTBHeight, secondLevelTBHeight, thirdLevelTBHeight);

            OrganizationalDataOfBadge badgeDescriprion = new OrganizationalDataOfBadge (badgeDimensions, null);

            return badgeDescriprion;
        }


        public BadgeLayout GetBadgeLayout ( string badgeTemplateName )
        {
            string jsonPath = nameAndJson [ badgeTemplateName ];

            double badgeWidth = GetterFromJson.GetSectionValue ( new List<string> { "Width" }, jsonPath ).TranslateIntoDouble ( );
            double badgeHeight =
            GetterFromJson.GetSectionValue ( new List<string> { "Height" }, jsonPath ).TranslateIntoDouble ( );
            Size badgeSize = new Size ( badgeWidth, badgeHeight );

            List<TextualAtom> atoms = GetAtoms ( jsonPath );
            SetUnitingAtoms (atoms, jsonPath);

            List<InsideImage> pictures = GetImages ( jsonPath );
            BadgeLayout result = new BadgeLayout (badgeSize, atoms, pictures);

            return result;
        }


        private List<TextualAtom> GetAtoms ( string jsonPath ) 
        {
            List<TextualAtom> atoms = new ();

            atoms.Add (BuildTextualAtom ("FamilyName", jsonPath));
            atoms.Add (BuildTextualAtom ("FirstName", jsonPath));
            atoms.Add (BuildTextualAtom ("PatronymicName", jsonPath));
            atoms.Add (BuildTextualAtom ("Post", jsonPath));
            atoms.Add (BuildTextualAtom ("Department", jsonPath));

            return atoms;
        }


        private void SetUnitingAtoms ( List<TextualAtom> atoms, string jsonPath ) 
        {
            IEnumerable<IConfigurationSection> unitings =
                              GetterFromJson.GetIncludedItemsOfSection (new List<string> { "UnitedTextBlocks : Items" }, jsonPath);

            foreach ( IConfigurationSection unit in unitings )
            {
                IConfigurationSection unitedSection = unit.GetSection ("United");
                IEnumerable<IConfigurationSection> unitedSections = GetterFromJson.GetChildrenOfSection (unitedSection);
                List<string> unitedAtomsNames = new List<string> ();

                foreach ( IConfigurationSection name in unitedSections )
                {
                    unitedAtomsNames.Add (name.Value);
                }

                TextualAtom unitingAtom = BuildTextualAtom (unit, unitedAtomsNames);
                atoms.Add (unitingAtom);
            }
        }


        private List<InsideImage> GetImages ( string jsonPath ) 
        {
            List<InsideImage> pictures = new ();

            IEnumerable<IConfigurationSection> images =
                              GetterFromJson.GetIncludedItemsOfSection (new List<string> { "InsideImages : Items" }, jsonPath);

            foreach ( IConfigurationSection image in images )
            {
                InsideImage picture = BuildInsideImage (image);
                pictures.Add (picture);
            }

            return pictures;
        }


        private TextualAtom BuildTextualAtom ( string atomName, string jsonPath ) 
        {
            double width = GetterFromJson.GetSectionValue (new List<string> { atomName, "Width" }, jsonPath)
                .TranslateIntoDouble ();
            double height = GetterFromJson.GetSectionValue (new List<string> { atomName, "Height" }, jsonPath)
            .TranslateIntoDouble ();
            double topOffset = GetterFromJson.GetSectionValue (new List<string> { atomName, "TopOffset" }, jsonPath)
            .TranslateIntoDouble ();
            double leftOffset = GetterFromJson.GetSectionValue (new List<string> { atomName, "LeftOffset" }, jsonPath)
            .TranslateIntoDouble ();
            string alignment = GetterFromJson.GetSectionValue (new List<string> { atomName, "Alignment" }, jsonPath);
            double fontSize = GetterFromJson.GetSectionValue (new List<string> { atomName, "FontSize" }, jsonPath)
            .TranslateIntoDouble ();
            string fontFamily = GetterFromJson.GetSectionValue (new List<string> { atomName, "FontFamily" }, jsonPath);
            string shiftableString = GetterFromJson.GetSectionValue (new List<string> { atomName, "IsShiftableBelow" }, jsonPath);
            bool isShiftable = false;

            try 
            {
                int shiftableInt = Int32.Parse (shiftableString);
                isShiftable = Convert.ToBoolean (shiftableInt);
            }
            catch (Exception ex) {}

            TextualAtom atom = new TextualAtom (atomName, width, height, topOffset, leftOffset
                           , alignment, fontSize, fontFamily, null, isShiftable);

            return atom;
        }


        private TextualAtom BuildTextualAtom ( IConfigurationSection section, List<string> united )
        {
            IConfigurationSection childSection = section.GetSection ("Name");
            string atomName = GetterFromJson.GetSectionValue (childSection);
            childSection = section.GetSection ("Width");
            double width = GetterFromJson.GetSectionValue (childSection).TranslateIntoDouble ();
            childSection = section.GetSection ("Height");
            double height = GetterFromJson.GetSectionValue (childSection).TranslateIntoDouble ();
            childSection = section.GetSection ("TopOffset");
            double topOffset = GetterFromJson.GetSectionValue (childSection).TranslateIntoDouble ();
            childSection = section.GetSection ("LeftOffset");
            double leftOffset = GetterFromJson.GetSectionValue (childSection).TranslateIntoDouble ();
            childSection = section.GetSection ("Alignment");
            string alignment = GetterFromJson.GetSectionValue (childSection);
            childSection = section.GetSection ("FontSize");
            double fontSize = GetterFromJson.GetSectionValue (childSection).TranslateIntoDouble ();
            childSection = section.GetSection ("FontFamily");
            string fontFamily = GetterFromJson.GetSectionValue (childSection);
            childSection = section.GetSection ("IsShiftableBelow");
            string shiftableString = GetterFromJson.GetSectionValue (childSection);
            bool isShiftable = false;

            try
            {
                int shiftableInt = Int32.Parse (shiftableString);
                isShiftable = Convert.ToBoolean (shiftableInt);
            }
            catch ( Exception ex ) { }


            TextualAtom atom = new TextualAtom ( atomName, width, height, topOffset, leftOffset 
                                               , alignment, fontSize, fontFamily, united, isShiftable );
            return atom;
        }


        private InsideImage BuildInsideImage ( IConfigurationSection section )
        {
            IConfigurationSection childSection = section.GetSection ( "Name" );
            string imageName = GetterFromJson.GetSectionValue ( childSection );
            
            childSection = section.GetSection ( "Width" );
            double width = GetterFromJson.GetSectionValue ( childSection ).TranslateIntoDouble ( );
            
            childSection = section.GetSection ( "Height" );
            double height = GetterFromJson.GetSectionValue ( childSection ).TranslateIntoDouble ( );
            
            childSection = section.GetSection ( "TopOffset" );
            double topOffset = GetterFromJson.GetSectionValue ( childSection ).TranslateIntoDouble ( );

            childSection = section.GetSection ( "LeftOffset" );
            double leftOffset = GetterFromJson.GetSectionValue ( childSection ).TranslateIntoDouble ( );

            childSection = section.GetSection ( "ImagePath" );
            string path = GetterFromJson.GetSectionValue ( childSection );

            childSection = section.GetSection ( "Color" );
            string color = GetterFromJson.GetSectionValue ( childSection );
            
            childSection = section.GetSection ( "ImageGeometricElementName" );
            string geometricElement = GetterFromJson.GetSectionValue ( childSection );

            InsideImage image = 
                       new InsideImage ( path, new Size ( width , height ), color, geometricElement, topOffset, leftOffset );
            return image;
        }


        public List<string> GetBadgeTemplateNames ()
        {
            List<string> templateNames = new ();

            foreach ( KeyValuePair<string, string> template in nameAndJson )
            {
                templateNames.Add (template.Key);
            }

            return templateNames;
        }


        public List<FileInfo> GetBadgeModelsNames ()
        {
            string badgeModelsFolderPath = @"./";
            DirectoryInfo modelFileDirectory = new DirectoryInfo (badgeModelsFolderPath);
            FileInfo [] Files = modelFileDirectory.GetFiles ("*.jpg");
            List<FileInfo> modelNames = new List<FileInfo> ();

            foreach ( FileInfo file in Files )
            {
                modelNames.Add (file);
            }

            return modelNames;
        }


        public List<FileInfo> GetBadgeModelsNamess ()
        {
            string badgeModelsFolderPath = @"./";
            DirectoryInfo modelFileDirectory = new DirectoryInfo (badgeModelsFolderPath);
            FileInfo [] Files = modelFileDirectory.GetFiles ("*.jpg");
            List<FileInfo> modelNames = new List<FileInfo> ();

            foreach ( FileInfo file in Files )
            {
                modelNames.Add (file);
            }

            return modelNames;
        }




    }
}