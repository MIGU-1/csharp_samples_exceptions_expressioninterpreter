﻿using System;
using System.Text;

namespace ExpressionInterpreter.Logic
{
    public class Interpreter
    {
        private double _operandLeft;
        private double _operandRight;
        private char _op;  // Operator                  

        /// <summary>
        /// Eingelesener Text
        /// </summary>
        public string ExpressionText { get; private set; }
        public double OperandLeft => _operandLeft;
        public double OperandRight => _operandRight;
        public char Op => _op;

        public void Parse(string expressionText)
        {
            if (String.IsNullOrWhiteSpace(expressionText))
                throw new Exception("Ausdruck ist null oder empty!");

            ExpressionText = expressionText;
            ParseExpressionStringToFields();
        }
        /// <summary>
        /// Wertet den Ausdruck aus und gibt das Ergebnis zurück.
        /// Fehlerhafte Operatoren und Division durch 0 werden über Exceptions zurückgemeldet
        /// </summary>
        public double Calculate()
        {
            switch (_op)
            {
                case '+':
                    {
                        return _operandLeft + _operandRight;
                    }
                case '-':
                    {
                        return _operandLeft - _operandRight;
                    }
                case '*':
                    {
                        return _operandLeft * _operandRight;
                    }
                case '/':
                    {
                        if (_operandRight != 0)
                        {
                            return _operandLeft / _operandRight;
                        }
                        else
                        {
                            throw new DivideByZeroException("Exceptionmessage: Division durch 0 ist nicht erlaubt");
                        }
                    }
                default:
                    throw new Exception("Operant ist falsch gewählt!");
            }
        }
        /// <summary>
        /// Expressionstring in seine Bestandteile zerlegen und in die Felder speichern.
        /// 
        ///     { }[-]{ }D{D}[,D{D}]{ }(+|-|*|/){ }[-]{ }D{D}[,D{D}]{ }
        ///     
        /// Syntax  OP = +-*/
        ///         Vorzeichen -
        ///         Zahlen double/int
        ///         Trennzeichen Leerzeichen zwischen OP, Vorzeichen und Zahlen
        /// </summary>
        public void ParseExpressionStringToFields()
        {
            bool isNegative;

            for (int i = 0; i < ExpressionText.Length; i++)
            {
                SkipBlanks(ref i);
                isNegative = ScanSign(ref i);
                SkipBlanks(ref i);
                _operandLeft = isNegative ? ScanNumber(ref i) * -1 : ScanNumber(ref i);
                SkipBlanks(ref i);
                _op = ScanOp(ref i);
                SkipBlanks(ref i);
                isNegative = ScanSign(ref i);
                SkipBlanks(ref i);
                _operandRight = isNegative ? ScanNumber(ref i) * -1 : ScanNumber(ref i);
            }
        }
        private char ScanOp(ref int i)
        {
            char op = char.MinValue;
            switch (ExpressionText[i])
            {
                case '+':
                    {
                        op = '+';
                        break;
                    }
                case '-':
                    {
                        op = '-';
                        break;
                    }
                case '*':
                    {
                        op = '*';
                        break;
                    }
                case '/':
                    {
                        op = '/';
                        break;
                    }
                default:
                    {
                        throw new Exception($"Operator {ExpressionText[i]} ist fehlerhaft!");
                    }
            }
            if (i + 1 < ExpressionText.Length)
            {
                i++;
            }
            else
            {
                //throw new ArgumentException("");
            }
            return op;
        }
        /// <summary>
        /// Eine Ganzzahl muss mit einer Ziffer beginnen.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        private int ScanInteger(ref int pos, ref ArgumentException ex)
        {
            int result = 0;
            int counter = -10;
            int i = pos;
            if (!Char.IsDigit(ExpressionText[pos]))
            {
                if (ExpressionText[pos] == ',')
                {
                    ex = new ArgumentException("Ganzzahlanteil ist fehlerhaft");
                }
            }
            while (i < ExpressionText.Length && Char.IsDigit(ExpressionText[i]))
            {
                counter += 10;
                i++;
            }

            if (counter == 0)
                counter++;

            while (pos < ExpressionText.Length && Char.IsDigit(ExpressionText[pos]))
            {
                result += (ExpressionText[pos] - '0') * counter;
                counter -= 10;
                pos++;
            }

            return result;
        }
        /// <summary>
        /// Setzt die Position weiter, wenn Leerzeichen vorhanden sind
        /// </summary>
        /// <param name="pos"></param>
        private void SkipBlanks(ref int pos)
        {
            while (pos < ExpressionText.Length && ExpressionText[pos] == ' ')
            {
                pos++;
            }
        }
        /// <summary>
        /// Exceptionmessage samt Innerexception-Texten ausgeben
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string GetExceptionTextWithInnerExceptions(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            if (ex.InnerException == null)
            {
                sb.AppendLine(ex.Message);
            }
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
                sb.AppendLine(ex.Message);
            }
            return sb.ToString();
        }
        private bool ScanSign(ref int i)
        {
            if (ExpressionText[i] == '-')
            {
                i++;
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Ein Double muss mit einer Ziffer beginnen. Gibt es Nachkommastellen,
        /// müssen auch diese mit einer Ziffer beginnen.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        private double ScanNumber(ref int pos)
        {
            double result = 0;
            ArgumentException ex = null;
            bool isException = true;
            if (pos + 1 < ExpressionText.Length)
            {
                int leftInt = ScanInteger(ref pos, ref ex);
                if (ex == null)
                {
                    isException = false;
                    if (ExpressionText[pos] == ',')
                    {
                        pos++;
                        int rightInt = ScanInteger(ref pos, ref ex);
                        result = leftInt + GetDoubleFromInt(rightInt);
                    }
                    else
                    {
                        result = leftInt;
                    }
                }
            }
            if (isException)
            {
                if (ExpressionText[pos] == _op)
                {
                    throw new ArgumentException("Rechter Operand ist fehlerhaft", ex);
                }
                else
                {
                    throw new ArgumentException("Linker Operand ist fehlerhaft", ex);
                }
            }
            return result;
        }
        private double GetDoubleFromInt(int rightInt)
        {
            int counter = -10;
            int tmp = rightInt;

            while (tmp > 0)
            {
                tmp /= 10;
                counter += 10;
            }

            return counter == 0 ? (double)rightInt / 10 : (double)rightInt / counter;
        }
    }
}
