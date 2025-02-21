using ContentAssembler;
using ExtentionsAndAuxiliary;
using Microsoft.Extensions.Configuration;
using NJsonSchema;
using NJsonSchema.Validation;
using System.Drawing.Text;
using System.Text;
using static System.Collections.Specialized.BitVector32;


namespace DataGateway
{
    public class BadgeAppearenceProvider : IBadgeAppearenceProvider, IMemberColorProvider
    {
        private static string _resourceFolder;
        private static FileInfo _schemeFile;

        private static readonly string _defaultSplitability = "0";
        
        private static Dictionary<string, string> _templateNameToJson;
        private static Dictionary<string, string> _templateToIncorrectLineBackground;
        private static Dictionary<string, string> _templateToIncorrectMemberBorderColor;
        private static Dictionary<string, string> _templateToCorrectMemberBorderColor;
        private static Dictionary<string, List<byte>> _templateToIncorrectMemberBorderThickness;
        private static Dictionary<string, List<byte>> _templateToCorrectMemberBorderThickness;
        private static Dictionary<string, BadgeLayout> _badgeLayouts;
        private static Dictionary<string, ICollection <ValidationError>> _jsonAndErrors;
        private static Dictionary<string, string> _incorrectJsonAndError;
        private static string _osName;

        private enum States
        {
            BeforeColon = 0,
            AfterColon = 1,
            BeforeName = 2,
            InName = 3
        }


        public static Task SetUp ( string resourceFolder, string jsonSchemeFolder, string osName )
        {
            Task task = new Task
            (() =>
            {
                BadgeAppearenceProvider badgeAppearenceProvider = new BadgeAppearenceProvider ();
                badgeAppearenceProvider.SetLayouts (resourceFolder, jsonSchemeFolder);
                Encoding.RegisterProvider (CodePagesEncodingProvider.Instance);
                _osName = osName;
            });

            task.Start ();

            return task;
        }


        public void SetLayouts ( string resourceFolder, string jsonSchemeFolder )
        {
            _resourceFolder = resourceFolder;

            _badgeLayouts = new Dictionary<string, BadgeLayout> ();
            _templateNameToJson = new ();
            _templateToIncorrectLineBackground = new ();
            _templateToIncorrectMemberBorderColor = new ();
            _templateToCorrectMemberBorderColor = new ();
            _templateToIncorrectMemberBorderThickness = new ();
            _templateToCorrectMemberBorderThickness = new ();
            _jsonAndErrors = new ();
            _incorrectJsonAndError = new ();

            DirectoryInfo containingDirectory = new DirectoryInfo (jsonSchemeFolder);
            FileInfo [] containingFiles = containingDirectory.GetFiles ("*.json");
            _schemeFile = containingFiles [0];

            containingDirectory = new DirectoryInfo (_resourceFolder);
            FileInfo [] fileInfos = containingDirectory.GetFiles ("*.json");

            foreach ( FileInfo fileInfo   in   fileInfos )
            {
                string jsonPath = fileInfo.FullName;
                string validationMessage = null;
                bool jsonIsValid = GetterFromJson.CheckJsonCorrectness (jsonPath, out validationMessage);

                if ( jsonIsValid )
                {
                    string templateName = GetSectionStrValue (new List<string> { "TemplateName" }, jsonPath);

                    bool nameShouldBeAdded = ! string.IsNullOrEmpty (templateName)
                                          && !_templateNameToJson.ContainsKey (templateName);

                    if ( nameShouldBeAdded )
                    {
                        _templateNameToJson.Add (templateName, jsonPath);
                    }

                    bool colorShouldBeAdded = ( nameShouldBeAdded   &&   ! string.IsNullOrEmpty (templateName) );

                    if ( colorShouldBeAdded )
                    {
                        SetBadgeComponetMarkers (jsonPath, templateName);
                    }
                }
                else
                {
                    string templateName;
                    bool isTemplate = TryFindTemplateFeature (jsonPath, out templateName);

                    if ( isTemplate ) 
                    {
                        _incorrectJsonAndError.Add(jsonPath, TranslateIncorrectJsonMessage(validationMessage));
                        _templateNameToJson.Add (templateName, jsonPath);

                        SetBadgeComponetMarkers (jsonPath, templateName);
                    }
                }
            }
        }


        private void SetBadgeComponetMarkers ( string jsonPath, string templateName )
        {
            string background = GetIncorrectMemberBackground (jsonPath);
            _templateToIncorrectLineBackground.Add (templateName, background);

            string incorrectBorderColor = GetIncorrectMemberBorderColor (jsonPath);
            _templateToIncorrectMemberBorderColor.Add (templateName, incorrectBorderColor);

            string correctBorderColor = GetCorrectMemberBorderColor (jsonPath);
            _templateToCorrectMemberBorderColor.Add (templateName, correctBorderColor);

            List<byte> incorrectBorderThickness = GetIncorrectBorderThickness (jsonPath);
            _templateToIncorrectMemberBorderThickness.Add (templateName, incorrectBorderThickness);

            List<byte> correctBorderThickness = GetCorrectBorderThickness (jsonPath);
            _templateToCorrectMemberBorderThickness.Add (templateName, correctBorderThickness);
        }


