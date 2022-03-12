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


        public Interpreter(List<Tokens> t)
        {
            tokens = new List<Tokens>(t);
            errorMsg = new List<string>();
            tCounter = tCounter2 = whileStartCounter = whileStopCounter = 0;
            foundStart = foundStop = false;
            map = new Dictionary<string, object>();

        }
        public int Parse()
        {
            object temp;
            {
                //Console.WriteLine("Total Count: " + tokens.Count);
                while (tCounter < tokens.Count)
                {
                    //Console.WriteLine("LEXEME: " + tokens[tCounter].Lexeme + " Count: " + tCounter);

                    switch (tokens[tCounter].Type)
                    {
                        case TokenType.VAR:

                            if (foundStart)
                            {
                                Console.WriteLine("Invalid Variable Declaration"); 
                                // errorMsg.Add(string.Format("Invalid variable declaration. Declaration after START at line {0}.", tokens[tCounter].Line + 1));
                                tCounter++;
                                break; 
                            } else


                            {
                                tCounter++;
                                ParseVar(); 
                            }
                            break;
                        case TokenType.AS:
                            tCounter++;
                            funcAs(); 
                            break;
                        case TokenType.START:
                            startCount++;
                            if (!foundStart)
                            {
                                //Console.WriteLine("There is a Start"); 
                                foundStart = true; 
                            } else
                            {
                                Console.WriteLine("Syntax Error. Incorrect usage of start"); 
                            }
                            tCounter++; 
                            break;
                        case TokenType.STOP:
                            stopCount++;
                            if (!foundStop && foundStart)
                            {
                                foundStop = true;
                                //Console.WriteLine("Inside StopLEXEME: " + tokens[tCounter].Lexeme + " Count: " + tCounter);
                            } else
                            {
                                Console.WriteLine("Syntax error. Incorrect usage of stop"); 
                            }
                            tCounter++; 
                            break;
                        case TokenType.IDENTIFIER:
                            tCounter++;
                            funcIdentifer(); 
                            
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
                    Console.WriteLine("Program execution failed. It has no STOP");
                return errorMsg.Count; 
            }
        }

        private void funcIdentifer()
        {
            int currLine = tokens[tCounter].Line;
            object temp;
            //to be continued
        }

        private void funcOutput()
        {
            //having tCounter does not make sense have to change it to much easier to understand
            string temp_identOut = "";
            int pos = 0;
            tCounter2 = tCounter;
            if(tokens[tCounter2].Type == TokenType.COLON)
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
                                Console.WriteLine("Variable not initialized"); 
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
                            Console.WriteLine("Type Error at Line: " + tokens[tCounter].Line);
                        }
                    }
                    else
                    {
                        map.Add(x, 0);
                        //Console.WriteLine("Inside AS: " + x); 
                    }

                }
                tCounter++;
                varList.Clear();
            }
        }

        private void ParseVar()
        {
            if(tokens[tCounter].Type == TokenType.IDENTIFIER)
            {
                varList.Add(tokens[tCounter].Lexeme);
                tCounter++;
                if (tokens[tCounter].Type == TokenType.EQUALS)
                {
                    temp_ident = tokens[tCounter - 1].Lexeme;
                    tCounter++;
                    switch (tokens[tCounter].Type)
                    {
                        case TokenType.INT_LIT:
                            declared.Add(temp_ident, (int)tokens[tCounter].Literal);
                            tCounter++;
                            break;
                        default:
                            Console.WriteLine("Syntax Error");
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
