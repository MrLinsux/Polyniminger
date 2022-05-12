using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Polyniminger
{
    struct Monomial
    {
        public int[] powers;        // состоит из степеней всех переменных
        public float scalar;           // коэффициент перед одночленом
        public string[] Vars;            // переменные, из которых состоит одночлен
        public int Power
        {
            // степень одночлена, т.е. сумма степеней каждой переменной
            get
            {
                int power = 0;
                for (int i = 0; i < powers.Length; i++)
                    power += powers[i];
                return power;
            }
        }
        public int VarNumber
        {
            get { return powers.Length; }
        }
        public bool isConst
        {
            get
            {
                for (int i = 0; i < VarNumber; i++)
                    if (powers[i] != 0)
                        return false;
                return true;
            }
        }
        public string Sign
        {
            get { return scalar >= 0 ? "+" : "-"; }
        }
        public float Abs
        {
            get { return Math.Abs(scalar); }
        }

        public Monomial(string[] vars)
        {
            ///<summary>
            /// создаёт одночлен от n переменных из одного скаляра равного 0
            ///</summary>
            int n = vars.Length;
            this.Vars = vars;
            powers = new int[n];
            scalar = 0;
            for(int i = 0; i < n; i++)
                powers[i] = 0;
        }

        public Monomial(string[] vars, float _scalar, params int[] powers)
        {
            ///<summary>
            /// создаёт одночлен от n переменных из скаляра и со всеми указаными степенями
            /// если степеней меньше, то остальные равны 0
            ///</summary>
            int n = vars.Length;
            this.Vars = vars;
            this.powers = new int[n];
            scalar = _scalar;
            for (int i = 0; i < powers.Length; i++)
                this.powers[i] = powers[i];
            for (int i = powers.Length; i < n; i++)
                this.powers[i] = 0;
        }

        public Monomial(string[] vars, float _scalar)
        {
            ///<summary>
            /// создаёт одночлен от n переменных из одного скаляра
            ///</summary>
            int n = vars.Length;
            this.Vars = vars;
            powers = new int[n];
            scalar = _scalar;
            for (int i = 0; i < n; i++)
                powers[i] = 0;
        }
        public Monomial(string[] vars, params int[] powers)
        {
            ///<summary>
            /// создаёт одночлен от n переменных из скаляра равного 1 и со всеми указаными степенями
            /// если степеней меньше, то остальные равны 0
            ///</summary>
            int n = vars.Length;
            this.Vars = vars;
            this.powers = new int[n];
            scalar = 1;
            for (int i = 0; i < powers.Length; i++)
                this.powers[i] = powers[i];
            for (int i = powers.Length - 1; i < n + 1; i++)
                this.powers[i] = 0;
        }

        public static explicit operator Monomial(Polynomial pol)
        {
            return pol.C;
        }
        public static Polynomial operator+(Monomial a, Monomial b)
        {
            if(a.powers.SequenceEqual(b.powers))
            {
                return new Monomial(a.Vars, a.scalar + b.scalar, a.powers);
            }
            else
            {
                return new Polynomial(new Monomial[] { Max(a, b), Min(a, b) });
            }
        }
        public static Polynomial operator -(Monomial a, Monomial c)
        {
            Monomial b = -c;
            if (a.powers == b.powers)
            {
                return new Monomial(a.Vars, a.scalar + b.scalar, a.powers);
            }
            else
            {
                return new Polynomial(new Monomial[] { Max(a, b), Min(a, b) });
            }
        }
        public static Monomial operator -(Monomial a)
        {
            return a * -1;
        }
        public static Monomial operator*(Monomial a, Monomial b)
        {
            Monomial answer = new Monomial(a.Vars, a.scalar * b.scalar);
            for(int i = 0; i < a.VarNumber; i++)
                answer.powers[i] = a.powers[i] + b.powers[i];
            return answer;
        }
        public static Monomial operator *(Monomial a, float b)
        {
            return new Monomial(a.Vars, a.scalar * b, a.powers);
        }

        public static Monomial operator/(Monomial a, Monomial b)
        {
            Monomial ans = new Monomial(a.Vars);
            if ((b.scalar != 0) && (a % b))
            {
                ans.scalar = a.scalar / b.scalar;
                for (int i = 0; i < a.VarNumber; i++)
                {
                    ans.powers[i] = a.powers[i] - b.powers[i];
                }
            }

            return ans;
        }
        public override bool Equals(object obj)
        {
            return ((this.scalar == ((Monomial)obj).scalar)&&(this.powers == ((Monomial)obj).powers));
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public static bool operator ==(Monomial a, Monomial b)
        {
            return a.powers == b.powers;
        }
        public static bool operator !=(Monomial a, Monomial b)
        {
            return a.powers != b.powers;
        }
        public static bool operator >(Monomial a, Monomial b)
        {
            for (int i = 0; i < a.VarNumber; i++)
            {
                if (a.powers[i] < b.powers[i])
                    return false;
                else if (a.powers[i] > b.powers[i])
                    return true;
            }
            return false;
        }
        public static bool operator <(Monomial a, Monomial b)
        {
            for (int i = 0; i < a.VarNumber; i++)
            {
                if (a.powers[i] > b.powers[i])
                    return false;
                else if (a.powers[i] < b.powers[i])
                    return true;
            }
            return false;
        }
        public static bool operator % (Monomial a, Monomial b)
        {
            // делится ли один одночлен, на другой
            for(int i = 0; i < a.VarNumber; i++)
            {
                if (a.powers[i] - b.powers[i] < 0)
                    return false;
            }
            return true;
        }
        public static Monomial Min(Monomial a, Monomial b)
        {
            if (a < b)
                return a;
            else
                return b;
        }
        public static Monomial Max(Monomial a, Monomial b)
        {
            if (a > b)
                return a;
            else
                return b;
        }
        public static Monomial GCD(Monomial a, Monomial b)
        {
            //TODO: подумать о реализации без поиска НОД скаляров
            // находим НОД одночленов
            Monomial ans = new Monomial(a.Vars);

            // сначала коэффициент
            /*while (b.scalar != a.scalar)
            {
                if (b.scalar > a.scalar)
                    b.scalar -= a.scalar;
                else
                    a.scalar -= b.scalar;
            }*/

            ans.scalar = 1;
            // а потом и степеней переменных
            for(int i = 0; i < a.VarNumber; i++)
            {
                ans.powers[i] = Math.Min(a.powers[i], b.powers[i]);
            }

            return ans;
        }

        public static Monomial RandMonomial(string[] vars, int min, int max)
        {
            int n = vars.Length;
            Random rand = new Random();
            Monomial answer = new Monomial(vars);
            answer.scalar = rand.Next(min, max);
            for (int i = 0; i < n; i++)
                answer.powers[i] = rand.Next(min, max);
            return answer;
        }
        public string GetMonomial(bool withSign = false)
        {
            return (withSign ? this.scalar : Math.Abs(this.scalar)) + "*(" + String.Join(", ", this.powers) + ")";
        }

        public string GetLaTeXView(bool withSign = false, bool with1 = false, params string[] vars)
        {
            string ans = "";
            if (withSign)
                ans = this.Sign == "+"?"":"-";
            if (!with1)
                if (this.Abs != 1)
                    ans += this.Abs;
                else;
            else
                ans += this.Abs;

            if (with1)
                for (int i = 0; i < this.VarNumber; i++)
                    if (powers[i] != 0)
                        ans += vars[i] + "^" + powers[i];
                    else;
            else
            {
                if (this.isConst)
                    return this.Abs.ToString();
                else
                for (int i = 0; i < this.VarNumber; i++)
                    if (powers[i] != 0)
                        ans += vars[i] + (powers[i] == 1 ? "" : ("^" + powers[i]));
            }

            return ans;
        }

    }
}
