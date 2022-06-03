using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using WpfMath;
using ComplexNumber;

namespace Polyniminger
{
    class Program
    {
        const int maxLinesNumber = 41;      // число строк, после которого будет записан отдельный файл с решением
        static int fileNum = 0;                    // если файлов будет бельше чем 1, то так будем присваивать им имена с индеком 0 - один файл, иначе файлов несклько
        [STAThreadAttribute]
        static void Main(string[] args)
        {
            string latex = @"\text{Решение системы с помощью базиса грёбнера}\\\text{Исходная система:}";   // latex-заготовка для создания картинки

            // нахождение Базиса Грёбнера из готовой системы (справа нули, а слева многочлены степени 1 и больше) от n переменных
            List<Polynomial> system = new List<Polynomial>();   // система многочленов

            string sysNum = "";
            if (args.Length < 1)
            {
                Console.Write("Choose System Num: ");
                sysNum = Console.ReadLine();
            }
            else
            {
                sysNum = args[0];
            }

            Directory.CreateDirectory(sysNum);

            string systemFile = $"sourceSystem{sysNum}.txt";
            // тут обрабатываем файл со строками
            string[] fileContent;
            if (File.Exists(systemFile))
                fileContent = File.ReadAllLines(systemFile);
            else
            {
                // если файла нет, то создадим его с содержимым по умолчанию
                fileContent = "Переменные (в лексикографическом порядке)\nx y z\nМногочлены исходной системы - одночлены через пробел: скаляр степень переменной 1 степень переменной 2 ... степень переменной n:\n1 1 2 0 -1 0 0 1 -1 0 0 2\n1 2 1 0 -1 0 1 0\n1 0 2 0 -1 0 0 2".Split('\n');
                File.WriteAllLines(systemFile, fileContent);
            }
            string[] vars = fileContent[1].Split(' ');
            string[][] nums = new string[fileContent.Length - 3][];
            for (int i = 3; i < fileContent.Length; i++)
                nums[i - 3] = fileContent[i].Split(' ');

            int varNum = vars.Length;
            int polNum = fileContent.Length - 3;
            for (int i = 0; i < polNum; i++)
            {
                var temp = new List<Monomial>();
                for (int j = 0; j < nums[i].Length; j += varNum + 1)
                {
                    var temp1 = new int[varNum];
                    for (int k = j + 1; k < j + varNum + 1; k++)
                        temp1[k - 1 - j] = Convert.ToInt32(nums[i][k]);
                    temp.Add(new Monomial(vars, Convert.ToInt32(nums[i][j]), temp1));
                }
                system.Add(new Polynomial(true, temp.ToArray()));
            }

            // выводим систему
            latex += OutSystem(system.ToArray());
            string mainLatex = "";  // сохраняем промежуточный прогресс решения

            // цикл проходит по всей "лесенке" многочленов, включая новые уравнения
            // если появляется новый, но начинаем цикл с 
            for (int i = 0; i < system.Count - 1; i++)
            {
                int l = 0;
                for (int j = i + 1; j < system.Count; j++)
                {
                    // находим НОД одночленов 
                    // так проверяем зацеп и собираем начальный многочлен Fij, который будет редуцировать
                    Monomial gcd = Monomial.GCD(system[i].C, system[j].C); l = 0;
                    if (!gcd.isConst)
                    {
                        Polynomial F = (system[j].C / gcd) * system[i] - (system[i].C / gcd) * system[j];
                        latex += @$"\\\text{{({i + 1}, {j + 1}): }}" + F.GetLaTeXView(@$"F_{{{i + 1},{j + 1}}}=", vars);
                        bool isNew = false;
                        while ((F.C.scalar != 0) && !isNew)
                        {
                            // редуцируем многочлен Fij, пока есть на что
                            isNew = true;       // предполагаем, что Fij нельзя редуцировать ни по одному многочлену
                            foreach (Polynomial item in system)
                            {
                                // проходим по всем многочленам из системы
                                if (F.C % item.C)
                                {
                                    // если можем проредуцировать, делаем это
                                    F = Polynomial.Reducing(F, item, ref latex).Normalazing();
                                    Console.WriteLine(F.GetPolynomial()); l++;
                                    if (l >= 10)
                                    {
                                        if (latex.Split(@"\\").Length >= maxLinesNumber)
                                        {
                                            // если размер решения превысил максимальное число строк, то
                                            // сохраняем файл и начинаем записывать новый
                                            SaveImage(sysNum+@"\grebnerResult" + (++fileNum) + ".png", latex);
                                            mainLatex += latex;
                                            latex = @"\text{Часть решения " + (fileNum + 1) + "}";
                                        }
                                        latex += "\\\\";
                                        l = 0;
                                    }
                                    if (F.C.scalar >= int.MaxValue)
                                    {
                                        SaveImage(sysNum + @"\errorResolve.png", latex); throw new Exception("!!!");
                                    }

                                    latex += F.GetLaTeXView(@$"={{({item.GetLaTeXView("", "x", "y", "z")})}}=", vars);
                                    isNew = false;
                                    break;
                                }
                            }
                        }
                        Console.WriteLine($"({i + 1}, {j + 1}): {F.GetPolynomial()}");
                        if (F.C.scalar != 0)
                        {
                            // если проредуцировали до конца и это не 0, то сохраняем
                            if (F.C.isConst)
                            {
                                // если это оказалась константа, то система является несовместной, о чём и сообщается 
                                Console.WriteLine("Got Const.\nSystem is not be resolve");
                                latex += @"\\\text{В процессе решения после редуцирования была полученная константа. Значит система несовместна.}";
                                // создаём изображение в LaTeX
                                if(fileNum > 0)
                                    SaveImage(sysNum + @"\grebnerResult" + (++fileNum) + ".png", latex);
                                else
                                    SaveImage(sysNum + @"\resolveGrebner.png", latex);
                                return;
                            }
                            system.Add(F.Normalazing());
                            latex += @"=\color{orange}{f_" + system.Count + "}";
                            latex = CheckNewHook(i, j, latex, ref system, ref mainLatex, sysNum);          // проверяем, сохраняя результат в latex
                        }
                    }
                    else
                    {
                        // если НОД - константа, но зацепа нет
                        latex += @$"\\\text{{({i + 1}, {j + 1}): Нет зацепа}}";
                        Console.WriteLine($"({i + 1}, {j + 1}): Нет зацепа");
                    }

                    if(latex.Split(@"\\").Length >= maxLinesNumber)
                    {
                        // если размер решения превысил максимальное число строк, то
                        // сохраняем файл и начинаем записывать новый
                        SaveImage(sysNum + @"\grebnerResult" + (++fileNum)+".png", latex);
                        mainLatex += latex;
                        latex = @"\text{Часть решения " + (fileNum + 1) + "}";
                    }
                }
            }

            Console.WriteLine("Итоговый базис: ");
            for (int i = 0; i < system.Count; i++)
                Console.WriteLine($"f{i} = {system[i].GetPolynomial()}");

            latex += @"\\\text{Решив систему с помощью базиса Грёбнера получили систему:}\\ \cases{";
            for (int i = 0; i < system.Count; i++)
                latex += system[i].GetLaTeXView("f_" + (i + 1).ToString() + "=", vars) + @"=0\\";
            latex += @"}";

            // надо его минимизировать
            Console.WriteLine("Minimization...");
            for (int i = 0; i < system.Count; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (system[j].C % system[i].C)
                    {
                        system.RemoveAt(j--);
                        i--;
                    }
                }
                for (int j = i + 1; j < system.Count; j++)
                {
                    if (system[j].C % system[i].C)
                    {
                        system.RemoveAt(j--);
                    }

                }
            }
            latex += @"\\\text{Минимизированная система:}\\ \cases{";
            for (int i = 0; i < system.Count; i++)
                latex += system[i].GetLaTeXView("f_" + (i + 1).ToString() + "=", vars) + @"=0\\";
            latex += @"}";
            Console.WriteLine("Ok");

