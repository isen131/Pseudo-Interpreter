using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter_Library
{
    public class Operations
    {
        public enum PartType { Number, Identifier, Sign, Parenthesis };     // Создаем перечисление типов частей
        public struct Part              // Создание структуры "Часть"
        {
            public string Value;        // Значение
            public PartType Type;       // Тип
            public int Position;        // Позиция
        }
        public struct Operation         // Структура "Операция"
        {
            public char Sign;           // Знак
            public int Precedence;      // Приоритет
        }
        public struct Identifier                // Структура "Идентификатор"
        {
            public string IdentifierString;     // Имя идентификатора
            public double Value;                // Числовое значение идентификатора
        }
        public static List<Part> Parts;                 // Список частей
        public static Stack<double> ArgStack;           // Стек аргументов
        public static Stack<Operation> OpStack;         // Стек операций
        public static List<Identifier> Identifiers;     // Список идентификаторов
        public static string Input;                     // Переменная для ввода выражения с клавиатуры
        public const long MaxLength = 100;              // Максимально допустимая длина вводимой строки     
        static char LastPart = ' ';                     // Предыдущая часть

        public static void Split(string S)              // Метод для разделения строки на лексемы
        {
            // Проверка по длине строки
            if (S.Length > MaxLength)
                throw new Exception("Слишком длинная строка");
            Parts = new List<Part>();
            Part NewPart;
            int Index = 0;
            double R = 0;
            // Цикл по всем символам в строке для разделения на части и формирования списка "Parts"
            while (Index < S.Length)
            {
                switch (S[Index])
                {
                    case ' ':
                        Index++;
                        break;
                    case '+':
                    case '-':
                    case '*':
                    case '/':
                    case '^':
                        if (S[Index] == '-' && LastPart == '-')
                            throw new Exception("Более одного унарного минуса подряд недопустимо");
                        LastPart = S[Index];
                        NewPart.Value = S[Index].ToString();
                        NewPart.Type = PartType.Sign;
                        NewPart.Position = Index + 1;
                        Parts.Add(NewPart);
                        Index++;
                        break;
                    case '(':
                    case ')':
                        LastPart = S[Index];
                        NewPart.Value = S[Index].ToString();
                        NewPart.Type = PartType.Parenthesis;
                        NewPart.Position = Index + 1;
                        Parts.Add(NewPart);
                        Index++;
                        break;

                    default:
                        if (S[Index] > 64 && S[Index] < 91 || S[Index] > 96 && S[Index] < 123 || S[Index] == '_')
                        {
                            NewPart.Value = "";
                            NewPart.Type = PartType.Identifier;
                            NewPart.Position = Index + 1;
                            while (Char.IsUpper(S[Index]) || Char.IsLower(S[Index]) || S[Index] == '_' || Char.IsNumber(S[Index]))
                            {
                                NewPart.Value = NewPart.Value + S[Index];
                                Index++;
                                if (Index == S.Length)
                                    break;
                            }
                            Index--;
                            Parts.Add(NewPart);
                        }
                        else
                            if (Char.IsNumber(S[Index]) || S[Index] == ',')
                        {
                            NewPart.Value = "";
                            NewPart.Type = PartType.Number;
                            NewPart.Position = Index + 1;
                            while (Char.IsNumber(S[Index]) || S[Index] == ',')
                            {
                                NewPart.Value = NewPart.Value + S[Index].ToString();
                                Index++;
                                if (Index == S.Length)
                                    break;
                            }
                            Index--;
                            if (!double.TryParse(NewPart.Value, out R))
                                throw new Exception("Неверное число: позиция " + NewPart.Position.ToString());
                            Parts.Add(NewPart);
                        }
                        else
                            throw new Exception("Недопустимый символ. Позиция: " + (Index + 1).ToString());
                        Index++;
                        break;
                }
            }
            // Проверка строки на пустоту
            if (Parts.Count == 0)
                throw new Exception("Список лексем пуст");
        }

        public static void Reduce(int minPrecedence)
        {
            if (OpStack.Count == 0)
                return;
            Operation CurrentOperation = OpStack.Peek();
            if (CurrentOperation.Precedence >= minPrecedence)
            {
                double Arg1, Arg2, Res = 0.0;
                CurrentOperation = OpStack.Pop();
                try
                {
                    switch (CurrentOperation.Sign)
                    {
                        case '+':
                            Arg1 = ArgStack.Pop();
                            Arg2 = ArgStack.Pop();
                            Res = Arg2 + Arg1;
                            ArgStack.Push(Res);
                            break;
                        case '-':
                            Arg1 = ArgStack.Pop();
                            Arg2 = ArgStack.Pop();
                            Res = Arg2 - Arg1;
                            ArgStack.Push(Res);
                            break;
                        case '*':
                            Arg1 = ArgStack.Pop();
                            Arg2 = ArgStack.Pop();
                            Res = Arg2 * Arg1;
                            ArgStack.Push(Res);
                            break;
                        case '/':
                            Arg1 = ArgStack.Pop();
                            Arg2 = ArgStack.Pop();
                            Res = Arg2 / Arg1;
                            ArgStack.Push(Res);
                            break;
                        case '~':
                            Arg1 = ArgStack.Pop();
                            Res = -Arg1;
                            ArgStack.Push(Res);
                            break;
                        case '^':
                            Arg1 = ArgStack.Pop();
                            Arg2 = ArgStack.Pop();
                            Res = Math.Pow(Arg2, Arg1);
                            ArgStack.Push(Res);
                            break;
                    }
                }
                catch
                {
                    throw new Exception("Проверьте правильность введенного выражения");
                }
                if (double.IsNaN(Res))
                    throw new Exception("Неопределённый результат операции");
                else
                    if (double.IsInfinity(Res))
                    throw new Exception("Переполнение");
                Reduce(minPrecedence);
            }
        }

        public static double Evaluate(ArgTable argTable)
        {
            ArgStack = new Stack<double>();
            OpStack = new Stack<Operation>();
            int depth = 0;
            bool needValue = true;
            int n = 0;

            Identifiers = new List<Identifier>();
            Identifier ID;
            ID.IdentifierString = "";
            ID.Value = 0;
            bool IsNew = true;

            //Console.WriteLine("Семантический анализ: ");
            foreach (Part CurrentPart in Parts)
            {
                n++;
                switch (CurrentPart.Type)
                {
                    case PartType.Number:
                        if (!needValue)
                            throw new Exception("Неуместное число: (" + CurrentPart.Value.ToString() + "): позиция "
                                                                      + CurrentPart.Position.ToString());
                        ArgStack.Push(double.Parse(CurrentPart.Value));
                        needValue = false;
                        break;
                    case PartType.Parenthesis:
                        if (CurrentPart.Value[0] == '(')
                            if (!needValue)
                                throw new Exception("Неуместная открывающая скобка: позиция " + CurrentPart.Position.ToString());
                            else
                                depth++;
                        else
                            if (depth == 0 || needValue)
                            throw new Exception("Неуместная закрывающая скобка: позиция " + CurrentPart.Position.ToString());
                        else
                            depth--;
                        break;
                    case PartType.Sign:
                        Operation CurrentOperation;
                        if (needValue)
                            if (CurrentPart.Value[0] == '-')
                            {
                                CurrentOperation.Sign = '~';
                                CurrentOperation.Precedence = depth * 10 + 1;
                                OpStack.Push(CurrentOperation);
                            }
                            else
                                throw new Exception("Неуместный знак: (" + CurrentPart.Value.ToString() + "): позиция "
                                                                               + CurrentPart.Position.ToString());
                        else
                        {
                            CurrentOperation.Sign = CurrentPart.Value[0];
                            CurrentOperation.Precedence = 0;
                            switch (CurrentPart.Value[0])
                            {
                                case '+':
                                    CurrentOperation.Precedence = depth * 10 + 1;
                                    break;
                                case '-':
                                    CurrentOperation.Precedence = depth * 10 + 1;
                                    break;
                                case '*':
                                    CurrentOperation.Precedence = depth * 10 + 2;
                                    break;
                                case '/':
                                    CurrentOperation.Precedence = depth * 10 + 2;
                                    break;
                                case '^':
                                    CurrentOperation.Precedence = depth * 10 + 3;
                                    break;
                            }
                            Reduce(CurrentOperation.Precedence);
                            OpStack.Push(CurrentOperation);
                            needValue = true;
                        }

                        break;
                    case PartType.Identifier:
                        if (!needValue)
                            throw new Exception("Неуместный идиентификатор: (" + CurrentPart.Value.ToString() +
                                "): позиция " + CurrentPart.Position.ToString());
                        foreach (Identifier CurrentIdentifier in Identifiers)
                        {
                            if ((CurrentPart.Value).ToLower() == (CurrentIdentifier.IdentifierString).ToLower())
                            {
                                ArgStack.Push(CurrentIdentifier.Value);
                                IsNew = false;
                            }
                            else
                                IsNew = true;
                            if (IsNew == false)
                                break;
                        }

                        if (IsNew == true)
                        {
                            ID.IdentifierString = CurrentPart.Value;
                            for (int i = 0; i < argTable.args.Count; i++)
                            {
                                if (ID.IdentifierString == argTable.args[i])
                                    ID.Value = argTable.values[i];
                            }
                            ArgStack.Push(ID.Value);
                            Identifiers.Add(ID);
                        }
                        needValue = false;
                        break;
                }
                // Вывод списка лексем с текущими состояниями стеков аргументов и операций
                //Console.WriteLine("__________________");
                //Console.WriteLine("Обработка лексемы № {0:d}", n);
                //Console.WriteLine("Стек аргументов: ");
                //foreach (double Arg in ArgStack)
                //    Console.WriteLine("{0:g}", Arg.ToString());
                //Console.WriteLine("Стек операций:");
                //foreach (Operation Op in OpStack)
                //    Console.WriteLine("{0:g} ({1:d})", Op.Sign.ToString(), Op.Precedence.ToString());
            }
            Reduce(0);
            if (depth > 0)
            {
                throw new Exception("Не закрыты скобки в конце строки");
            }
            return ArgStack.FirstOrDefault();
        }

        public static double evaluate(string s, ArgTable argTable)
        {
            try
            {
                Split(s);
            }
            catch (Exception E)
            {
                Console.WriteLine("Ошибка разбиения: " + E.Message);
                Console.Read();
            }
            Console.WriteLine();
            double Result = 0.0;
            try
            {
                Result = Evaluate(argTable);
            }
            catch (Exception E)
            {
                Console.Write("ОШИБКА ВЫЧИСЛЕНИЯ: " + E.Message);
                Console.Read();
            }
            return Result;
        }
    }
}
