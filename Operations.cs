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
            int ctr = 0, len = infixTokens.Count;
            List<Tokens> postfix = new List<Tokens>();
            Stack<Tokens> stack = new Stack<Tokens>();
            
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
                    while (stack.Count != 0 && isOperator(infixTokens[ctr].Lexeme[0]) && (getPrecedence(infixTokens[ctr].Lexeme) <= getPrecedence(stack.Peek().Lexeme)))
                        postfix.Add(stack.Pop());
                    
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
                    errorMessages.Add(string.Format("Invalid equation at line" + (infixTokens[ctr].Line + 1)));
                    break;
                }
                ctr++;
            }
            while (stack.Count != 0)
            {
                postfix.Add(stack.Pop());
            }
            return postfix;
        }
        public List<Tokens> logicInfixToPostFix()
        {
            int ctr = 0, len = infixTokens.Count;
            List<Tokens> postfix = new List<Tokens>();
            Stack<Tokens> stack = new Stack<Tokens>();

            while (ctr < len)
            {
                if (infixTokens[ctr].Lexeme[0] == '(')
                    stack.Push(infixTokens[ctr]);
                else if (infixTokens[ctr].Type == TokenType.IDENTIFIER || infixTokens[ctr].Type == TokenType.FLOAT_LIT
                || infixTokens[ctr].Type == TokenType.INT_LIT || infixTokens[ctr].Type == TokenType.BOOL_LIT)
                {
                    postfix.Add(infixTokens[ctr]);
                }
                else if (isLogicOperator(infixTokens[ctr].Lexeme))
                {
                    // check precedence of the scanned operator and the stacked operator
                    while (stack.Count != 0 && isLogicOperator(infixTokens[ctr].Lexeme) && (getPrecedence(infixTokens[ctr].Lexeme) <= getPrecedence(stack.Peek().Lexeme)))
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
                    errorMessages.Add(string.Format("Invalid equation at line" + (infixTokens[ctr].Line + 1)));
                    break;
                }
                ctr++;
            }
            while (stack.Count != 0)
                postfix.Add(stack.Pop());
            
            return postfix;
        }
        public bool evaluateBooleanExpression(List<Tokens> postfix)
        {
            bool flag = false;
            Stack<string> stack = new Stack<string>();
            List<string> postfixValues = new List<string>();
            int ctr = 0, res=0, len = postfix.Count;
            string val;
            Console.WriteLine("VAR COUNT " + variables.Count);
            foreach (KeyValuePair<string, object> x in variables)
                Console.WriteLine(x.Key + ", " + x.Value);
            //copying
            while (ctr < len)
            {
                if (postfix[ctr].Type == TokenType.IDENTIFIER)
                {
                    val = Convert.ToString(variables[postfix[ctr].Lexeme]);
                    postfixValues.Add(val);
                }
                else if (postfix[ctr].Type == TokenType.FLOAT_LIT || postfix[ctr].Type == TokenType.INT_LIT 
                    || postfix[ctr].Type == TokenType.BOOL_LIT)
                {
                    val = postfix[ctr].Lexeme;
                    postfixValues.Add(val.ToString());
                }
                else if (isLogicOperator(postfix[ctr].Lexeme))
                    postfixValues.Add(postfix[ctr].Lexeme);
                else
                {
                    errorMessages.Add(string.Format("Invalid infix expression at line" + infixTokens[ctr].Line + 1));
                    break;
                }
                ctr++;
            }
            ctr = 0;
            // postfix Values now contains a string of float_lit, int_lit, bool_lit, and operators
            while (ctr < len)
            {
                if (postfix[ctr].Type == TokenType.INT_LIT || postfix[ctr].Type == TokenType.FLOAT_LIT
                || postfix[ctr].Type == TokenType.IDENTIFIER || postfix[ctr].Type == TokenType.BOOL_LIT)
                {
                    Console.WriteLine("Pushed " + postfix[ctr].Lexeme + ", " + postfix[ctr].Literal + " to the stack");
                    stack.Push(postfixValues[ctr]);
                }
                else if (isLogicOperator(postfixValues[ctr]))
                {
                    string i1 = stack.Pop();
                    string i2 = stack.Pop();
                    if (isOperator(postfixValues[ctr][0]))
                    {
                        int n1 = int.Parse(i1);
                        int n2 = int.Parse(i2);
                        if (postfixValues[ctr] == "+")
                            res = n2 + n1;
                        else if (postfixValues[ctr] == "-")
                            res = n2 - n1;
                        else if (postfixValues[ctr] == "*")
                            res = n2 * n1;
                        else if (postfixValues[ctr] == "/")
                            res = n2 / n1;
                        else if (postfixValues[ctr] == "%")
                            res = n2 % n1;
                        stack.Push(res.ToString());
                    }
                    else if (isRelational(postfixValues[ctr]))
                    {
                        int n1 = int.Parse(i1);
                        int n2 = int.Parse(i2);
                        if (postfixValues[ctr] == ">")
                            flag = n2 > n1;
                        else if (postfixValues[ctr] == ">=")
                            flag = n2 >= n1;
                        else if (postfixValues[ctr] == "==")
                            flag = n2 == n1;
                        else if (postfixValues[ctr] == "<>")
                            flag = n2 != n1;
                        else if (postfixValues[ctr] == "<")
                            flag = n2 < n1;
                        else if (postfixValues[ctr] == "<=")
                            flag = n2 <= n1;

                        stack.Push(flag.ToString());
                    }
                    else if (isLogic(postfixValues[ctr]))
                    {
                        bool b1 = Convert.ToBoolean(i1);
                        bool b2 = Convert.ToBoolean(i2);
                        if (postfixValues[ctr] == "AND")
                            flag = b2 && b1;
                        else if (postfixValues[ctr] == "OR")
                            flag = b2 || b1;

                        stack.Push(flag.ToString());
                    }
                    else
                        Console.Write("ELSE ASDASD");
                    
                }
                ctr++;
            }
            Console.WriteLine("STACK COUNT " + stack.Count());
            if(stack.Count != 0)
            {
                Console.WriteLine("STACK POPPED");
                Console.WriteLine("STRING "+ stack.Peek().ToString().ToLower());
                flag = bool.Parse(stack.Pop().ToString().ToLower());
            }
            return flag;
        }
        public int evaluateIntegerExpression(List<Tokens> postfix)
        {
            int res = 0;
            Stack<int> stack = new Stack<int>();
            List<string> postfixValues = new List<string>();
            int ctr = 0, val;
            int len = postfix.Count;
            //copying
            while (ctr < len)
            {
                if (postfix[ctr].Type == TokenType.IDENTIFIER)
                {
                    val = (int)variables[postfix[ctr].Lexeme];
                    postfixValues.Add(val.ToString());
                }
                else if (postfix[ctr].Type == TokenType.FLOAT_LIT || postfix[ctr].Type == TokenType.INT_LIT)
                {
                    val = int.Parse(postfix[ctr].Lexeme);
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
            while (ctr < len)
            {
                if (postfix[ctr].Type == TokenType.INT_LIT || postfix[ctr].Type == TokenType.FLOAT_LIT
                || postfix[ctr].Type == TokenType.IDENTIFIER)
                {
                    stack.Push(int.Parse(postfixValues[ctr]));
                }
                else if (isOperator(postfixValues[ctr][0]))
                {
                    int n1 = (int)stack.Pop();
                    int n2 = (int)stack.Pop();
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
            res = (int)stack.Pop();
            return res;
        }
        // public char evaluateCharPostfix(List<Tokens> postfix)
        //{

        //}
        public double evaluateFloatExpression(List<Tokens> postfix)
        {
            double res = 0.0, val;
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
                    stack.Push(Convert.ToDouble(postfixValues[ctr]));
                }
                else if (isOperator(postfixValues[ctr][0]))
                {
                    double n1 = Convert.ToDouble(stack.Pop());
                    double n2 = Convert.ToDouble(stack.Pop());
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
        public bool isLogicOperator(string x) 
        {
            return (x == "+" || x == "-" || x == "*" || x == "/" || x == "%" || x == "AND" || x == "OR" || x == "<"
                 || x == "<=" || x == "==" || x == "<>" || x == ">" || x == ">=");
        }
        public bool isRelational(string x)
        {
            return x == "<" || x == "<=" || x == "==" || x == "<>" || x == ">" || x == ">=";
        }
        public bool isLogic(string x)
        {
            return x == "AND" || x == "OR";
        }
        public int getPrecedence(string symbol) 
        {
            if (symbol == "*" || symbol == "/" || symbol == "%") // highest precedence
                return 4;
            else if (symbol == "+" || symbol == "-")
                return 3;
            else if (symbol == ">" || symbol == ">=" || symbol == "==" || symbol == "<>" || symbol == "<" || symbol == "<=") // relational operators
                return 2;
            else if (symbol == "AND" || symbol == "OR") // lowest precedence, logic operators
                return 1;
            else
                return 0;
        }
    }
}

