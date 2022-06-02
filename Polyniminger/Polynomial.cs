using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Polyniminger
{
    struct Polynomial
    {
        // полином как сумма одночленов
        private Monomial[] Monomials
        {
            get 
            {
                return monomials;
            }
            set
            {
                if (value != null)
                    monomials = value;
                else
                    monomials = new Monomial[] { new Monomial(this.Vars) };
            }
        }

        private Monomial[] monomials;
        public Monomial C
        {
            get 
            {
                return Monomials[0]; 
            }
            set { Monomials[0] = value; }
        }
        public Monomial MinorMonomial
        {
            get { return Monomials[TermNum - 1]; }
            set { Monomials[TermNum - 1] = value; }
        }
        public Polynomial M 
        {
            get
            {
                Polynomial ans = new Polynomial(Vars);
                ans.Monomials = new Monomial[TermNum - 1];
                for (int i = 1; i < TermNum; i++)
                {
                    ans.SetMonom(i - 1, this.GetMonom(i));
                }
                return ans;
            }
        }
        public int Power
        {
            get
            {
                int power = 0;
                for(int i = 0; i < Monomials.Length; i++)
                {
                    power += Monomials[i].Power;
                }
                return power;
            }
        }
        public int TermNum
        {
            get { return Monomials.Length; }
        }
        public string[] Vars;


        public Polynomial(params Monomial[] monomials)
        {
            if (monomials.Length > 0) this.monomials = monomials;
            else throw new Exception("Forbidden to create Polynomial without Monomials");
            Vars = monomials[0].Vars;
            this.SortMonomials();
        }
        public Polynomial(bool printPolynomial = false, params Monomial[] monomials)
        {
            if (monomials.Length > 0) this.monomials = monomials;
            else throw new Exception("Forbidden to create Polynomial without Monomials");
            Vars = monomials[0].Vars;
            this.SortMonomials();
            if (printPolynomial)
                Console.WriteLine(this.GetPolynomial());
        }
        public Polynomial(string[] vars)
        {
            monomials = new Monomial[] { new Monomial(vars) };
            Vars = vars;
            this.SortMonomials();
        }

        public static implicit operator Polynomial(Monomial a)
        {
            return new Polynomial(a);
        }

        public static Polynomial operator +(Polynomial a, Polynomial b)
        {
            if(a.TermNum >= b.TermNum)
            {
                for(int i = 0; i < b.TermNum; i++)
                {
                    a += b.GetMonom(i);
                }
                a.SortMonomials();
                return a;
            }
            else
            {
                for (int i = 0; i < a.TermNum; i++)
                {
                    b += a.GetMonom(i);
                }
                b.SortMonomials();
                return b;
            }

        }

        public static Polynomial operator +(Polynomial a, Monomial b)
        {
            if (b.scalar == 0)
                return a;

            for (int i = 0; i < a.TermNum; i++)
            {
                if (a.Monomials[i].powers.SequenceEqual(b.powers))
                {
                    a.SetMonom(i, (Monomial)(a.GetMonom(i) + b));
                    return a;
                }
            }

            a.Add(b);
            a.SortMonomials();
            return a;
        }

        public static Polynomial operator -(Polynomial a, Polynomial b)
        {
            b = -b;
            if (a.TermNum >= b.TermNum)
            {
                for (int i = 0; i < b.TermNum; i++)
                {
                    a += b.GetMonom(i);
                }
                a.SortMonomials();
                return a;
            }
            else
            {
                for (int i = 0; i < a.TermNum; i++)
                {
                    b += a.GetMonom(i);
                }
                b.SortMonomials();
                return b;
            }

        }

        public static Polynomial operator -(Polynomial a)
        {
            for (int i = 0; i < a.TermNum; i++)
                a.SetMonom(i, -a.GetMonom(i));

            a.SortMonomials();
            return a;
        }

        public static Polynomial operator +(Polynomial a, float c)
        {
            Monomial b = new Monomial(a.Vars, c);
            for (int i = 0; i < a.TermNum; i++)
            {
                if (a.Monomials[i].powers == b.powers)
                {
                    a.SetMonom(i, (Monomial)(a.GetMonom(i) + b));
                    return a;
                }
            }

            a.Add(b);
            a.SortMonomials();
            return a;
        }
        public void Add(Monomial a)
        {
            Monomial[] t = new Monomial[this.Monomials.Length+1];
            for (int i = 0; i < t.Length-1; i++)
                t[i] = this.Monomials[i];
            t[this.Monomials.Length] = a;

            this.Monomials = t;
            //this.SortMonomials();
        }

        public void SortMonomials()
        {
            bool isSort;
            do
            {
                isSort = true;
                for(int i = 0; i < this.Monomials.Length-1; i++)
                {
                    if(this.Monomials[i] < this.Monomials[i+1])
                    {
                        isSort = false;
                        var t = this.Monomials[i];
                        this.Monomials[i] = this.Monomials[i + 1];
                        this.Monomials[i + 1] = t;
                    }
                }
            }
            while (!isSort);
            if(this.Monomials.Length == 0)
                Monomials = new Monomial[] { new Monomial(this.Vars) };
        }

        public static Polynomial operator *(Polynomial a, Polynomial b)
        {
            Polynomial ans = new Polynomial(new Monomial(a.Vars));
            for(int i = 0; i < a.TermNum; i++)
            {
                for (int j = 0; j < b.TermNum; j++) 
                {
                    ans += a.GetMonom(i) * b.GetMonom(j);
                    //Console.WriteLine($"({i} {j}) {ans.GetPolynomial()}");
                }
            }

            ans.SortMonomials();
            return ans;
        }
        public static Polynomial operator *(Polynomial a, Monomial b)
        {
            Polynomial ans = new Polynomial(new Monomial(a.Vars));
            for (int i = 0; i < a.TermNum; i++)
            {
                    ans += a.GetMonom(i) * b;
                    //Console.WriteLine($"({i}) {ans.GetPolynomial()}");
            }

            ans.SortMonomials();
            return ans;
        }
        public static Polynomial operator *(Polynomial a, float b)
        {
            for(int i = 0; i < a.TermNum; i++)
                a.Monomials[i] *= b;

            a.SortMonomials();
            return a;
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public static bool operator ==(Polynomial a, Polynomial b)
        {
            if ((a.Vars == b.Vars) && (a.Monomials == b.Monomials))
                return true;
            else
                return false;
        }
        public static bool operator !=(Polynomial a, Polynomial b)
        {
            if ((a.Vars != b.Vars) || (a.Monomials != b.Monomials))
                return true;
            else
                return false;
        }

        public string GetPolynomial()
        {
            string answer = this.C.GetMonomial();
            if (this.C.isConst)
                return this.C.scalar.ToString();
            for (int i = 1; i < this.TermNum; i++)
                if (GetMonom(i).scalar != 0)
                    answer += ((GetMonom(i).scalar>0)?'+':'-') + this.GetMonom(i).GetMonomial();
            return answer;
        }
        public static string GetPolynomial(Polynomial a)
        {
            string answer = a.C.GetMonomial();
            for (int i = 1; i < a.TermNum; i++)
                if (a.GetMonom(i).scalar != 0)
                    answer += "+" + a.GetMonom(i).GetMonomial();
            return answer;
        }            

        public static Polynomial Reducing(Polynomial a, Polynomial b, ref string latex)
        {
            // проверяем зацеп
            // исходим из предположения, что a можео редуцировать по b
            if (!(a.C % b.C))
                throw new Exception("Monomial a is not be divided by b");

            var t = a.C / b.C;
            t.scalar = a.C.scalar;

            var s = b.C.scalar;
            b *= t;
            a *= s;
            latex += a.GetLaTeXView(@"f_{00}=", "x", "y", "z");
            latex += b.GetLaTeXView(@"f_{11}=", "x", "y", "z");
            return a - b;
        }

        public Monomial GetMonom(int i) => this.Monomials[i];
        public void SetMonom(int k, Monomial a)
        {
            if(a.scalar == 0)
            {
                Monomial[] t = new Monomial[this.TermNum - 1];
                for(int i = 0; i < k; i++)
                    t[i] = this.Monomials[i];
                for (int i = k; i < this.TermNum-1; i++)
                    t[i] = this.Monomials[i+1];

                this.Monomials = t;
            }   
            else
                this.Monomials[k] = a;
        }
        public static Polynomial Abs(Polynomial a)
        {
            if (a.C.scalar < 0)
                return -a;
            else
                return a;
        }
        public string GetLaTeXView(string name, params string[] vars)
        {
            string ans = name + this.GetMonom(0).GetLaTeXView(true, false, vars);
            for (int i = 1; i < this.TermNum; i++)
                if (this.GetMonom(i).scalar != 0)
                    ans += ((GetMonom(i).scalar > 0) ? '+' : '-') + this.GetMonom(i).GetLaTeXView(false, false, vars);

            return ans;
        }

        public Polynomial Normalazing()
        {
            // возвращает многочлен, у которого коэффициенты - целые числа
            var ans = this;
            //if (ans.TermNum > 1)
            //{
            //    for (int i = 0; i < ans.TermNum; i++)
            //    {

            //    }
            //}
            //else if(ans.TermNum == 1)
            //{
            //    ans.Monomials[0].scalar = 1;
            //    return ans;
            //}
            if (ans.TermNum == 1) ans.Monomials[0].scalar = 1;
            if (ans.Monomials[0].scalar < 0)
                ans = -ans;
            return ans;
        }

        private static int GCD(int a, int b)
        {
            while (a != 0 && b != 0)
            {
                if (a > b)
                    a %= b;
                else
                    b %= a;
            }
            return a | b;
        }
    }
}
