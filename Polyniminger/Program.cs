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

            //system.Add(new Polynomial(true, new Monomial(3, 1f, 2), new Monomial(3, -1f)));
            //system.Add(new Polynomial(true, new Monomial(3, 1f, 1, 1), new Monomial(3, -1f, 0, 1)));
            //system.Add(new Polynomial(true, new Monomial(3, 1f, 1, 0, 1), new Monomial(3, 1f, 0, 0, 1)));

            //system.Add(new Polynomial(true, new Monomial(3, 1f, 2), new Monomial(3, 1f, 0,2), new Monomial(3, 1f, 0,0,2)));
            //system.Add(new Polynomial(true, new Monomial(3, 1f, 1), new Monomial(3, 1f, 0, 1), new Monomial(3, -1f, 0, 0, 1)));
            //system.Add(new Polynomial(true, new Monomial(3, 1f, 0, 1), new Monomial(3, 1f, 0, 0, 2)));

            system.Add(new Polynomial(true, new Monomial(3, 1f, 1, 1), new Monomial(3, -2f, 0, 1), new Monomial(3, 1f)));
            system.Add(new Polynomial(true, new Monomial(3, 1f, 0, 1,1), new Monomial(3, 1f, 0,0, 1), new Monomial(3, -1f)));
            system.Add(new Polynomial(true, new Monomial(3, 1f, 0, 1,1), new Monomial(3, 1f, 1,1, 1), new Monomial(3, 1f, 0,0,1)));

            latex += @"\\ \cases{";
            for (int i = 0; i < system.Count; i++)
                latex += system[i].GetLaTeXView("f_" + (i + 1).ToString(), "x", "y", "z") + @"=0\\";
            latex += @"}";

            for (int i = 0; i < system.Count-1; i++)
            {
                for (int j = i + 1; j < system.Count; j++)
                {
                    Monomial gcd = Monomial.GCD(system[i].C, system[j].C);
                    if(!gcd.isConst)
                    {
                        Polynomial F = (system[j].C / gcd) * system[i] - (system[i].C / gcd) * system[j];
                        latex += @$"\\\text{{({i + 1}, {j + 1}): }}" + F.GetLaTeXView(@$"F_{{{i + 1},{j + 1}}}", "x", "y", "z");
                        bool isNew = false;
                        while((F.C.scalar != 0) && !isNew)
                        {
                            isNew = true;
                            foreach(Polynomial item in system)
                            {
                                if (F.C % item.C)
                                {
                                    F = Polynomial.Reducing(F, item);
                                    latex +=F.GetLaTeXView(@$"(^{{{item.C.GetLaTeXView(false,false, "x", "y", "z")}}})", "x", "y", "z");
                                    isNew = false;
                                }
                            }
                        }
                        Console.WriteLine($"({i+1}, {j+1}): {F.GetPolynomial()}");
                        if (F.C.scalar != 0)
                        {
                            system.Add(Polynomial.Abs(F));
                            latex += "=f_" + system.Count;
                            i = 0;
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
                latex += system[i].GetLaTeXView("f_" + (i + 1).ToString(), "x", "y", "z") + @"=0\\";
            latex += @"}";

            // надо его минимизировать
            for (int i = 0; i < system.Count - 1; i++)
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
                latex += system[i].GetLaTeXView("f_" + (i + 1).ToString(), "x", "y", "z") + @"=0\\";
            latex += @"}";
            // и редуцировать
            for (int i = 0; i < system.Count - 1; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    for(int k = 1; k < system[j].TermNum; k++)
                    {
                        if(system[j].GetMonom(k) % system[i].C)
                        {
                            system[j] = Polynomial.Reducing(system[j], system[i]);
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
                            system[j] = Polynomial.Reducing(system[j], system[i]);
                            break;
                        }
                    }
                }
            }
            latex += @"\\\text{Минимизированная и редуцированная система:}\\ \cases{";
            for (int i = 0; i < system.Count; i++)
                latex += system[i].GetLaTeXView("f_" + (i + 1).ToString(), "x", "y", "z") + @"=0\\";
            latex += @"}";


            // создаём изображение в LaTeX
            const string fileName = @"resolveGrebner.png";
            var parser = new TexFormulaParser();
            var formula = parser.Parse(latex);
            var pngBytes = formula.RenderToPng(30.0, 0.0, 0.0, "Arial");
            File.WriteAllBytes(fileName, pngBytes);
            if (File.Exists(fileName))
            {
                Console.WriteLine("Resolve File Sucsess Created");
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