            // и редуцировать
            Console.WriteLine("Reducing...");
            bool isReduced;
            do
            {
                isReduced = true;
                for (int i = 0; i < system.Count; i++)
                {
                    for (int j = 0; j < i; j++)
                    {
                        for (int k = 1; k < system[j].TermNum; k++)
                        {
                            if (system[j].GetMonom(k) % system[i].C)
                            {
                                system[j] = (system[i] * (system[j].GetMonom(k) / system[i].C)) - system[j];
                                isReduced = false;
                                break;
                            }
                        }
                    }
                    for (int j = i + 1; j < system.Count; j++)
                    {
                        for (int k = 1; k < system[j].TermNum; k++)
                        {
                            if (system[j].GetMonom(k) % system[i].C)
                            {
                                system[j] = (system[i] * (system[j].GetMonom(k) / system[i].C)) - system[j];
                                isReduced = false;
                                break;
                            }
                        }
                    }
                }
            }
            while (!isReduced);
            latex += @"\\\text{Минимизированная и редуцированная система:}\\ \cases{";
            for (int i = 0; i < system.Count; i++)
                latex += system[i].GetLaTeXView("f_" + (i + 1).ToString() + "=", vars) + @"=0\\";
            latex += @"}";
            for (int i = 0; i < system.Count; i++)
                Console.WriteLine(system[i].GetPolynomial());
            Console.WriteLine("Ok");

