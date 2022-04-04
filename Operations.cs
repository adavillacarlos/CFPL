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
        public Operations() { }
        public List<string> ErrorMessages { get { return errorMessages; } }
        public List<string> multipleIden { get { return forMultipleIden; } }

        public List<Tokens> convertInfixToPostfix()
        {
            int ctr = 0, len = infixTokens.Count;
            List<Tokens> postfix = new List<Tokens>();
            Stack<Tokens> stack = new Stack<Tokens>();
            while (ctr < len)
            {
                if (infixTokens[ctr].Type == TokenType.LEFT_PAREN)
                {
                    stack.Push(infixTokens[ctr]);
                }
                else if (infixTokens[ctr].Type == TokenType.IDENTIFIER || infixTokens[ctr].Type == TokenType.FLOAT_LIT
                || infixTokens[ctr].Type == TokenType.INT_LIT || infixTokens[ctr].Type == TokenType.BOOL_LIT
                || infixTokens[ctr].Type == TokenType.EQUALS || infixTokens[ctr].Type == TokenType.CHAR_LIT)
                {
                    postfix.Add(infixTokens[ctr]);
                }
                else if (isOperator(infixTokens[ctr].Lexeme))
                {
                    // check precedence of the scanned operator and the stacked operator
                    while (stack.Count != 0 && isOperator(infixTokens[ctr].Lexeme) && (getPrecedence(infixTokens[ctr].Lexeme) <= getPrecedence(stack.Peek().Lexeme)))
                    {
                        postfix.Add(stack.Pop());
                    }
                    stack.Push(infixTokens[ctr]);
                }
                else if (infixTokens[ctr].Type == TokenType.RIGHT_PAREN)
                {
                    while (stack.Peek().Type != TokenType.LEFT_PAREN)
                    {
                        postfix.Add(stack.Pop());
                        if (stack.Count == 0)
                        {
                            errorMessages.Add("(infixToPostfix)Invalid equation at line" + (infixTokens[ctr].Line + 1));
                            break;
                        }

                    }
                    if (stack.Peek().Type == TokenType.LEFT_PAREN)
                    {
                        stack.Pop();
                    }
                    //  stack.Pop();
                }
                else
                {
                    Console.WriteLine(infixTokens[ctr].Type + ", " + infixTokens[ctr].Lexeme);
                    errorMessages.Add(string.Format("(infixToPostfix)Invalid equation at line" + (infixTokens[ctr].Line + 1)));
                    break;
                }
                ctr++;
            }
            while (stack.Count != 0)
            {

                //if (stack.Peek().Type == TokenType.LEFT_PAREN || stack.Peek().Type == TokenType.RIGHT_PAREN)
                //    stack.Pop();
                //else
                //{
                //    Console.WriteLine(stack.Peek().Type + ", " + stack.Peek().Lexeme + " popped");
                postfix.Add(stack.Pop());
                //}

            }
            Console.WriteLine("\nPOSTFIX EXPRESSION: ");
            foreach (Tokens x in postfix)
                Console.WriteLine(x.Type + ", " + x.Lexeme);
            Console.WriteLine("END OF POSTFIX\n");
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
                    //postfixValues.Add(val.ToString());
                    postfixValues.Add(val);
                }
                else if (isOperator(postfix[ctr].Lexeme) || postfix[ctr].Type == TokenType.EQUALS)
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
                //postfix.Count > 1 &&
                if (postfixValues.Count != 1 &&
                    (postfix[ctr].Type == TokenType.IDENTIFIER && postfix[ctr + 1].Type == TokenType.EQUALS))
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
                    //Console.WriteLine("Pushed " + postfix[ctr].Lexeme + ", " + postfix[ctr].Literal + " to the stack");
                    stack.Push(postfixValues[ctr]);
                }
                else if (isOperator(postfixValues[ctr]))
                {
                    string i1 = stack.Pop();
                    string i2 = stack.Pop();
                    if (isArithmeticOperator(postfixValues[ctr]))
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
                    else if (postfixValues[ctr] == "==" || postfixValues[ctr] == "<>")
                    {
                        if (postfix[ctr - 2].Type == TokenType.IDENTIFIER)
                        {
                            string iden = postfix[ctr - 2].Lexeme;
                            if (variables[iden].GetType() == typeof(char))
                            {
                                char c1 = char.Parse(i1);
                                char c2 = char.Parse(i2);
                                if (postfixValues[ctr] == "==")
                                    flag = c1 == c2;
                                else
                                    flag = c1 != c2;
                            }
                            else if (variables[iden].GetType() == typeof(int))
                            {
                                int c1 = int.Parse(i1);
                                int c2 = int.Parse(i2);
                                if (postfixValues[ctr] == "==")
                                    flag = c1 == c2;
                                else
                                    flag = c1 != c2;
                            }
                            else if (variables[iden].GetType() == typeof(double))
                            {
                                double c1 = double.Parse(i1);
                                double c2 = double.Parse(i2);
                                if (postfixValues[ctr] == "==")
                                    flag = c1 == c2;
                                else
                                    flag = c1 != c2;
                            }
                            else if (variables[iden].GetType() == typeof(string))
                            {
                                bool c1 = bool.Parse(i1);
                                bool c2 = bool.Parse(i2);
                                if (postfixValues[ctr] == "==")
                                    flag = c1 == c2;
                                else
                                {
                                    flag = c1 != c2;
                                }
                                flag = c1 != c2;
                            }
                        }
                        else if (postfix[ctr - 1].Type == TokenType.IDENTIFIER)
                        {
                            string iden = postfix[ctr - 1].Lexeme;
                            if (variables[iden].GetType() == typeof(char))
                            {
                                char c1 = char.Parse(i1);
                                char c2 = char.Parse(i2);
                                if (postfixValues[ctr] == "==")
                                    flag = c1 == c2;
                                else
                                    flag = c1 != c2;
                            }
                            else if (variables[iden].GetType() == typeof(int))
                            {
                                int c1 = int.Parse(i1);
                                int c2 = int.Parse(i2);
                                if (postfixValues[ctr] == "==")
                                    flag = c1 == c2;
                                else
                                    flag = c1 != c2;
                            }
                            else if (variables[iden].GetType() == typeof(double))
                            {
                                double c1 = double.Parse(i1);
                                double c2 = double.Parse(i2);
                                if (postfixValues[ctr] == "==")
                                    flag = c1 == c2;
                                else
                                    flag = c1 != c2;
                            }
                            else if (variables[iden].GetType() == typeof(string))
                            {
                                bool c1 = bool.Parse(i1);
                                bool c2 = bool.Parse(i2);
                                if (postfixValues[ctr] == "==")
                                    flag = c1 == c2;
                                else
                                {
                                    flag = c1 != c2;
                                }
                                // flag = c1 != c2;
                            }
                        }
                        else // literals
                        {
                            if (postfix[ctr - 2].Type == TokenType.CHAR_LIT || postfix[ctr - 1].Type == TokenType.CHAR_LIT)
                            {
                                char c1 = char.Parse(i1);
                                char c2 = char.Parse(i2);
                                if (postfixValues[ctr] == "==")
                                    flag = c1 == c2;
                                else
                                    flag = c1 != c2;
                            }
                            else if (postfix[ctr - 2].Type == TokenType.INT_LIT || postfix[ctr - 1].Type == TokenType.INT_LIT)
                            {
                                int c1 = int.Parse(i1);
                                int c2 = int.Parse(i2);
                                if (postfixValues[ctr] == "==")
                                    flag = c1 == c2;
                                else
                                    flag = c1 != c2;
                            }
                            else if (postfix[ctr - 2].Type == TokenType.FLOAT_LIT || postfix[ctr - 1].Type == TokenType.FLOAT_LIT)
                            {
                                double c1 = double.Parse(i1);
                                double c2 = double.Parse(i2);
                                if (postfixValues[ctr] == "==")
                                    flag = c1 == c2;
                                else
                                    flag = c1 != c2;
                            }
                            else if (postfix[ctr - 2].Type == TokenType.BOOL_LIT || postfix[ctr - 1].Type == TokenType.BOOL_LIT)
                            {
                                bool c1 = bool.Parse(i1);
                                bool c2 = bool.Parse(i2);
                                if (postfixValues[ctr] == "==")
                                {
                                    flag = c1 == c2;
                                }
                                else
                                {
                                    flag = c1 != c2;
                                }

                            }
                        }
                        stack.Push(flag.ToString());
                    }
                    else if (isRelationalOperator(postfixValues[ctr]))
                    {
                        int n1 = int.Parse(i1);
                        int n2 = int.Parse(i2);
                        if (postfixValues[ctr] == ">")
                            flag = n2 > n1;
                        else if (postfixValues[ctr] == ">=")
                            flag = n2 >= n1;
                        else if (postfixValues[ctr] == "<")
                            flag = n2 < n1;
                        else if (postfixValues[ctr] == "<=")
                            flag = n2 <= n1;

                        stack.Push(flag.ToString());
                    }
                    else if (isLogicalOperator(postfixValues[ctr]))
                    {
                        if (i1 == "True" || i1 == "False")
                        { }
                        else
                        {
                            errorMessages.Add("Invalid boolean expression at line " + (postfix[ctr].Line + 1));
                            return "error";
                        }
                        if (i2 == "True" || i2 == "False")
                        { }
                        else
                        {
                            errorMessages.Add("Invalid boolean expression at line " + (postfix[ctr].Line + 1));
                            return "error";
                        }
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
            //Console.WriteLine("STACK COUNT " + stack.Count());
            if (stack.Count != 0)
            {
                Console.WriteLine("TO BE POPPED: STRING " + stack.Peek().ToString());
            }
            return stack.Pop();
        }
        // helper functions
        public bool isDigit(string x)
        {

            double i = double.Parse(x);
            return (i >= 0 || i < 0);
        }
        public bool isArithmeticOperator(string x)
        {
            return (x == "+" || x == "-" || x == "*" || x == "/" || x == "%");
        }
        public bool isOperator(string x)
        {
            return (x == "+" || x == "-" || x == "*" || x == "/" || x == "%" // arithmetic operators
                 || x == "AND" || x == "OR" // logical operators
                 || x == "<" || x == "<=" || x == "==" || x == "<>" || x == ">" || x == ">="); // relational operators

            //return (isRelationalOperator(x) || isLogicalOperator(x) || isArithmeticOperator(x));
        }
        public bool isRelationalOperator(string x)
        {
            return x == "<" || x == "<=" || x == "==" || x == "<>" || x == ">" || x == ">=";
        }
        public bool isLogicalOperator(string x)
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