        private string GetIncorrectMemberBackground ( string jsonPath )
        {
            return GetSectionStrValueOrDefault (new List<string> { "IncorrectMemberSettings", "Background" }, jsonPath);
        }


        private string GetIncorrectMemberBorderColor ( string jsonPath )
        {
            return GetBorderColor (jsonPath, "IncorrectMemberSettings");
        }


        private string GetCorrectMemberBorderColor ( string jsonPath )
        {
            return GetBorderColor (jsonPath, "CorrectMemberSettings");
        }


        private List<byte> GetIncorrectBorderThickness ( string jsonPath )
        {
            return GetThickness (jsonPath, "IncorrectMemberSettings");
        }


        private List<byte> GetCorrectBorderThickness ( string jsonPath )
        {
            return GetThickness(jsonPath, "CorrectMemberSettings");
        }


        private List<byte> GetThickness ( string jsonPath, string tag )
        {
            List<byte> thickness = new ();

            byte left = ( byte ) GetSectionIntValueOrDefault
                (new List<string> { tag, "BorderThickness", "Left" }, jsonPath);
            thickness.Add (left);

            byte top = ( byte ) GetSectionIntValueOrDefault
                (new List<string> { tag, "BorderThickness", "Top" }, jsonPath);
            thickness.Add (top);

            byte right = ( byte ) GetSectionIntValueOrDefault
                (new List<string> { tag, "BorderThickness", "Right" }, jsonPath);
            thickness.Add (right);

            byte bottom = ( byte ) GetSectionIntValueOrDefault 
                (new List<string> { tag, "BorderThickness", "Bottom" }, jsonPath);
            thickness.Add (bottom);

            return thickness;
        }


        private string GetBorderColor ( string jsonPath, string tag )
        {
            return GetSectionStrValueOrDefault (new List<string> { tag, "BorderColor" }, jsonPath);
        }


        private string TranslateIncorrectJsonMessage ( string message )
        {
            string result = null;

            string seekable = "LineNumber: ";
            int lenght = seekable.Length;
            int incomingIndex = message.IndexOf (seekable);

            int endIndex = message.IndexOf ("|");

            string lineNumStr = message.Substring (incomingIndex + lenght, endIndex - ( incomingIndex + lenght ));

            result = "Ошибка на строке " + lineNumStr + " (" + message + ")";

            return result;
        }


        private bool TryFindTemplateFeature ( string jsonPath, out string templateName )
        {
            List<char> tempNameChars = new ();

            List<char> forbidenForName = new () { '\'', '(', ')', '{', '}', '[', ']' };

            string jsonText = File.ReadAllText ( jsonPath );
            int lenght = "\"TemplateName\"".Length;
            int incomingIndex = jsonText.IndexOf ("\"TemplateName\"");

            if ( incomingIndex > -1 ) 
            {
                States states = States.BeforeColon;

                int scratch = incomingIndex + lenght;

                for ( int index = scratch;   index < jsonText.Length;   index++ ) 
                {
                    char current = jsonText[index];

                    if ( states == States.BeforeColon ) 
                    {
                        if ( current == ' ' )
                        {
                            continue;
                        }
                        else if ( current == ':' ) 
                        {
                            states = States.AfterColon;
                        }
                        else 
                        {
                            templateName = string.Empty;
                            return false;
                        }
                    }
                    else if ( states == States.AfterColon ) 
                    {
                        if ( current == ' ' )
                        {
                            continue;
                        }
                        else if ( current == '"' )
                        {
                            states = States.BeforeName;
                        }
                        else
                        {
                            templateName = string.Empty;
                            return false;
                        }
                    }
                    else if ( states == States.BeforeName )
                    {
                        if ( ( current == ' ' )   ||   ( current == '"' )   ||   forbidenForName.Contains(current) )
                        {
                            templateName = string.Empty;
                            return false;
                        }
                        else
                        {
                            states = States.InName;
                            tempNameChars.Add( current );
                        }
                    }
                    else if ( states == States.InName )
                    {
                        if ( current == '"' )
                        {
                            templateName = new string (tempNameChars.ToArray());
                            return true;
                        }
                        else if ( forbidenForName.Contains (current) )
                        {
                            templateName = string.Empty;
                            return false;
                        }
                        else
                        {
                            tempNameChars.Add (current);
                            continue;
                        }
                    }
                }
            }

            templateName = string.Empty;
            return false;
        }


        public string GetBadgeImageUri ( string templateName )
        {
            string fileName = _templateNameToJson [ templateName ];
            fileName = GetSectionStrValueOrDefault ( new List<string> { "BackgroundImagePath" }, fileName) ?? "";
            string fileUri = _resourceFolder + fileName;
            return fileUri;
        }