            // создаём изображение в LaTeX
            string path = sysNum + @"\resolveGrebner.png";
            if (fileNum > 0)
                path = sysNum + @"\grebnerResult" + (++fileNum) + ".png";
            // пробуем скопировать latex-строку в буфер обмена
            try
            {
                Clipboard.SetText(latex);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: Unable copy to clipboard\n" + e.Message);
            }

            SaveImage(path, latex);
        }

        static void SaveImage(string path, string latex)
        {
            // создаём файл из latex-строки
            latex = latex.Replace("∞", "?");
            latex = latex.Replace("не число", "?");
            latex = latex.Replace(" ", "?");
            try
            {
                var parser = new TexFormulaParser();
                var formula = parser.Parse(latex);
                var pngBytes = formula.RenderToPng(30.0, 0.0, 0.0, "Arial");
                Console.WriteLine("Save Image...");
                File.WriteAllBytes(path, pngBytes);
            }
            catch(Exception e)
            {
                Console.WriteLine("Error: Unable create file\n" + e.Message);
            }
        }

        static string OutSystem(Polynomial[] system)
        {
            // возвращает строку для latex
            var latex = @"\\ \cases{";
            for (int i = 0; i < system.Length; i++)
                latex += system[i].GetLaTeXView("f_{" + (i + 1).ToString() + "}=", system[0].Vars) + @"=0\\";
            latex += @"}";

            return latex;
        }

        static string CheckNewHook(int endI, int j, string latex, ref List<Polynomial> system, ref string mainLatex, string sysNum)
        {
            // рекурсивная функция, которая проверяет зацепы нового многочлена с уже пройдеными
            // если тут бедет найден новый многочлен, то запустится рекурсовная функция
            for (int i = 0; i < endI; i++)
            {
                // находим НОД одночленов 
                // так проверяем зацеп и собираем начальный многочлен Fij, который будет редуцировать
                Monomial gcd = Monomial.GCD(system[i].C, system[j].C);
                if (!gcd.isConst)
                {
                    Polynomial F = (system[j].C / gcd) * system[i] - (system[i].C / gcd) * system[j];
                    latex += @$"\\\text{{({i + 1}, {j + 1}): }}" + F.GetLaTeXView(@$"F_{{{i + 1},{j + 1}}}=", system[0].Vars);
                    bool isNew = false;
                    while ((F.C.scalar != 0) && !isNew)
                    {
                        // редуцируем многочлен Fij, пока есть на что
                        isNew = true;       // предполагаем, что Fij нельзя редуцировать ни по одному многочлену
                        foreach (Polynomial item in system)
                        {
                            // проходим по всем многочленам из системы
                            if (F.C % item.C)
                            {
                                // если можем проредуцировать, делаем это
                                F = Polynomial.Reducing(F, item, ref latex).Normalazing();
                                latex += F.GetLaTeXView(@$"={{({item.GetLaTeXView("", "x", "y", "z")})}}=", system[0].Vars);
                                isNew = false;
                                break;
                            }
                        }
                    }
                    Console.WriteLine($"({i + 1}, {j + 1}): {F.GetPolynomial()}");
                    if (F.C.scalar != 0)
                    {
                        // если проредуцировали до конца и это не 0, то сохраняем
                        if (F.C.isConst)
                        {
                            // если это оказалась константа, то система является несовместной, о чём и сообщается 
                            Console.WriteLine("Got Const.\nSystem is not be resolve");
                            latex += @"\\\text{В процессе решения после редуцирования была полученная константа. Значит система несовместна.}";
                            // создаём изображение в LaTeX
                            if (fileNum > 0)
                                SaveImage(sysNum + @"\grebnerResult" + (++fileNum) + ".png", latex);
                            else
                                SaveImage(sysNum + @"\resolveGrebner.png", latex);
                            Environment.Exit(0);
                        }
                        system.Add(F.Normalazing());
                        latex += @"=\color{orange}{f_" + system.Count + "}";
                        latex = CheckNewHook(i, j+1, latex, ref system, ref mainLatex, sysNum);          // проверяем, сохраняя результат в latex
                    }
                }
                else
                {
                    // если НОД - константа, но зацепа нет
                    latex += @$"\\\text{{({i + 1}, {j + 1}): Нет зацепа}}";
                    Console.WriteLine($"({i + 1}, {j + 1}): Нет зацепа");
                }

                if (latex.Split(@"\\").Length >= maxLinesNumber)
                {
                    // если размер решения превысил максимальное число строк, то
                    // сохраняем файл и начинаем записывать новый
                    SaveImage(sysNum + @"\grebnerResult" + (++fileNum), latex);
                    mainLatex += latex;
                    latex = @"\text{Часть решения " + (fileNum + 1) + "}";
                }
            }

            return latex;
        }
    }
}
