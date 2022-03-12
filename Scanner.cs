using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFPL
{
    class Scanner
    {
        private readonly List<Tokens> tokens;
        private readonly string[] source;
        string[] stringSeparators = new string[] { "\r\n" };
        private static int line;
        private string currString;
        private int charCounter;
        private int currStringLength;
        private Tokens item;
        private static List<string> errorMsg;


        public List<Tokens> Tokens { get {  return tokens; } }
        public string[] Source { get { return source; } }

        public List<string> ErrorMsg { get { return errorMsg; } }
        public Scanner(string source)
        {
            tokens = new List<Tokens>();
            this.source = source.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
            line = 0;
            currString = ""; 
            charCounter = 0;
            errorMsg = new List<string>();
        }

        //Process the entire string
        public int Process()
        {
            int i = 0;
            while (i < source.Length)
            {
                ProcessLine();
                line++;
                i++;
                charCounter = 0; 
            }
            return errorMsg.Count != 0 ? 1 : 0; 
             
        }

        public char getNextChar()
        {
            return charCounter + 1 < currStringLength ? currString[charCounter + 1] : '|';
        }

        //Process line by line.
        //Adding character by character
        private void ProcessLine()
        {
          
            currString = source[line];
            currStringLength = currString.Length;
            
            while (charCounter < currStringLength)
            {
                char x = currString[charCounter];
                switch (x)
                {
                    //for declaration
                    case '=':
                        if (getNextChar() == '=') // == 
                        {
                            string temp = "" + x + getNextChar();
                            item = new Tokens(TokenType.EQUALS, temp, null, line); //adding the string as a token to the list
                            tokens.Add(item);
                            charCounter += 2;
                        }
                        else
                        {
                            item = new Tokens(TokenType.EQUALS, x.ToString(), null, line);
                            tokens.Add(item);
                            charCounter++;
                        }
                        break;
                    case ',':
                        item = new Tokens(TokenType.COMMA, x.ToString(), null, line);
                        tokens.Add(item);
                        charCounter++;
                        break;
                    case ':':
                        item = new Tokens(TokenType.COLON, x.ToString(), null, line);
                        tokens.Add(item);
                        charCounter++;
                        break;
                    case '"':
                        if ((getNextChar() == 'F' || getNextChar() == 'T'))
                        {
                            charCounter++;
                            char b = currString[charCounter];
                            //Console.WriteLine("b"+b);
                        }
                        else
                        {
                            item = new Tokens(TokenType.QUOTE, x.ToString(), null, line);
                            tokens.Add(item);
                            charCounter++;
                        }
                        break;
                    case ' ':
                        charCounter++;
                        break;
                    default:
                        if (isDigit(x))
                        {
                            isType(x);
                            break;

                        }
                        else if (isAlpha(x))
                        {
                            isIdentifier(x); 
                            break;
                        }
                        else if (isChar(x))
                        {
                            CharVal(x); 
                        } else
                        {
                            //errorMsg.Add(string.Format("Encountered unsupported character \"{0}\" at line {1}.\n", x, line + 1));
                            charCounter++; 
                        }
                        break; 
                }
            
            }
        }

        private void CharVal(char x)
        {
            int count = 0;
            var a = TokenType.BOOL_LIT;
            string temp = "";
            string temp2 = "";
            while (isChar(x))
            {
                if(count == 1)
                {
                    temp2 += x;
                }
                temp += x;
                x = getNextChar();
                charCounter++;
                count++; 
            }
        }

        private bool isChar(char x)
        {
            return(x == '\'' || (x >= 'a' && x <= 'z') || (x >= 'A' && x <= 'Z') || x == '_' || (x >= '0' && x <= '9'));
        }

        //Checking if the string is a variable keyword or not
        private void isIdentifier(char x)
        {
            string temp = ""; 
            while(isAlpha(x) || isDigit(x))
            {
                temp += x;
                x = getNextChar();
                charCounter++;
            }
            switch (temp)
            {
                case "START":
                    item = new Tokens(TokenType.START,temp, null, line);
                    tokens.Add(item);
                    break;
                case "STOP":
                    item = new Tokens(TokenType.STOP, temp, null, line);
                    tokens.Add(item);
                    break;
                case "VAR":
                    item = new Tokens(TokenType.VAR, temp, null, line);
                    tokens.Add(item);
                    break;
                case "AS":
                    item = new Tokens(TokenType.AS, temp, null, line);
                    tokens.Add(item);
                    break;
                case "INT":
                    item = new Tokens(TokenType.INT, temp, null, line);
                    tokens.Add(item);
                    break;
                case "FLOAT":
                    item = new Tokens(TokenType.FLOAT, temp, null, line);
                    tokens.Add(item);
                    break;
                case "BOOL":
                    item = new Tokens(TokenType.BOOL, temp, null, line);
                    tokens.Add(item);
                    break;
                case "OUTPUT":
                    item = new Tokens(TokenType.OUTPUT, temp, null, line);
                    tokens.Add(item);
                    break;
                default:
                    item = new Tokens(TokenType.IDENTIFIER, temp, null, line);
                    //Console.WriteLine(item.Lexeme); 
                    tokens.Add(item);
                    break;
            }
        }

        //Check if it is alpha
        private bool isAlpha(char x)
        {
            return(x >= 'a' && x <= 'z') || (x >= 'A' && x <= 'Z') || x == '_';
        }

        //Check if Float Or Integer
        private void isType(char x) 
        {
            var a = TokenType.INT_LIT;
            string temp = "";
            //Checking if everything is a number
            while (isDigit(x))
            {
                temp += x;
                x = getNextChar();
                charCounter++; 
            }
            if(x == '.')
            {
                temp += x;
                a = TokenType.FLOAT_LIT;
                x = getNextChar();
                charCounter++;
                while (isDigit(x))
                {
                    temp += x;
                    x = getNextChar();
                    charCounter++;
                }
            }
            if(a == TokenType.INT_LIT)
            {
                item = new Tokens(a, temp, Convert.ToInt32(temp), line);
                tokens.Add(item);
            } else
            {
                item = new Tokens(a, temp, Convert.ToDouble(temp), line);
                tokens.Add(item);
            }
        }

        private bool isDigit(char x)
        {
            return x >= '0' && x <= '9';
        }
    }
}