        public string GetIncorrectLineBackgroundStr ( string templateName )
        {
            return _templateToIncorrectLineBackground [templateName];
        }


        public string GetIncorrectMemberBorderStr ( string templateName )
        {
            return _templateToIncorrectMemberBorderColor [templateName];
        }


        public string GetCorrectMemberBorderStr ( string templateName )
        {
            return _templateToCorrectMemberBorderColor [templateName];
        }


        public List<byte> GetIncorrectMemberBorderThickness ( string templateName )
        {
            return _templateToIncorrectMemberBorderThickness [templateName];
        }


        public List<byte> GetCorrectMemberBorderThickness ( string templateName )
        {
            return _templateToCorrectMemberBorderThickness [templateName];
        }


        public BadgeLayout GetBadgeLayout ( string templateName )
        {
            if ( _badgeLayouts.ContainsKey (templateName) )
            {
                return _badgeLayouts [templateName];
            }

            string jsonPath = _templateNameToJson [ templateName ];

            double badgeWidth = GetSectionIntValueOrDefault ( new List<string> { "Width" }, jsonPath );
            double badgeHeight = GetSectionIntValueOrDefault (new List<string> { "Height" }, jsonPath);

            List<double> spans = new List<double> ();

            double leftSpan = GetSectionIntValueOrDefault (new List<string> { "Padding", "Left" }, jsonPath);
            spans.Add (leftSpan);

            double topSpan = GetSectionIntValueOrDefault (new List<string> { "Padding", "Top" }, jsonPath);
            spans.Add(topSpan);

            double rightSpan = GetSectionIntValueOrDefault (new List<string> { "Padding", "Right" }, jsonPath);
            spans.Add (rightSpan);

            double bottomSpan = GetSectionIntValueOrDefault (new List<string> { "Padding", "Bottom" }, jsonPath);
            spans.Add (bottomSpan);

            List <TextualAtom> atoms = GetAtoms ( jsonPath );
            SetUnitingAtoms (atoms, jsonPath);

            List <InsideImage> images = GetImages ( jsonPath );
            List <InsideShape> shapes = GetShapes (jsonPath);

            BadgeLayout result = new BadgeLayout (badgeWidth, badgeHeight, templateName, spans, atoms, images, shapes);

            return result;
        }


        private List <TextualAtom> GetAtoms ( string jsonPath ) 
        {
            List <TextualAtom> atoms = new ();

            atoms.Add (BuildTextualAtom ("FamilyName", jsonPath, 0));
            atoms.Add (BuildTextualAtom ("FirstName", jsonPath, 1));
            atoms.Add (BuildTextualAtom ("PatronymicName", jsonPath, 2));
            atoms.Add (BuildTextualAtom ("Post", jsonPath, 3));
            atoms.Add (BuildTextualAtom ("Department", jsonPath, 4));

            return atoms;
        }


        private void SetUnitingAtoms ( List <TextualAtom> atoms, string jsonPath ) 
        {
            IEnumerable <IConfigurationSection> items =
                          GetterFromJson.GetIncludedItemsOfSection (new List<string> { "UnitedTextBlocks" }, jsonPath);

            int count = items.Count ();

            if ( items.Count() < 1 ) 
            {
                List<string> sectionPath = new () { "default", "UnitedTextBlocks" };
                items = GetterFromJson.GetIncludedItemsOfSection (sectionPath, _schemeFile.FullName);
                jsonPath = _schemeFile.FullName;
            }

            foreach ( IConfigurationSection item   in   items )
            {
                string path = item.Path;
                string numberStr = item.GetSection ("Number")?.Value;

                int number = 0;
                number = DigitalStringParser.ParseToInt (numberStr);

                if ( number == 0 ) 
                {
                    number = 1;
                }

                IConfigurationSection unitedSection = item.GetSection ("United");
                IEnumerable <IConfigurationSection> unitedSections = unitedSection.GetChildren();
                List<string> unitedAtomsNames = new List<string> ();

                foreach ( IConfigurationSection name   in   unitedSections )
                {
                    unitedAtomsNames.Add (name.Value);
                }

                TextualAtom unitingAtom = BuildTextualAtom (item, jsonPath, unitedAtomsNames, number);
                atoms.Add (unitingAtom);
            }
        }


        private List <InsideImage> GetImages ( string jsonPath ) 
        {
            List <InsideImage> images = new ();

            IEnumerable <IConfigurationSection> imageSections =
                              GetterFromJson.GetIncludedItemsOfSection (new List<string> { "InsideImages" }, jsonPath);

            foreach ( IConfigurationSection imageSection   in   imageSections )
            {
                InsideImage image = BuildInsideImage (imageSection);
                images.Add (image);
            }

            return images;
        }


