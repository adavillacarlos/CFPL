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
    

        public Interpreter(List<Tokens> t)
        {
            tokens = new List<Tokens>(t);
            tCounter = tCounter2 = whileStartCounter = whileStopCounter = 0;
            foundStart = false; 

        }
        public int Parse()
        {
            object temp;
            string temp_ident = "";
            List<string> varList = new List<string>();
            Dictionary<string, object> declared = new Dictionary<string, object>();

            {
                while(tCounter < tokens.Count)
                {

                    switch (tokens[tCounter].Type)
                    {
                        case TokenType.VAR:
                            Console.WriteLine("Variable: " + tokens[tCounter+1].Lexeme);
                            if (foundStart)
                            {
                                Console.WriteLine("Invalid Variable Declaration"); 
                                // errorMsg.Add(string.Format("Invalid variable declaration. Declaration after START at line {0}.", tokens[tCounter].Line + 1));
                                tCounter++;
                            } else
                            {
                                tCounter++;
                                varList.Add(tokens[tCounter].Lexeme); 

                            }
                            break; 
                         default:
                                    break; 
                    }
                    tCounter++; 
                }
                return 0; 
            }
        }
    }
}
