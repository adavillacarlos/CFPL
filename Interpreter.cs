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
        private static int tCounter, tCounter2, whileStartCounter, whileStopCounter;
        private static bool foundStart;
        private static List<string> errorMsg;

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
            errorMsg = new List<string>();
            tCounter = tCounter2 = whileStartCounter = whileStopCounter = 0;
            foundStart = foundStop = false;
            map = new Dictionary<string, object>();

        }

        public List<string> ErrorMsg { get { return errorMsg; } }

        public int Parse()
        {
            object temp;
            {
                while (tCounter < tokens.Count)
                {
                   
                    switch (tokens[tCounter].Type)
                    {
                        case TokenType.VAR:

                            if (foundStart)
                            {
                                msg = "Invalid variable declaration. Declaration after START at line " + (tokens[tCounter].Line + 1); 
                                errorMsg.Add(msg);
                                tCounter++;
                                break; 
                            } else
                            {
                                tCounter++;
                                ParseDeclaration();
                            }
                            break;
                         case TokenType.AS:
                            tCounter++;
                            funcAs(); 
                            break; 
                        case TokenType.START: //ERROR DETECTING NOT WORKING HUHU
                            startCount++;
                            if (!foundStart)
                            {
                                foundStart = true; 
                            } else
                            {
                                msg = "Syntax Error. Incorrect usage of start at line " + (tokens[tCounter].Line + 1);
                                errorMsg.Add(msg); 
                            }
                            tCounter++; 
                            break;
                        case TokenType.STOP:
                            stopCount++;
                            if (!foundStop && foundStart)
                            {
                                foundStop = true;
                            } else
                            {
                                msg = "Syntax Error. Incorrect usage of stop at line " + (tokens[tCounter].Line + 1);
                                errorMsg.Add(msg); 
                            }
                            tCounter++; 
                            break;
                        case TokenType.IDENTIFIER:
                            //should happen after the var
                            tCounter++;
                            temp_ident = tokens[tCounter++].Lexeme; 
                            funcIdentifer(temp_ident); 
                            break;
                        case TokenType.OUTPUT:
                            tCounter++;
                            funcOutput();
                            break; 
                        case TokenType.INT_LIT:
                            temp = (int)tokens[tCounter].Literal;
                            tCounter++;
                            break;
                        default:
                            tCounter++; 
                            break; 
                    }
                    temp_ident = "";
                    temp = null; 
                }
                if (!foundStop)
                {
                    msg = "Program execution failed."; 
                    errorMsg.Add(msg); 
                }
                    
                return errorMsg.Count; 
            }
        }

        private void funcIdentifer(string identifier)
        {
            int currLine = tokens[tCounter].Line;
            object temp;
            if(tokens[tCounter].Type == TokenType.EQUALS)
            {
                tCounter++;
                tCounter2 = tCounter;
                List <string>  expression= new List<string>; 
            }
        }

        private void funcOutput()
        {
            //having tCounter does not make sense have to change it to much easier to understand 
            string temp_identOut = "";
            int pos = 0;
            tCounter2 = tCounter;
            if (tokens[tCounter2].Type == TokenType.COLON)
            {
                tCounter2++;
                Console.WriteLine("Output: "); 
                while(tokens[tCounter2].Type == TokenType.IDENTIFIER) { 

                    switch(tokens[tCounter2].Type)
                    {
                        case TokenType.IDENTIFIER:
                            temp_identOut = tokens[tCounter2].Lexeme;
                            if (map.ContainsKey(temp_identOut))
                            {
                                Console.WriteLine(map[temp_identOut].ToString());
                                pos++;
                            } else
                            {
                                msg = "Variable not initialized at line"  + tokens[tCounter].Line + 1;
                                errorMsg.Add(msg); 
                                break; 
                            }
                            tCounter2++;
                            break; 
                        
                    }
                    
                }
            }
            
        }

        private void funcAs()
        {
            if (tokens[tCounter].Type == TokenType.INT)
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
                            msg= "Type Error at Line: " + tokens[tCounter].Line; 
                            Console.WriteLine(msg);
                        }
                    }
                    else
                    {
                        map.Add(x, 0);
                    }

                }
                tCounter++;
                varList.Clear();
            }
        }

        private void ParseDeclaration()
        {
            if(tokens[tCounter].Type == TokenType.IDENTIFIER)
            {
                varList.Add(tokens[tCounter].Lexeme);
                tCounter++;
                if (tokens[tCounter].Type == TokenType.EQUALS)
                {
                    temp_ident = tokens[tCounter - 1].Lexeme; //get the variable name 
                    tCounter++;
                    switch (tokens[tCounter].Type)
                    {
                        case TokenType.INT_LIT:
                            declared.Add(temp_ident, (int)tokens[tCounter].Literal);
                            tCounter++;
                            break;
                        default:
                            msg = "Syntax Error at line" + (tokens[tCounter].Line + 1);
                            errorMsg.Add(msg); 
                            tCounter++;
                            break;
                    }
                }
            } else
            {
              Console.WriteLine("Invalid variable declaration");
            }
        }
    }
}