        private List <InsideShape> GetShapes ( string jsonPath )
        {
            List <InsideShape> shapes = new ();

            IEnumerable <IConfigurationSection> shapeSections =
                              GetterFromJson.GetIncludedItemsOfSection (new List<string> { "InsideShapes" }, jsonPath);

            foreach ( IConfigurationSection shapeSection   in   shapeSections )
            {
                InsideShape shape = BuildInsideShape (shapeSection);
                shapes.Add (shape);
            }

            return shapes;
        }


        private TextualAtom BuildTextualAtom ( string atomName, string jsonPath, int numberToLocate ) 
        {
            double width = GetSectionIntValueOrDefault (new List<string> { atomName, "Width" }, jsonPath);
            double height = GetSectionIntValueOrDefault (new List<string> { atomName, "Height" }, jsonPath);
            double topOffset = GetSectionIntValueOrDefault (new List<string> { atomName, "TopOffset" }, jsonPath);
            double leftOffset = GetSectionIntValueOrDefault (new List<string> { atomName, "LeftOffset" }, jsonPath);
            string alignment = GetSectionStrValueOrDefault (new List<string> { atomName, "Alignment" }, jsonPath) ?? "";
            double fontSize = GetSectionIntValueOrDefault (new List<string> { atomName, "FontSize" }, jsonPath);
            string fontName = GetSectionStrValueOrDefault (new List<string> { atomName, "FontName" }, jsonPath) ?? "";

            string foreground = GetSectionStrValueOrDefault (new List<string> { atomName, "Foreground" }, jsonPath);
            
            string fontWeight = GetSectionStrValueOrDefault (new List<string> { atomName, "FontWeight" }, jsonPath) ?? "";
            bool isShiftable = GetSectionBoolValueOrDefault (new List<string> { atomName, "IsSplitable" }, jsonPath);

            TextualAtom atom = new TextualAtom (atomName, width, height, topOffset, leftOffset
                                               , alignment, fontSize, fontName, foreground
                                             , fontWeight, null, isShiftable, numberToLocate);

            return atom;
        }


        private TextualAtom BuildTextualAtom 
                        ( IConfigurationSection section, string jsonPath, List<string> united, int numberToLocate )
        {
            string atomName = section.GetSection ("Name")?.Value ?? "";

            IConfigurationSection childSection = section.GetSection ("Width");
            double width = GetSectionIntValueOrDefault (childSection, jsonPath, "Width");

            childSection = section.GetSection ("Height");
            double height = GetSectionIntValueOrDefault (childSection, jsonPath, "Height");

            childSection = section.GetSection ("TopOffset");
            double topOffset = GetSectionIntValueOrDefault (childSection, jsonPath, "TopOffset");

            childSection = section.GetSection ("LeftOffset");
            double leftOffset = GetSectionIntValueOrDefault (childSection, jsonPath, "LeftOffset");

            childSection = section.GetSection ("Alignment");
            string alignment = GetSectionStrValueOrDefault (childSection, jsonPath, "Alignment");

            childSection = section.GetSection ("FontSize");
            double fontSize = GetSectionIntValueOrDefault (childSection, jsonPath, "FontSize");

            childSection = section.GetSection ("FontName");
            string fontName = GetSectionStrValueOrDefault (childSection, jsonPath, "FontName");

            childSection = section.GetSection ("Foreground");
            string foreground = GetSectionStrValueOrDefault (childSection, jsonPath, "Foreground");

            childSection = section.GetSection ("FontWeight");
            string fontWeight = GetSectionStrValueOrDefault (childSection, jsonPath, "FontWeight");

            string shiftableString = section.GetSection ("IsSplitable")?.Value ?? _defaultSplitability;
            bool isShiftable = false;

            int shiftableInt = DigitalStringParser.ParseToInt (shiftableString);
            isShiftable = Convert.ToBoolean (shiftableInt);

            TextualAtom atom = new TextualAtom ( atomName, width, height, topOffset, leftOffset , alignment, fontSize
                                          , fontName, foreground, fontWeight, united, isShiftable, numberToLocate);
            return atom;
        }


        private InsideImage BuildInsideImage ( IConfigurationSection section )
        {
            double [] commonData = GetCommonData (section);

            string fileName = section.GetSection ("Path")?.Value ?? "";
            string fileUri = _resourceFolder + fileName;

            string bindingObjectName = section.GetSection ("BindingObject")?.Value ?? "";

            string isAbove = section.GetSection ("IsAboveOfBinding")?.Value ?? "";

            bool isAboveOfBinding = false;

            if ( isAbove == "yes" ) 
            {
                isAboveOfBinding = true;
            }

            InsideImage image = new InsideImage (fileUri, commonData [0], commonData [1], commonData [2], commonData [3]
                                               , bindingObjectName, isAboveOfBinding);
            return image;
        }


