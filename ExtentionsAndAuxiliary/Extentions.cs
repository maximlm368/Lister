using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using ContentAssembler;

namespace ExtentionsAndAuxiliary
{
    public static class StringExtention
    {
        public static List<string> SeparateTail ( this string str )
        {
            if ( string.IsNullOrWhiteSpace(str) ) 
            {
                return new ();
            }

            List<string> result = new (2);

            for ( int index = str.Length - 1;   index >= 0;   index-- )
            {
                if ( (str [index] == ' ')   ||   (str [index] == '-') )
                {
                    int gapLength = 1;
                    int endPartLength = str.Length - index - gapLength;

                    string secondPart = str.Substring (index + gapLength, endPartLength);
                    string firstPart = str.Substring (0, index);

                    result.Add (firstPart);
                    result.Add (secondPart);

                    break;
                }
            }

            return result;
        }


        public static string TrimLastNewLineChar ( this string beingProcessed )
        {
            bool isNotEmpty = !string.IsNullOrEmpty (beingProcessed);

            if ( isNotEmpty )
            {
                char lastChar = beingProcessed [beingProcessed.Length - 1];
                bool isGoal = ( lastChar == '\n' );

                if ( isGoal )
                {
                    beingProcessed = beingProcessed.Substring (0, beingProcessed.Length - 1);
                }
            }

            return beingProcessed;
        }


        public static List<string> SplitBySeparators ( this string str, char [] separators, char [] onceUnremovableSeparators )
        {
            List<string> result = new List<string> ();

            if ( ( separators == null )  ||  ( separators.Length < 1 )  ||  ( str == null ) )
            {
                return result;
            }

            string [] separatorStrs = new string [separators.Length];

            for ( int index = 0;   index < separators.Length;   index++ ) 
            {
                separatorStrs [index] = separators [index].ToString();
            }

            string rest = string.Empty;
            int splitingStart = 0;
            int splitingLength = 1;
            bool shouldSplit = false;
            bool isWaitingUnremovable = false;
            bool unremovableIsEncountered = false;

            for ( int index = 0;   index < str.Length - 1;   index++ )
            {
                if ( separators.Contains (str [index]) )
                {
                    string splited = str.Substring (splitingStart, splitingLength);
                    rest = str.Substring (index + 1, str.Length - index - 1);
                    splitingStart = index + 1;
                    splitingLength = 1;

                    if ( ( splited != string.Empty )   &&   ! separatorStrs.Contains (splited) )
                    {
                        result.Add (splited);
                    }

                    if ( onceUnremovableSeparators.Contains (str [index]) )
                    {
                        if ( result.Count > 0 ) 
                        {
                            string last = result.Last ();

                            if ( isWaitingUnremovable   &&   ( last != null ) )
                            {
                                char lastGlyph = last.Last ();

                                if ( separators.Contains(lastGlyph) ) 
                                {
                                    last = last.TrimEnd (lastGlyph);
                                }

                                last = last + str [index];
                                result [result.Count - 1] = last;
                            }
                        }

                        isWaitingUnremovable = false;
                        unremovableIsEncountered = true;
                    }
                    else if ( ! onceUnremovableSeparators.Contains (str [index])   &&   ! unremovableIsEncountered )
                    {
                        isWaitingUnremovable = true;
                    }
                }
                else
                {
                    splitingLength++;
                    isWaitingUnremovable = false;
                    unremovableIsEncountered = false;
                }
            }

            result.Add (rest);
            return result;
        }
    }



    public static class DigitalStringParser 
    {
        public static int ParseToInt ( string parsable )
        {
            if ( string.IsNullOrEmpty(parsable) ) 
            {
                return 0;
            }

            int result = 0;

            bool isInt = int.TryParse( parsable, out result );

            if ( ! isInt ) 
            {
                return 0;
            }

            return result;
        }


        public static byte ParseToByte ( string parsable )
        {
            if ( string.IsNullOrEmpty (parsable) )
            {
                return 0;
            }

            byte result = 0;

            bool isByte = byte.TryParse (parsable, out result);

            if ( ! isByte ) 
            {
                return 0;
            }

            return result;
        }
    }



    public static class TerminalCommandExecuter 
    {
        public static string ExecuteCommand ( string command )
        {
            using ( Process process = new Process () )
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{command}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                process.Start ();

                string result = process.StandardOutput.ReadToEnd ();

                process.WaitForExit ();

                return result;
            }
        }
    }
}
