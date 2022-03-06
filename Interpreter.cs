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

        public Interpreter(List<Tokens> t)
        {
            tokens = new List<Tokens>(t);
            tCounter = tCounter2 = whileStartCounter = whileStopCounter = 0;

        }
        public int Parse()
        {
            List<string> varList = new List<string>();
            Dictionary<string, object> declared = new Dictionary<string, object>();
            object temp;
            string temp_ident = "";

            {
                while(tCounter < tokens.Count)
                {
                    
                    tCounter++; 
                }
                return 0; 
            }
        }
    }
}