        private InsideShape BuildInsideShape ( IConfigurationSection section )
        {
            double [] commonData = GetCommonData (section);

            string kind = section.GetSection ("Type")?.Value ?? "";
            string fillColor = section.GetSection ("FillColor")?.Value ?? "#000000";
            string bindingObjectName = section.GetSection ("BindingObject")?.Value ?? "";
            string isAbove = section.GetSection ("IsAboveOfBinding")?.Value ?? "";

            bool isAboveOfBinding = false;

            if ( isAbove == "yes" )
            {
                isAboveOfBinding = true;
            }

            InsideShape shape = new InsideShape (commonData [0], commonData [1], commonData [2], commonData [3]
                                               , fillColor, kind
                                               , bindingObjectName, isAboveOfBinding);
            return shape;
        }


        private double[] GetCommonData ( IConfigurationSection section )
        {
            double[] result = new double [4];

            double.TryParse (section.GetSection ("Width")?.Value, out result [0]);
            double.TryParse(section.GetSection ("Height")?.Value, out result [1]);
            double.TryParse (section.GetSection ("TopOffset")?.Value, out result [2]);
            double.TryParse (section.GetSection ("LeftOffset")?.Value, out result [3]);

            return result;
        }


        public Dictionary <BadgeLayout, KeyValuePair <string, List<string>>> GetBadgeLayouts ()
        {
            Dictionary <BadgeLayout, KeyValuePair<string, List<string>>> layouts = new ();
            var schemeTask = JsonSchema.FromFileAsync (_schemeFile.FullName);
            schemeTask.Wait ();

            foreach ( KeyValuePair<string, string> template   in   _templateNameToJson )
            {
                string jsonPath = template.Value;

                try
                {
                    string json = File.ReadAllText (jsonPath);
                    var result = schemeTask.Result;

                    if ( result != null )
                    {
                        JsonSchemaValidator validator = new ();
                        JsonSchema schema = schemeTask.Result;
                        List<string> message = new ();


                        if ( _incorrectJsonAndError.ContainsKey (jsonPath) )
                        {
                            if ( !_jsonAndErrors.ContainsKey (jsonPath) )
                            {
                                message.Add (_incorrectJsonAndError [jsonPath]);
                            }
                        }
                        else 
                        {
                            ICollection <ValidationError> errors = validator.Validate (json, schema);
                            bool templateIsIncorrect = ( errors.Count != 0 );

                            if ( templateIsIncorrect )
                            {
                                List <ValidationError> children = new ();

                                foreach ( ValidationError err   in   errors )
                                {
                                    ChildSchemaValidationError childErr = err as ChildSchemaValidationError;

                                    if ( childErr != null )
                                    {
                                        var childErrors = childErr.Errors;

                                        foreach ( var chErr in childErrors )
                                        {
                                            if ( chErr.Value.Count > 0 )
                                            {
                                                foreach ( ValidationError er in chErr.Value )
                                                {
                                                    children.Add (er);
                                                }
                                            }
                                        }
                                    }
                                }

                                foreach ( ValidationError err   in   children )
                                {
                                    errors.Add (err);
                                }

                                foreach ( ValidationError err   in   errors )
                                {
                                    string messageLine = string.Empty;
                                    string errKind = TranslateErrorKindToRuss (err.Kind);
                                    messageLine += errKind;

                                    if ( err.Kind != ValidationErrorKind.PropertyRequired )
                                    {
                                        messageLine += " Ошибка в свойстве ";
                                        string propertyPath = err.Path;
                                        propertyPath = TrimWaste (propertyPath);
                                        messageLine += propertyPath + " на строке номер ";
                                        string lineNumber = err.LineNumber.ToString ();
                                        messageLine += lineNumber;
                                    }
                                    else
                                    {
                                        if ( err.Path != null   &&   err.Path.Length > 2 ) 
                                        {
                                            messageLine += err.Path.Substring(2, err.Path.Length - 2);
                                        }
                                    }

                                    messageLine += ";";
                                    message.Add (messageLine);
                                }

                                if ( !_jsonAndErrors.ContainsKey (jsonPath) )
                                {
                                    _jsonAndErrors.Add (jsonPath, errors);
                                }
                            }
                        }

                        List<string> uninstalledFonts = GetUninstalledFontsFrom (jsonPath);

                        foreach ( string uninstalledFont   in   uninstalledFonts ) 
                        {
                            string uninstalledFontMessage = "Не установлен шрифт с именем " + uninstalledFont;
                            message.Add (uninstalledFontMessage);
                        }

                        KeyValuePair <string, List<string>> jsonAndErrors = KeyValuePair.Create (jsonPath, message);
                        layouts.Add (GetBadgeLayout (template.Key), jsonAndErrors);
                    }
                }
                catch ( AggregateException ex )
                {
                    var mess = ex.Message;
                }
            }

            return layouts;
        }


