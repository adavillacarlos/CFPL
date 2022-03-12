using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFPL
{
    class Interpreter
    {
        private List<Tokens> tokens;
        private static int tokenCounter, tokenCounter2;
        private static bool foundStart;
        private static List<string> errorMessages;
        private static List<string> outputMessages; 

        List<string> varList = new List<string>();

        private static Dictionary<string, object> map;
        Dictionary<string, object> declared = new Dictionary<string, object>();
        private int startCount=0;
        private int stopCount;
        private bool foundStop;
        string temp_ident = "";
        string msg = "";
        int check=0; 

        public Interpreter(List<Tokens> t)
        {
            tokens = new List<Tokens>(t);
            errorMessages = new List<string>();
            outputMessages = new List<string>();
            tokenCounter = tokenCounter2 = 0; 
            foundStart = foundStop = false;
            map = new Dictionary<string, object>();

        }

        public List<string> ErrorMessages { get { return errorMessages; } }
        public List<string> OutputMessages { get { return outputMessages;  } }

        public int Parse()
        {
            object temp;
            {
                while (tokenCounter < tokens.Count)
                {
                    switch (tokens[tokenCounter].Type)
                    {
                        case TokenType.VAR:
                            //error messages does not work NGANO MAN KAIRIT HA
                            if (foundStart)
                            {
                                msg = "Invalid variable declaration. Declaration after START at line " + (tokens[tokenCounter].Line + 1); 
                                errorMessages.Add(msg);
                                tokenCounter++;
                                break; 
                            } else
                            {
                                tokenCounter++;
                                ParseDeclaration();
                            }
                            break;
                         case TokenType.AS:
                            tokenCounter++;
                            ParseAs(); 
                            break; 
                        case TokenType.START: //ERROR DETECTING NOT WORKING HUHU
                            startCount++;
                            if (!foundStart)
                            {
                                foundStart = true; 
                            } else
                            {
                                msg = "Syntax Error. Incorrect usage of start at line " + (tokens[tokenCounter].Line + 1);
                                errorMessages.Add(msg); 
                            }
                            tokenCounter++; 
                            break;
                        case TokenType.STOP:
                            stopCount++;
                            if (!foundStop && foundStart)
                            {
                               
                                foundStop = true;
                            } else
                            {
                                msg = "Syntax Error. Incorrect usage of stop at line " + (tokens[tokenCounter].Line + 1);
                                errorMessages.Add(msg); 
                            }
                            tokenCounter++; 
                            break;
                        case TokenType.IDENTIFIER:
                            //should happen after the var
                            //happens after variable declaration
                            tokenCounter++;
                            temp_ident = tokens[tokenCounter].Lexeme;
                            ParseIdentifier(temp_ident); 
                            break;
                        case TokenType.OUTPUT:
                            tokenCounter++;
                            ParseOutput();
                            break; 
                        case TokenType.INT_LIT:
                            temp = (int)tokens[tokenCounter].Literal;
                            tokenCounter++;
                            break;
                        default:
                            tokenCounter++; 
                            break; 
                    }
                    temp_ident = "";
                    temp = null; 
                }
                if (!foundStop)
                {
                    msg = "Program execution failed."; 
                    errorMessages.Add(msg); 
                }
                    
                return errorMessages.Count; 
            }
        }

        private void ParseIdentifier(string identifier)
        {
            int currentLine = tokens[tokenCounter].Line;
            object temp;
            if(tokens[tokenCounter].Type == TokenType.EQUALS)
            {
                tokenCounter++;
                tokenCounter2 = tokenCounter;
                List <string> expression= new List<string>();
                string a = "";
                if (map.ContainsKey(identifier))
                {
                    Console.WriteLine(identifier);
                    while (tokens[tokenCounter2].Line == currentLine)
                    {
                        Console.WriteLine("Inside line"); 
                    }
                }
                
            }
        }

        private void ParseOutput()
        {
            //having tokenCounter does not make sense have to change it to much easier to understand 
            string temp_identOut = "";
            string output = ""; 
            int pos = 0;
            tokenCounter2 = tokenCounter;
            if (tokens[tokenCounter2].Type == TokenType.COLON)
            {
                tokenCounter2++;
                Console.WriteLine("Output: "); 
                while(tokens[tokenCounter2].Type == TokenType.IDENTIFIER) { 

                    switch(tokens[tokenCounter2].Type)
                    {
                        case TokenType.IDENTIFIER:
                            temp_identOut = tokens[tokenCounter2].Lexeme;
                            if (map.ContainsKey(temp_identOut))
                            {
                                output = map[temp_identOut].ToString(); 
                                Console.WriteLine(output);
                                outputMessages.Add(output); 
                                pos++;
                            } else
                            {
                                msg = "Variable not initialized at line"  + tokens[tokenCounter].Line + 1;
                                errorMessages.Add(msg); 
                                break; 
                            }
                            tokenCounter2++;
                            break; 
                        
                    }
                    
                }
            }
            
        }

        private void ParseAs()
        {
            if (tokens[tokenCounter].Type == TokenType.INT)
            {
                foreach (string x in varList)
                {
                    if (declared.ContainsKey(x))
                    {
                        if (declared[x].GetType() == typeof(Int32))
                        {
                            map.Add(x, (Int32)declared[x]);
                        }
                        else
                        {
                            msg= "Type Error at Line: " + tokens[tokenCounter].Line; 
                            Console.WriteLine(msg);
                        }
                    }
                    else
                    {
                        map.Add(x, 0);
                    }

                }
                tokenCounter++;
                varList.Clear();
            }
        }

        private void ParseDeclaration()
        {
            if(tokens[tokenCounter].Type == TokenType.IDENTIFIER)
            {
                varList.Add(tokens[tokenCounter].Lexeme);
                tokenCounter++;
                if (tokens[tokenCounter].Type == TokenType.EQUALS)
                {
                    temp_ident = tokens[tokenCounter - 1].Lexeme; //get the variable name 
                    tokenCounter++;
                    switch (tokens[tokenCounter].Type)
                    {
                        case TokenType.INT_LIT:
                            declared.Add(temp_ident, (int)tokens[tokenCounter].Literal);
                            tokenCounter++;
                            break;
                        default:
                            msg = "Syntax Error at line" + (tokens[tokenCounter].Line + 1);
                            errorMessages.Add(msg); 
                            tokenCounter++;
                            break;
                    }
                }
            } else
            {
                msg = "Invalid variable declaration";
                errorMessages.Add(msg); 
            }
        }
    }
}
