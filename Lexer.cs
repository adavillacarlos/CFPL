using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFPL
{
    class Lexer
    {
        private readonly List<Tokens> tokens;
        private readonly string[] source;
        string[] stringSeparators = new string[] { "\r\n" };
        private static int line;
        private string currString;
            private int charCounter;
            private int currStringLength;
        private Tokens item;
        private static List<string> errorMessages;
        string msg = ""; 

        public List<Tokens> Tokens { get {  return tokens; } }
        public List<string> ErrorMessages { get { return errorMessages; } }

        public Lexer(string source)
        {
            tokens = new List<Tokens>();
            this.source = source.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
            line = 0;
            currString = ""; 
            charCounter = 0;
            errorMessages = new List<string>();
        }

        //Analyze the entire string
        public int Analyze()
        {
            int i = 0;
            while (i < source.Length)
            {
                AnalyzeLine();
                line++;
                i++;
                charCounter = 0; 
            }
            return errorMessages.Count != 0 ? 1 : 0; 
             
        }

        public char getNextChar()
        {
            return charCounter +  1 < currStringLength ? currString[charCounter + 1] : '|';
        }

        //Analyze line by line.
        //Adding character by character
        private void AnalyzeLine()
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
                            item = new Tokens(TokenType.EQUAL, x.ToString(), null, line);
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
                            BooleanVal(b);
                        }
                        else
                        {
                            item = new Tokens(TokenType.DOUBLE_QUOTE, x.ToString(), null, line);
                            tokens.Add(item);
                            charCounter++;
                        }
                        break;
                    case ' ':
                        charCounter++;
                        break;
                    case '+':
                        tokens.Add(new Tokens(TokenType.ADD, x.ToString(), null, line));
                        charCounter++;
                        break;
                    case '-':
                        tokens.Add(new Tokens(TokenType.SUBT, x.ToString(), null, line));
                        charCounter++;
                        break;
                    case '/':
                        tokens.Add(new Tokens(TokenType.DIV, x.ToString(), null, line));
                        charCounter++;
                        break;
                    case '*':
                        if (charCounter == 0)
                        {
                            while (charCounter != currStringLength) { charCounter++; }
                        }
                        else
                        {
                            tokens.Add(new Tokens(TokenType.MULT, x.ToString(), null, line));
                            charCounter++;
                        }
                        break;
                    case '(':
                        tokens.Add(new Tokens(TokenType.LEFT_PAREN, x.ToString(), null, line));
                        charCounter++;
                        break;
                    case ')':
                        tokens.Add(new Tokens(TokenType.RIGHT_PAREN, x.ToString(), null, line));
                        charCounter++;
                        break;
                    case '~':
                        tokens.Add(new Tokens(TokenType.TILDE, x.ToString(), null, line));
                        charCounter++;
                        break;
                    case '>':
                        if (getNextChar() == '=')
                        {
                            string temp = "" + x + getNextChar();
                            tokens.Add(new Tokens(TokenType.GREATER_EQUAL, temp, null, line));
                            charCounter += 2;
                        }
                        else
                        {
                            tokens.Add(new Tokens(TokenType.GREATER, x.ToString(), null, line));
                            charCounter++;

                        }
                        break;
                    case '<':
                        if (getNextChar() == '=')
                        {
                            string temp = "" + x + getNextChar();
                            tokens.Add(new Tokens(TokenType.LESSER_EQUAL, temp, null, line));
                            charCounter += 2;
                        }
                        else
                        {
                            tokens.Add(new Tokens(TokenType.LESSER, x.ToString(), null, line));
                            charCounter++;
                        }
                        break;
                    case '%':
                        tokens.Add(new Tokens(TokenType.MOD, x.ToString(), null, line));
                        charCounter++;
                        break;
                    case '&':
                        tokens.Add(new Tokens(TokenType.AMPERSAND, x.ToString(), null, line));
                        charCounter++;
                        break;
                    case '#':
                        tokens.Add(new Tokens(TokenType.SHARP, x.ToString(), null, line));
                        charCounter++;
                        break;
                    case '[':
                        tokens.Add(new Tokens(TokenType.LEFT_BRACE, x.ToString(), null, line));
                        charCounter++;
                        break;
                    case ']':
                        tokens.Add(new Tokens(TokenType.RIGHT_BRACE, x.ToString(), null, line));
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
                            msg = "Encountered unsupported character: " +  x + " at line " + (line + 1);  
                            errorMessages.Add(msg);
                            Console.WriteLine(msg); 
                            charCounter++; 
                        }
                        break; 
                }
            
            }
        }

        private void BooleanVal(char a)
        {
            var t = TokenType.BOOL_LIT;
            string temp = "";
            while (isBoolean(a))
            {
                if (a == ' ')
                {
                    a = getNextChar();
                    charCounter++;
                }
                else
                {
                    temp += a;
                    a = getNextChar();
                    charCounter++;
                }
            }


            if (temp == "TRUE")
            {
                tokens.Add(new Tokens(t, temp, Convert.ToString("TRUE"), line));
            }
            else if (temp == "FALSE")
            {
                tokens.Add(new Tokens(t, temp, Convert.ToString("FALSE"), line));
            }
            else
            {
                errorMessages.Add(string.Format("Invalid value at line {0}.", line + 1));
            }

        }

        private bool isBoolean(char b)
        {
            return ((b >= 'A' && b <= 'Z'));
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
            if (count - 2 == 1)
            {
                a= TokenType.CHAR_LIT;
                tokens.Add(new Tokens(a, temp2, char.Parse(temp2), line));
            }
            
        }

        private bool isChar(char x)
        {
            return(x == '\'' || (x >= 'a' && x <= 'z') || (x >= 'A' && x <= 'Z') || x == '_' || (x >= '0' && x <= '9'));
        }

        //Checking if the string is t variable keyword or not
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
                    if (tokens[tokens.Count - 1].Type == TokenType.AS)
                        tokens.Add(new Tokens(TokenType.INT, temp, null, line));
                   else
                    {
                        msg = "Invalid usage of reserved word INT at line " + line + 1;
                        errorMessages.Add(msg);
                        Console.WriteLine(msg);
                    }
                    break;
                case "FLOAT":
                    if (tokens[tokens.Count - 1].Type == TokenType.AS)
                        tokens.Add(new Tokens(TokenType.FLOAT, temp, null, line));
                    else
                    {
                        msg = "Invalid usage of reserved word FLOAT at line " + line + 1;
                        errorMessages.Add(msg);
                        Console.WriteLine(msg);
                    }
                
                    break;
                case "BOOL":
                    item = new Tokens(TokenType.BOOL, temp, null, line);
                    tokens.Add(item);
                    break;
                case "INPUT":
                    tokens.Add(new Tokens(TokenType.INPUT, temp, null, line));
                    break;
                case "OUTPUT":
                    item = new Tokens(TokenType.OUTPUT, temp, null, line);
                    tokens.Add(item);
                    break;
                case "CHAR":
                    if(tokens[tokens.Count - 1].Type == TokenType.AS)
                    {
                        item = new Tokens(TokenType.CHAR, temp, null, line);
                        tokens.Add(item);
                    } else
                    {
                        msg = "Invalid usage of reserved word CHAR at line " + line + 1;
                        errorMessages.Add(msg);
                        Console.WriteLine(msg);
                    }
                    break;
                case "IF":

                    item = new Tokens(TokenType.IF, temp, null, line);
                    tokens.Add(item);

                    break;
                case "ELSE":
                    item = new Tokens(TokenType.ELSE, temp, null, line);
                    tokens.Add(item);
                    break;
                case "ELIF":
                    item = new Tokens(TokenType.ELIF, temp, null, line);
                    tokens.Add(item);

                    break;
                case "AND":
                    tokens.Add(new Tokens(TokenType.AND, temp, null, line));
                    break;
                case "OR":
                    tokens.Add(new Tokens(TokenType.OR, temp, null, line));
                    break;
                case "NOT":
                    tokens.Add(new Tokens(TokenType.NOT, temp, null, line));
                    break;
                case "TRUE":
                    tokens.Add(new Tokens(TokenType.TRUE, temp, null, line));
                    break;
                case "WHILE":
                    tokens.Add(new Tokens(TokenType.WHILE, temp, null, line));
                    break;
                case "FALSE":
                    tokens.Add(new Tokens(TokenType.FALSE, temp, null, line));
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
        private void isType(char a) 
        {
            var t = TokenType.INT_LIT;
            string temp = "";
            //Checking if everything is t number
            while (isDigit(a))
            {
                temp += a;
                a = getNextChar();
                charCounter++; 
            }
            if(a == '.')
            {
                temp += a;
                t = TokenType.FLOAT_LIT;
                a = getNextChar();
                charCounter++;
                while (isDigit(a))
                {
                    temp += a;
                    a = getNextChar();
                    charCounter++;
                }
            }
            if(t == TokenType.INT_LIT)
            {
                item = new Tokens(t, temp, Convert.ToInt32(temp), line);
                tokens.Add(item);
            } else
            {
                item = new Tokens(t, temp, Convert.ToDouble(temp), line);
                tokens.Add(item);
            }
        }

        private bool isDigit(char x)
        {
            return x >= '0' && x <= '9';
        }
    }
}