        private List<string> GetUninstalledFontsFrom ( string jsonPath )
        {
            List<string> fontNames = new ();
            fontNames.Add (GetterFromJson.GetSectionStrValue (new List<string> { "CommonDefaultFontFamily" }, jsonPath));
            fontNames.Add (GetterFromJson.GetSectionStrValue (new List<string> { "FamilyName", "FontName" }, jsonPath));
            fontNames.Add (GetterFromJson.GetSectionStrValue (new List<string> { "FirstName", "FontName" }, jsonPath));
            fontNames.Add (GetterFromJson.GetSectionStrValue (new List<string> { "PatronymicName", "FontName" }, jsonPath));
            fontNames.Add (GetterFromJson.GetSectionStrValue (new List<string> { "Post", "FontName" }, jsonPath));
            fontNames.Add (GetterFromJson.GetSectionStrValue (new List<string> { "Department", "FontName" }, jsonPath));

            IEnumerable <IConfigurationSection> unitings =
                      GetterFromJson.GetIncludedItemsOfSection (new List<string> { "UnitedTextBlocks" }, jsonPath);

            foreach ( IConfigurationSection unit   in   unitings )
            {
                IConfigurationSection fontNameSection = unit.GetSection ("FontName");
                fontNames.Add (fontNameSection.Value);
            }

            return GetUninstalledFontsFrom (fontNames);
        }


        private List<string> GetUninstalledFontsFrom ( List<string> fontNames )
        {
            if ( _osName == "Windows" )
            {
                InstalledFontCollection ifc = new InstalledFontCollection ();
                System.Drawing.FontFamily [] families = ifc.Families;
                List<string> installed = new List<string> ();

                foreach ( var family   in   families )
                {
                    installed.Add (family.Name);
                }

                return GetUninstalled ( installed, fontNames );
            }
            else if ( _osName == "Linux" ) 
            {
                string fontInstallingCommand = "fc-list : family | sort | uniq";
                string result = TerminalCommandExecuter.ExecuteCommand (fontInstallingCommand);
                List <string> installed = result.Split ('\n').ToList();

                return GetUninstalled (installed, fontNames);
            }

            return new List<string> ();
        }


        private List<string> GetUninstalled ( List<string> installed, List<string> checkebles )
        {
            List<string> uninstalled = new ();

            foreach ( var name   in   checkebles )
            {
                bool isExisting = ! string.IsNullOrWhiteSpace (name);

                if ( isExisting   &&   ! installed.Contains (name) )
                {
                    uninstalled.Add (name);
                }
            }

            uninstalled = uninstalled.Distinct (StringComparer.OrdinalIgnoreCase).ToList ();

            return uninstalled;
        }


        private string TrimWaste ( string propertyPath )
        {
            string result = propertyPath;

            if ( result.First () == '#' )
            {
                result = result.Substring (1);
            }

            if ( result.First () == '/' )
            {
                result = result.Substring (1);
            }

            return result;
        }


        private string TranslateErrorKindToRuss ( ValidationErrorKind kind )
        {
            string errKind = string.Empty;

            switch ( kind )
            {
                case ValidationErrorKind.IntegerExpected:
                    errKind = "Ожидалось целое число.";
                    break;
                case ValidationErrorKind.NumberExpected:
                    errKind = "Ожидалось целое число.";
                    break;
                case ValidationErrorKind.BooleanExpected:
                    errKind = "Ожидалось логическое значение.";
                    break;
                case ValidationErrorKind.StringExpected:
                    errKind = "Ожидалась строка.";
                    break;
                case ValidationErrorKind.ObjectExpected:
                    errKind = "Ожидался объект.";
                    break;
                case ValidationErrorKind.ArrayExpected:
                    errKind = "Ожидался массив.";
                    break;
                case ValidationErrorKind.NumberTooBig:
                    errKind = "Превышение максимума.";
                    break;
                case ValidationErrorKind.NumberTooSmall:
                    errKind = "Ниже минимума.";
                    break;
                case ValidationErrorKind.ArrayItemNotValid:
                    errKind = "Ошибка в элементе массива.";
                    break;
                case ValidationErrorKind.PropertyRequired:
                    errKind = "Не найдено обязательное поле ";
                    break;
                case ValidationErrorKind.PatternMismatch:
                    errKind = "Некорректное значение цвета. Присутствуют недопустимые символы.";
                    break;
                case ValidationErrorKind.StringTooLong:
                    errKind = "Некорректное значение цвета. Слишком длинное.";
                    break;
                case ValidationErrorKind.StringTooShort:
                    errKind = "Некорректное значение цвета. Слишком короткое.";
                    break;

                default:
                    errKind = kind.ToString () + ".";
                    break;
            }

            return errKind;
        }


