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


        public Interpreter(List<Tokens> t)
        {
            tokens = new List<Tokens>(t);
            tCounter = tCounter2 = whileStartCounter = whileStopCounter = 0;
            foundStart = false;
            map = new Dictionary<string, object>();

        }
        public int Parse()
        {
            object temp;

            {
                while(tCounter < tokens.Count)
                {
                    Console.WriteLine("LEXEME" + tokens[tCounter + 1].Lexeme);
                    switch (tokens[tCounter].Type)
                    {
                        case TokenType.VAR:

                            if (foundStart)
                            {
                                Console.WriteLine("Invalid Variable Declaration"); 
                                // errorMsg.Add(string.Format("Invalid variable declaration. Declaration after START at line {0}.", tokens[tCounter].Line + 1));
                                tCounter++;
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
                            tCounter++; 
                            break;
                        case TokenType.STOP:
                            tCounter++; 
                            break;
                        case TokenType.IDENTIFIER:
                            break;
                        case TokenType.INT_LIT:
                            temp = (int)tokens[tCounter].Literal;
                            tCounter++;
                            break;
                        default:
                            break; 
                    }
                }
                return 0; 
            }
        }

        private void funcAs()
        {
            if (tokens[tCounter].Type == TokenType.INT)
            {
                Console.WriteLine("Inside As");
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
            string temp_ident = "";
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
