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
        private List<string> forMultipleIden = new List<string>();
        public Operations(List<Tokens> tokens, List<string> error, Dictionary<string, object> var)
        {
            this.infixTokens = new List<Tokens>(tokens);
            this.errorMessages = error;
            this.variables = new Dictionary<string, object>(var);
        }
        public List<string> ErrorMessages { get { return errorMessages; } }
        public List<string> multipleIden { get { return forMultipleIden; } }

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
                || infixTokens[ctr].Type == TokenType.INT_LIT || infixTokens[ctr].Type == TokenType.BOOL_LIT
                || infixTokens[ctr].Type == TokenType.EQUALS || infixTokens[ctr].Type == TokenType.CHAR_LIT)
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
            Console.WriteLine("\nPOSTFIX EXPRESSION: ");
            foreach (Tokens x in postfix)
                Console.WriteLine(x.Type + ", " + x.Lexeme);
            Console.WriteLine("END OF POSTFIX");
            return postfix;
        }
        public string evaluateExpression(List<Tokens> postfix)
        {

            bool flag = false;
            Stack<string> stack = new Stack<string>();
            List<string> postfixValues = new List<string>();
            int ctr = 0, len = postfix.Count;
            double res = 0;
            string val;
            //Console.WriteLine("VAR COUNT " + variables.Count);
            //foreach (KeyValuePair<string, object> x in variables)
            //    Console.WriteLine(x.Key + ", " + x.Value);
            //copying
            while (ctr < len)
            {
                if (postfix[ctr].Type == TokenType.IDENTIFIER)
                {
                    val = Convert.ToString(variables[postfix[ctr].Lexeme]);
                    postfixValues.Add(val);
                }
                else if (postfix[ctr].Type == TokenType.FLOAT_LIT || postfix[ctr].Type == TokenType.INT_LIT
                    || postfix[ctr].Type == TokenType.BOOL_LIT || postfix[ctr].Type == TokenType.CHAR_LIT)
                {
                    val = postfix[ctr].Lexeme;
                    postfixValues.Add(val.ToString());
                }
                else if (isLogicOperator(postfix[ctr].Lexeme) || postfix[ctr].Type == TokenType.EQUALS)
                    postfixValues.Add(postfix[ctr].Lexeme);
                else
                {
                    errorMessages.Add(string.Format("Invalid infix expression at line" + (infixTokens[ctr].Line + 1)));
                    break;
                }
                ctr++;
            }
            ctr = 0;
            // postfix Values now contains a string of float_lit, int_lit, bool_lit, and operators
            while (ctr < len)
            {
                if (postfix[ctr].Type == TokenType.IDENTIFIER && postfix[ctr + 1].Type == TokenType.EQUALS)
                {
                    while (ctr != postfixValues.Count - 1)
                    {
                        // B = C = D = 1
                        forMultipleIden.Add(postfix[ctr].Lexeme);
                        ctr += 2;
                    }
                    stack.Push(postfixValues[ctr]);
                }
                else if (postfix[ctr].Type == TokenType.INT_LIT || postfix[ctr].Type == TokenType.FLOAT_LIT
                || postfix[ctr].Type == TokenType.IDENTIFIER || postfix[ctr].Type == TokenType.BOOL_LIT
                || postfix[ctr].Type == TokenType.CHAR_LIT)
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
                        double n1 = double.Parse(i1);
                        double n2 = double.Parse(i2);
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
            if (stack.Count != 0)
            {
                Console.WriteLine("TO BE POPPED: STRING " + stack.Peek().ToString().ToLower());
            }
            return stack.Pop();
        }
        public bool isDigit(string x)
        {
            int i = int.Parse(x);
            return (i >= 0 || i < 0);
        }
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