        private string GetSectionStrValueOrDefault ( List<string> keyPathInJson, string jsonPath )
        {
            if ( _jsonAndErrors.ContainsKey (jsonPath) ) 
            {
                ICollection <ValidationError> errors = _jsonAndErrors [jsonPath];

                foreach ( ValidationError err   in   errors )
                {
                    string propertyPath = TrimWaste (err.Path);
                    string [] steps = propertyPath.Split ('.', 10);

                    if ( keyPathInJson.SequenceEqual<string> (steps) )
                    {
                        List<string> keyPathToDefault = BuildPathToDefaultValue (steps);
                        string strValue = GetSectionStrValue (keyPathToDefault, _schemeFile.FullName);

                        return strValue;
                    }
                }
            }
            else if ( _incorrectJsonAndError.ContainsKey (jsonPath) )
            {
                List<string> keyPathToDefault = BuildPathToDefaultValue (keyPathInJson);
                string strValue = GetSectionStrValue (keyPathToDefault, _schemeFile.FullName);

                return strValue;
            }

            string result = GetterFromJson.GetSectionStrValue (keyPathInJson, jsonPath);
            bool errorsAbsentButSectionNotFound = ( result == null );

            if ( errorsAbsentButSectionNotFound ) 
            {
                List<string> keyPathToDefault = BuildPathToDefaultValue (keyPathInJson);
                result = GetterFromJson.GetSectionStrValue (keyPathToDefault, _schemeFile.FullName);
            }

            return result;
        }


        private int GetSectionIntValueOrDefault ( List<string> keyPathInJson, string jsonPath )
        {
            if ( _jsonAndErrors.ContainsKey(jsonPath) ) 
            {
                ICollection <ValidationError> errors = _jsonAndErrors [jsonPath];

                foreach ( ValidationError err   in   errors )
                {
                    string propertyPath = TrimWaste (err.Path);
                    string [] steps = propertyPath.Split ('.', 10);

                    if ( keyPathInJson.SequenceEqual<string> (steps) )
                    {
                        List<string> keyPathToDefault = BuildPathToDefaultValue (steps);
                        string strValue = GetSectionStrValue (keyPathToDefault, _schemeFile.FullName);

                        return DigitalStringParser.ParseToInt (strValue);
                    }
                }
            }
            else if ( _incorrectJsonAndError.ContainsKey (jsonPath) )
            {
                List<string> keyPathToDefault = BuildPathToDefaultValue (keyPathInJson);
                string strValue = GetSectionStrValue (keyPathToDefault, _schemeFile.FullName);

                return DigitalStringParser.ParseToInt (strValue);
            }

            int result = GetterFromJson.GetSectionIntValue (keyPathInJson, jsonPath);
            bool errorsAbsentButSectionNotFound = ( result == -1 );

            if ( errorsAbsentButSectionNotFound ) 
            {
                List<string> keyPathToDefault = BuildPathToDefaultValue (keyPathInJson);
                result = GetterFromJson.GetSectionIntValue (keyPathToDefault, _schemeFile.FullName);
            }

            return result;
        }


        private bool GetSectionBoolValueOrDefault ( List<string> keyPathInJson, string jsonPath )
        {
            bool result = false;

            if ( _jsonAndErrors.ContainsKey (jsonPath) ) 
            {

                ICollection <ValidationError> errors = _jsonAndErrors [jsonPath];

                foreach ( ValidationError err   in   errors )
                {
                    string propertyPath = TrimWaste (err.Path);

                    string [] steps = propertyPath.Split ('.', 10);

                    if ( keyPathInJson.SequenceEqual<string> (steps) )
                    {
                        List<string> keyPathToDefault = BuildPathToDefaultValue (steps);

                        string strValue = GetSectionStrValue (keyPathToDefault, _schemeFile.FullName);

                        if ( strValue == "True" )
                        {
                            result = true;
                        }

                        return result;
                    }
                }
            }
            else if ( _incorrectJsonAndError.ContainsKey (jsonPath) )
            {
                List<string> keyPathToDefault = BuildPathToDefaultValue (keyPathInJson);
                string strValue = GetSectionStrValue (keyPathToDefault, _schemeFile.FullName);

                if ( strValue == "True" )
                {
                    result = true;
                }

                return result;
            }

            return GetterFromJson.GetSectionBoolValue (keyPathInJson, jsonPath);
        }


        private string GetSectionStrValueOrDefault ( IConfigurationSection section, string jsonPath, string propertyName )
        {
            if ( _jsonAndErrors.ContainsKey (jsonPath) )
            {
                ICollection<ValidationError> errors = _jsonAndErrors [jsonPath];

                foreach ( ValidationError err   in   errors )
                {
                    if ( PathesEqual(section, err) )
                    {
                        List<string> keyPathToDefault = BuildPathToDefaultValue (section);
                        string strValue = GetSectionStrValue (keyPathToDefault, _schemeFile.FullName);

                        return strValue;
                    }
                }
            }
            else if ( _incorrectJsonAndError.ContainsKey (jsonPath) )
            {
                List<string> keyPathToDefault = BuildPathToDefaultValue (section);
                string strValue = GetSectionStrValue (keyPathToDefault, _schemeFile.FullName);

                return strValue;
            }

            return section.Value;
        }


