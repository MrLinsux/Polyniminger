using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Polyniminger
{
    struct Polynomial
    {
        // полином как сумма одночленов
        private Monomial[] monomials;
        public Monomial C
        {
            get 
            {
                return monomials[0]; 
            }
            set { monomials[0] = value; }
        }
        public Monomial MinorMonomial
        {
            get { return monomials[TermNum - 1]; }
            set { monomials[TermNum - 1] = value; }
        }
        public Polynomial M 
        {
            get
            {
                Polynomial ans = new Polynomial(Vars);
                ans.monomials = new Monomial[TermNum - 1];
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
                for(int i = 0; i < monomials.Length; i++)
                {
                    power += monomials[i].Power;
                }
                return power;
            }
        }
        public int TermNum
        {
            get { return monomials.Length; }
        }
        public int VarNumber;
        public string[] Vars
        {
            get { return this.C.Vars; }
        }


        public Polynomial(params Monomial[] monomials)
        {
            this.monomials = monomials;
            VarNumber = monomials[0].VarNumber;
            this.SortMonomials();
        }
        public Polynomial(bool printPolynomial = false, params Monomial[] monomials)
        {
            this.monomials = monomials;
            VarNumber = monomials[0].VarNumber;
            this.SortMonomials();
            if (printPolynomial)
                Console.WriteLine(this.GetPolynomial());
        }
        public Polynomial(string[] vars)
        {
            this.monomials = new Monomial[] { new Monomial(vars) };
            VarNumber = monomials[0].VarNumber;
            this.SortMonomials();
        }
        public Polynomial(Monomial monomial)
        {
            monomials = new Monomial[] { monomial };
            VarNumber = monomials[0].VarNumber;
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
                if (a.monomials[i].powers.SequenceEqual(b.powers))
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
                if (a.monomials[i].powers == b.powers)
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
            Monomial[] t = new Monomial[this.monomials.Length+1];
            for (int i = 0; i < t.Length-1; i++)
                t[i] = this.monomials[i];
            t[this.monomials.Length] = a;

            this.monomials = t;
            //this.SortMonomials();
        }

        public void SortMonomials()
        {
            bool isSort;
            do
            {
                isSort = true;
                for(int i = 0; i < this.monomials.Length-1; i++)
                {
                    if(this.monomials[i] < this.monomials[i+1])
                    {
                        isSort = false;
                        var t = this.monomials[i];
                        this.monomials[i] = this.monomials[i + 1];
                        this.monomials[i + 1] = t;
                    }
                }
            }
            while (!isSort);
            if(this.monomials.Length == 0)
                monomials = new Monomial[] { new Monomial(this.Vars) };
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
                a.monomials[i] *= b;

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
            if ((a.VarNumber == b.VarNumber) && (a.monomials == b.monomials))
                return true;
            else
                return false;
        }
        public static bool operator !=(Polynomial a, Polynomial b)
        {
            if ((a.VarNumber != b.VarNumber) || (a.monomials != b.monomials))
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

        public static Polynomial Reducing(Polynomial a, Polynomial b)
        {
            // проверяем зацеп
            // исходим из предположения, что a можео редуцировать по b
            b *= a.C / b.C;
            return a - b;
        }

        public Monomial GetMonom(int i) => this.monomials[i];
        public void SetMonom(int k, Monomial a)
        {
            if(a.scalar == 0)
            {
                Monomial[] t = new Monomial[this.TermNum - 1];
                for(int i = 0; i < k; i++)
                    t[i] = this.monomials[i];
                for (int i = k; i < this.TermNum-1; i++)
                    t[i] = this.monomials[i+1];

                this.monomials = t;
            }   
            else
                this.monomials[k] = a;
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
    }
}
