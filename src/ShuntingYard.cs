using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

public class ShuntingYard
{
    // operandy a jejich priority
    private static readonly Dictionary<string, int> OperatorPrecedence = new Dictionary<string, int>
    {
        { "+", 1 }, { "-", 1 },
        { "*", 2 }, { "/", 2 },
        { "^", 3 }
    };

    
    public static List<string> ConvertToPostfix(string[] infixTokens)
    {
        List<string> postfix = new List<string>();
        Stack<string> operators = new Stack<string>();

        //projed symboly
        foreach (string token in infixTokens)
        {
            //pokud je to cislo, pi, nebo t pridej do listu (aka. queue)
            if (double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out _) || token == "t" || token == "pi")
            {
                postfix.Add(token);
            }
            //zavorky pridej do zasobniku
            else if (token == "(")
            {
                operators.Push(token);
            }
            else if (token == ")")
            {
                // pridavej dokud do listu a mazej ze zasobnik do otevirajici zavorky
                while (operators.Peek() != "(")
                {
                    postfix.Add(operators.Pop());
                }
                operators.Pop(); // odstran (
            }
            // Operator
            else if (IsOperator(token))
            {
                while (operators.Count > 0 && IsOperator(operators.Peek()) &&
                       OperatorPrecedence[operators.Peek()] >= OperatorPrecedence[token])
                {
                    postfix.Add(operators.Pop());
                }
                operators.Push(token);
            }
            else if (IsFunction(token)) // vyhod funkci
            {
                operators.Push(token);
            }
        }

        // pridej ostatni operatory 
        while (operators.Count > 0)
        {
            postfix.Add(operators.Pop());
        }

        return postfix;
    }

    
    private static bool IsOperator(string token)
    {
        return OperatorPrecedence.ContainsKey(token);// je oprator
    }

    // Helper method to check if a token is a function
    private static bool IsFunction(string token)
    {
        return token == "sin" || token == "cos" || token == "tan" || token == "exp"; //je funkce
    }

    
    public static double EvaluatePostfix(List<string> postfixTokens, double tValue)
    {
        Stack<double> stack = new Stack<double>();

        // projizdej symboly
        foreach (string token in postfixTokens)
        {
            if (double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out double number)) // Operand
            {
                stack.Push(number);
            }
            else if (token == "t") // hodnota t je na case
            {
                stack.Push(tValue);
            }
            else if (token == "pi") // hodnota pi
            {
                stack.Push(Math.PI);
            }
            else if (token == "e") // hodnota e
            {
                stack.Push(Math.E);
            }
            else if (IsOperator(token)) // operator
            {
                double rightOperand = stack.Pop();
                double leftOperand = stack.Pop();
                stack.Push(ApplyOperator(token, leftOperand, rightOperand));
            }
            else if (IsFunction(token)) // funkce
            {
                double argument = stack.Pop();
                stack.Push(ApplyFunction(token, argument));
            }
        }

        return stack.Pop(); // vysledek je jako jediny v zasobniku
    }

    // operator handler
    private static double ApplyOperator(string op, double left, double right)
    {
        
        if (op == "+")
        {
            return left + right; 
        }
        else if (op == "-")
        {
            return left - right; 
        }
        else if (op == "*")
        {
            return left * right; 
        }
        else if (op == "/")
        {
            return left / right;
        }
        else if (op == "^")
        {
            return Math.Pow(left, right); 
        }

        
        return 0; // operator proste neexistuje
    }

    // function handler
    private static double ApplyFunction(string func, double argument)
    {
        
        if (func == "sin")
        {
            return Math.Sin(argument);
        }
        else if (func == "cos")
        {
            return Math.Cos(argument); 
        }
        else if (func == "tan")
        {
            return Math.Tan(argument);
        }
        else if (func == "exp")
        {
            return Math.Exp(argument);
        }

        
        return 0; //funkce neexistuje
    }
}