        private int GetSectionIntValueOrDefault ( IConfigurationSection section, string jsonPath, string propertyName )
        {
            string sectionPath = section.Path;
            string [] sectionSteps = sectionPath.Split (':');

            if ( _jsonAndErrors.ContainsKey (jsonPath) )
            {
                ICollection<ValidationError> errors = _jsonAndErrors [jsonPath];

                foreach ( ValidationError err in errors )
                {
                    if ( PathesEqual (section, err) )
                    {
                        List<string> keyPathToDefault = BuildPathToDefaultValue (section);
                        string strValue = GetSectionStrValue (keyPathToDefault, _schemeFile.FullName);

                        return DigitalStringParser.ParseToInt (strValue);
                    }
                }
            }
            else if ( _incorrectJsonAndError.ContainsKey(jsonPath) ) 
            {
                List<string> keyPathToDefault = BuildPathToDefaultValue (section);
                string strValue = GetSectionStrValue (keyPathToDefault, _schemeFile.FullName);

                return DigitalStringParser.ParseToInt(strValue);
            }

            return DigitalStringParser.ParseToInt (section.Value);
        }


        private bool GetSectionBoolValueOrDefault ( IConfigurationSection section, string jsonPath, string propertyName )
        {
            bool result = false;

            if ( _jsonAndErrors.ContainsKey (jsonPath) )
            {
                ICollection <ValidationError> errors = _jsonAndErrors [jsonPath];

                foreach ( ValidationError err   in   errors )
                {
                    if ( PathesEqual(section, err) )
                    {
                        List<string> keyPathToDefault = BuildPathToDefaultValue (section);
                        string strValue = GetSectionStrValue (keyPathToDefault, _schemeFile.FullName);

                        if ( strValue == "True" )
                        {
                            result = true;
                        }

                        return result;
                    }
                }
            }
            else if ( _incorrectJsonAndError.ContainsKey (jsonPath) )
            {
                List<string> keyPathToDefault = BuildPathToDefaultValue (section);
                string strValue = GetSectionStrValue (keyPathToDefault, _schemeFile.FullName);

                if ( strValue == "True" )
                {
                    result = true;
                }

                return result;
            }

            result = string.Equals (section?.Value, "true", StringComparison.CurrentCultureIgnoreCase);

            return result;
        }


        public bool PathesEqual ( IConfigurationSection section, ValidationError err )
        {
            string [] sectionSteps = section.Path.Split (':');

            string propertyPath = TrimWaste (err.Path);
            string [] impureSteps = propertyPath.Split ('.', 10);

            bool metArrayIndexer = false;
            string pureStep = string.Empty;
            string indexator = string.Empty;

            int indexatorNum = 0;
            int counter = 0;

            foreach ( string step   in   impureSteps )
            {
                bool stepInArray = ( step.Last () == ']' );

                if ( stepInArray )
                {
                    int index = 0;

                    for ( ;   index < step.Length;   index++ )
                    {
                        if ( step [index] == '[' )
                        {
                            break;
                        }
                    }

                    pureStep = step.Substring (0, index);
                    indexator = step.Substring ((index + 1), step.Length - index - 2);
                    metArrayIndexer = true;
                    indexatorNum = counter;
                }

                counter++;
            }

            string [] steps = impureSteps;

            if ( metArrayIndexer ) 
            {
                steps = new string [impureSteps.Length + 1];

                for ( int index = 0;   index < steps.Length;   index++ )
                {
                    if ( index < indexatorNum )
                    {
                        steps [index] = impureSteps [index];
                    }
                    else if ( index == indexatorNum )
                    {
                        steps [index] = pureStep;
                    }
                    else if ( index == ( indexatorNum + 1 ) )
                    {
                        steps [index] = indexator.ToString ();
                    }
                    else 
                    {
                        steps [index] = impureSteps [index - 1];
                    }
                }
            }

            bool equals = true;

            if ( sectionSteps.Length != steps.Length ) 
            {
                return false;
            }

            for ( int index = 0;   index < sectionSteps.Length;   index++ )
            {
                if ( steps [index] != sectionSteps [index] ) 
                {
                    equals = false;
                    break;
                }
            }

            return equals;
        }


        private string GetSectionStrValue ( List<string> keyPathInJson, string jsonPath )
        {
            string result = GetterFromJson.GetSectionStrValue (keyPathInJson, jsonPath);

            return result;
        }


        private List<string> BuildPathToDefaultValue ( IConfigurationSection section )
        {
            List<string> keyPathInJsonScheme = new () { "default" };

            string [] keyPathInJson = section.Path.Split (':');

            foreach ( string step   in   keyPathInJson )
            {
                keyPathInJsonScheme.Add (step);
            }

            return keyPathInJsonScheme;
        }


        private List<string> BuildPathToDefaultValue ( ICollection<string> keyPathInJson )
        {
            List<string> keyPathInJsonScheme = new () { "default" };

            foreach ( string step   in   keyPathInJson )
            {
                keyPathInJsonScheme.Add (step);
            }

            return keyPathInJsonScheme;
        }
    }
}
