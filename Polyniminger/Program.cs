using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using WpfMath;

namespace Polyniminger
{
    class Program
    {
        static void Main(string[] args)
        {
            string latex = @"\text{Решение системы с помощью базиса грёбнера}\\\text{Исходная система:}";

            // нахождение Базиса Грёбнера из готовой системы (справа нули, а слева многочлены степени 1 и больше) от n переменных
            List<Polynomial> system = new List<Polynomial>();

            string[] fileContent = File.ReadAllLines("sourceSystem.txt");
            string[] vars = fileContent[1].Split(' ');
            string[][] nums = new string[fileContent.Length - 3][];
            for (int i = 3; i < fileContent.Length; i++)
                nums[i - 3] = fileContent[i].Split(' ');

            int varNum = vars.Length;
            int polNum = fileContent.Length - 3;
            for (int i = 0; i < polNum;i++)
            {
                var temp = new List<Monomial>();
                for (int j = 0; j < nums[i].Length; j += varNum + 1)
                {
                    var temp1 = new int[varNum];
                    for (int k = j+1; k < j+varNum + 1; k++)
                        temp1[k-1-j] = Convert.ToInt32(nums[i][k]);
                    temp.Add(new Monomial(varNum, Convert.ToInt32(nums[i][j]), temp1));
                }
                system.Add(new Polynomial(true, temp.ToArray()));
            }

            latex += @"\\ \cases{";
            for (int i = 0; i < system.Count; i++)
                latex += system[i].GetLaTeXView("f_" + (i + 1).ToString()+"=", vars) + @"=0\\";
            latex += @"}";

            for (int i = 0; i < system.Count-1; i++)
            {
                for (int j = i + 1; j < system.Count; j++)
                {
                    Monomial gcd = Monomial.GCD(system[i].C, system[j].C);
                    if(!gcd.isConst)
                    {
                        Polynomial F = (system[j].C / gcd) * system[i] - (system[i].C / gcd) * system[j];
                        latex += @$"\\\text{{({i + 1}, {j + 1}): }}" + F.GetLaTeXView(@$"F_{{{i + 1},{j + 1}}}=", vars);
                        bool isNew = false;
                        while((F.C.scalar != 0) && !isNew)
                        {
                            isNew = true;
                            foreach(Polynomial item in system)
                            {
                                if (F.C % item.C)
                                {
                                    F = Polynomial.Reducing(F, item);
                                    latex +=F.GetLaTeXView(@$"^{{({item.GetLaTeXView("","x", "y", "z")})}}=", vars);
                                    isNew = false;
                                    break;
                                }
                            }
                        }
                        Console.WriteLine($"({i+1}, {j+1}): {F.GetPolynomial()}");
                        if (F.C.scalar != 0)
                        {
                            system.Add(Polynomial.Abs(F));
                            latex += @"=\color{orange}{f_" + system.Count+"}";
                            i = 0;
                            if (F.C.isConst)
                            {
                                Console.WriteLine("Got Const.\nSystem is not be resolve");
                                latex += @"\\\text{В процессе решения после редуцирования была полученная константа. Значит система несовместна.}";
                                // создаём изображение в LaTeX
                                string _fileName = @"resolveGrebner.png";
                                var _parser = new TexFormulaParser();
                                var _formula = _parser.Parse(latex);
                                var _pngBytes = _formula.RenderToPng(30.0, 0.0, 0.0, "Arial");
                                File.WriteAllBytes(_fileName, _pngBytes);
                                if (File.Exists(_fileName))
                                {
                                    Console.WriteLine("Result File Sucsess Created");
                                    try
                                    {
                                        Process.Start(_fileName);
                                    }
                                    catch
                                    {
                                        Console.WriteLine("Не удалось открыть изображение с решением. Сохранено в " + _fileName);
                                    }
                                }
                                return;
                            }
                        }
                    }
                    else
                    {
                        latex += @$"\\\text{{({i + 1}, {j + 1}): Нет зацепа}}";
                        Console.WriteLine($"({i+1}, {j+1}): Нет зацепа");
                    }
                }
            }

            Console.WriteLine("Итоговый базис: ");
            for (int i = 0; i < system.Count; i++)
                Console.WriteLine($"f{i} = {system[i].GetPolynomial()}");

            latex += @"\\\text{Решив систему с помощью базиса Грёбнера получили систему:}\\ \cases{";
            for (int i = 0; i < system.Count; i++)
                latex += system[i].GetLaTeXView("f_" + (i + 1).ToString()+"=", vars) + @"=0\\";
            latex += @"}";

            // надо его минимизировать
            for (int i = 0; i < system.Count; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (system[j].C % system[i].C)
                    {
                        system.RemoveAt(j);
                        i--;
                    }
                }
                for (int j = i+1; j < system.Count; j++)
                {
                    if (system[j].C % system[i].C)
                        system.RemoveAt(j);
                }
            }
            latex += @"\\\text{Минимизированная система:}\\ \cases{";
            for (int i = 0; i < system.Count; i++)
                latex += system[i].GetLaTeXView("f_" + (i + 1).ToString()+"=", vars) + @"=0\\";
            latex += @"}";

            // и редуцировать
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
                latex += system[i].GetLaTeXView("f_" + (i + 1).ToString()+"=", vars) + @"=0\\";
            latex += @"}";


            // создаём изображение в LaTeX
            string fileName = @"resolveGrebner.png";
            var parser = new TexFormulaParser();
            var formula = parser.Parse(latex);
            var pngBytes = formula.RenderToPng(30.0, 0.0, 0.0, "Arial");
            File.WriteAllBytes(fileName, pngBytes);
            if (File.Exists(fileName))
            {
                Console.WriteLine("Result File Sucsess Created");
                try
                {
                    Process.Start(fileName);
                }
                catch
                {
                    Console.WriteLine("Не удалось открыть изображение с решением. Сохранено в " + fileName);
                }
            }
        }
    }
}
