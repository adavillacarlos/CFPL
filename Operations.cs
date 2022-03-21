using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFPL
{
    class Operations
    {
        private List<Tokens> infixTokens;
        private List<string> errorMessages;
        private Dictionary<string, object> variables;

        public Operations(List<Tokens> tokens, List<string> error, Dictionary<string, object> var)
        {
            this.infixTokens = new List<Tokens>(tokens);
            this.errorMessages = error;
            this.variables = new Dictionary<string, object>(var);
        }
        public List<string> ErrorMessages { get { return errorMessages; } }
        public List<Tokens> infixToPostFix()
        {
            int ctr = 0;
            List<Tokens> postfix = new List<Tokens>();
            Stack<Tokens> stack = new Stack<Tokens>();
            int len = infixTokens.Count;
            string str = "";
            while (ctr < len)
            {
                if (infixTokens[ctr].Lexeme[0] == '(')
                    stack.Push(infixTokens[ctr]);
                else if (infixTokens[ctr].Type == TokenType.IDENTIFIER || infixTokens[ctr].Type == TokenType.FLOAT_LIT
                || infixTokens[ctr].Type == TokenType.INT_LIT)
                {
                    postfix.Add(infixTokens[ctr]);
                }
                else if (isOperator(infixTokens[ctr].Lexeme[0]))
                {
                    // check precedence of the scanned operator and the stacked operator
                    while (stack.Count != 0 && isOperator(infixTokens[ctr].Lexeme[0]) && (getPrecedence(infixTokens[ctr].Lexeme[0]) <= getPrecedence(stack.Peek().Lexeme[0])))
                    {
                        postfix.Add(stack.Pop());
                    }

                    stack.Push(infixTokens[ctr]);
                }
                else if (infixTokens[ctr].Lexeme[0] == ')')
                {
                    while (stack.Peek().Type != TokenType.LEFT_PAREN)
                    {
                        postfix.Add(stack.Pop());
                    }
                    if (stack.Peek().Type == TokenType.LEFT_PAREN)
                        stack.Pop();
                }
                else
                {
                    errorMessages.Add(string.Format("Invalid equation at line" + infixTokens[ctr].Line + 1));
                    break;
                }
                Console.WriteLine(str);
                ctr++;
            }
            while (stack.Count != 0)
            {
                postfix.Add(stack.Pop());
            }
            return postfix;
        }
        /*public List<Tokens> boolInfixToPostfix()
        {
            List<Tokens> postfix = new List<Tokens>();
            Stack<Tokens> stack = new Stack<Tokens>();
        }*/
        public int evaluateIntegerExpression(List<Tokens> postfix)
        {
            int res=-99;
            Stack<int> stack = new Stack<int>();
            List<string> postfixValues = new List<string>();
            int ctr = 0, val;
            int len = postfix.Count;
            //copying
            while(ctr < len)
            {
                if (postfix[ctr].Type == TokenType.IDENTIFIER)
                {
                    val = (int)variables[postfix[ctr].Lexeme];
                    postfixValues.Add(val.ToString());
                }
                else if(postfix[ctr].Type == TokenType.FLOAT_LIT || postfix[ctr].Type == TokenType.INT_LIT)
                {
                    val = (int) postfix[ctr].Literal;
                    postfixValues.Add(val.ToString());
                }
                else if (isOperator(postfix[ctr].Lexeme[0]))
                    postfixValues.Add(postfix[ctr].Lexeme);
                else
                {
                    errorMessages.Add(string.Format("Invalid equation at line" + infixTokens[ctr].Line + 1));
                    break;
                }
                ctr++;
            }
            ctr = 0;
            // postfix Values now contains a string of int literals and operators
            while(ctr < len)
            {
                if(postfix[ctr].Type == TokenType.INT_LIT || postfix[ctr].Type == TokenType.FLOAT_LIT
                || postfix[ctr].Type == TokenType.IDENTIFIER)
                {
                    stack.Push(int.Parse(postfixValues[ctr]));
                }
                else if (isOperator(postfixValues[ctr][0]))
                {
                    int n1 = (int) stack.Pop();
                    int n2 = (int) stack.Pop();
                    if(postfixValues[ctr] == "+")
                        res = n2 + n1;
                    if (postfixValues[ctr] == "-")
                        res = n2 - n1;
                    if (postfixValues[ctr] == "*")
                        res = n2 * n1;
                    if (postfixValues[ctr] == "/")
                        res = n2 / n1;
                    if (postfixValues[ctr] == "%")
                        res = n2 % n1;

                    stack.Push(res);
                }
                ctr++;
            }
            res = (int)stack.Pop();
            return res;
        }
       // public char evaluateCharPostfix(List<Tokens> postfix)
        //{

        //}
        public double evaluateFloatExpression(List<Tokens> postfix)
        {
            double res=0.0, val;
            Stack<double> stack = new Stack<double>();
            List<string> postfixValues = new List<string>();
            int ctr = 0;
            int len = postfix.Count;
            //copying
            while (ctr < len)
            {
                if (postfix[ctr].Type == TokenType.IDENTIFIER)
                {
                    val = (double)variables[postfix[ctr].Lexeme];
                    postfixValues.Add(val.ToString());
                }
                else if (postfix[ctr].Type == TokenType.FLOAT_LIT || postfix[ctr].Type == TokenType.INT_LIT)
                {
                    val = double.Parse(postfix[ctr].Lexeme);
                    postfixValues.Add(val.ToString());
                }
                else if (isOperator(postfix[ctr].Lexeme[0]))
                    postfixValues.Add(postfix[ctr].Lexeme);
                else
                {
                    errorMessages.Add(string.Format("Invalid equation at line" + infixTokens[ctr].Line + 1));
                    break;
                }
                ctr++;
            }
            ctr = 0;
            // postfix Values now contains a string of float literals and operators
            while (ctr < len)
            {
                if (postfix[ctr].Type == TokenType.INT_LIT || postfix[ctr].Type == TokenType.FLOAT_LIT
                || postfix[ctr].Type == TokenType.IDENTIFIER)
                {
                    stack.Push(double.Parse(postfixValues[ctr]));
                }
                else if (isOperator(postfixValues[ctr][0]))
                {
                    double n1 = (double)stack.Pop();
                    double n2 = (double)stack.Pop();
                    if (postfixValues[ctr] == "+")
                        res = n2 + n1;
                    if (postfixValues[ctr] == "-")
                        res = n2 - n1;
                    if (postfixValues[ctr] == "*")
                        res = n2 * n1;
                    if (postfixValues[ctr] == "/")
                        res = n2 / n1;
                    if (postfixValues[ctr] == "%")
                        res = n2 % n1;

                    stack.Push(res);
                }
                ctr++;
            }
            res = (double)stack.Pop();
            return res;
        }
        /*
         public bool evaluateBooleanExpression(List<Tokens> postfix)
        {

        }
        */
        
        public bool isOperator(char x) // for binary only
        {
            return (x == '+' || x == '-' || x == '*' || x == '/' || x == '%');
        }
        public int getPrecedence(char symbol) // for binary only
        {
            if (symbol == '*' || symbol == '/' || symbol == '%') // highest precedence
                return 2;
            else if (symbol == '+' || symbol == '-')          // lowest precedence 
                return 1;
            else
                return 0;
        }
    }
}
