using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter_Library
{
    public class MainClass
    {
        public int n = 0;
        public string[] keyWords = { "program", "declaration", "evaluation", "output" };
        public List<string> allLexems = new List<string>();
        public string declarations;
        public string evaluations;
        public string outputs;
        public string[] singleDeclarations;
        public string[] singleEvaluations;
        public string[] singleOutputs;
        ArgTable argTable = new ArgTable();
        

        public List<string> getAllLexems(string code)
        {
            List<string> result = new List<string>();
            string currentString;
            for (int i = 0; i < code.Length; i++)
            {
                currentString = "";
                while (i < code.Length && !Char.IsWhiteSpace(code[i]) && code[i] != ';')
                {
                    currentString += code[i];
                    i++;
                }
                if (currentString != "")
                    result.Add(currentString);
                if (i < code.Length && code[i] == ';')
                    result.Add(";");
            }
            return result;
        }

        public bool isIdent(string s)
        {
            bool result = true;
            if (!char.IsLetter(s[0]))
            {
                result = false;
            }
            for (int i = 1; i < s.Length; i++)
            {
                if (char.IsLetter(s[i]) || s[i] == '_' || char.IsNumber(s[i]))
                {
                    result = true;
                }
                else
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        public void setDeclarations()
        {
            while (allLexems[n] != keyWords[2])
            {
                declarations += allLexems[n] + " ";
                n++;
            }
            n++;
        }

        public void setEvaluations()
        {
            while (allLexems[n] != keyWords[3] && allLexems[n] != "}")
            {
                evaluations += allLexems[n] + " ";
                n++;
            }
            n++;
        }

        public void setOutputs()
        {
            while (n < allLexems.Count && allLexems[n] != "}")
            {
                outputs += allLexems[n] + " ";
                n++;
            }
        }

        public void setArgTable()
        {
            double tmp = 0;
            string[] line;
            for (int i = 0; i < singleDeclarations.Length; i++)
            {
                line = singleDeclarations[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (line.Length == 2)
                {
                    if (isIdent(line[1]))
                    {
                        argTable.args.Add(line[1]);
                        argTable.values.Add(0);
                    }
                    else
                        if (!isIdent(line[1]))
                        throw new Exception("Имя переменной " + line[1] + " некорректно.");
                }
                else
                {
                    if (isIdent(line[1]) && double.TryParse(line[3], out tmp))
                    {
                        argTable.args.Add(line[1]);
                        argTable.values.Add(tmp);
                    }
                    else
                        if (!isIdent(line[1]))
                        throw new Exception("Имя переменной " + line[1] + " некорректно.");
                    else
                        if (!double.TryParse(line[3], out tmp))
                        throw new Exception("Значение переменной " + line[1] + " некорректно.");
                }
            }
        }

        public void printAll()
        {
            string[] line;
            for (int i = 0; i < singleOutputs.Length; i++)
            {
                line = singleOutputs[i].Split(new char[] { '(', ')' }, StringSplitOptions.None);
                if (line[0] == "print")
                {
                    for (int j = 0; j < argTable.args.Count; j++)
                    {
                        if (line[1] == argTable.args[j])
                            Console.Write(argTable.values[j]);
                    }
                }
                else
                if (line[0] == "println")
                {
                    for (int j = 0; j < argTable.args.Count; j++)
                    {
                        if (line[1] == argTable.args[j])
                            Console.WriteLine(argTable.values[j]);
                    }
                }
                else
                    throw new Exception("Команда " + line[0] + " отсутствует.");
            }
        }

        public void evaluateAll()
        {
            string[] line;
            for (int i = 0; i < singleEvaluations.Length; i++)
            {
                line = singleEvaluations[i].Split(new string[] { " = " }, StringSplitOptions.None);
                for (int j = 0; j < argTable.args.Count; j++)
                {
                    if (line[0] == argTable.args[j])
                    {
                        argTable.values[j] = Operations.evaluate(line[1], argTable);
                    }
                }
            }
        }

        public void CompileIt(string code)
        {
            allLexems = getAllLexems(code);
            if (allLexems[0] != keyWords[0])
            {
                throw new Exception("Нет ключевого слова начала программы");
            }
            string programName = allLexems[1];
            if (!(isIdent(programName)))
                throw new Exception("Некорректное название программы");
            while (allLexems[n] != keyWords[1])
                n++;
            n++;
            setDeclarations();
            setEvaluations();
            setOutputs();
            singleDeclarations = declarations.Split(new string[] { "; " }, StringSplitOptions.RemoveEmptyEntries);
            singleEvaluations = evaluations.Split(new string[] { "; " }, StringSplitOptions.RemoveEmptyEntries);
            singleOutputs = outputs.Split(new string[] { "; " }, StringSplitOptions.RemoveEmptyEntries);
            setArgTable();
            evaluateAll();
            printAll();
            Console.ReadKey();
        }
    }
}
