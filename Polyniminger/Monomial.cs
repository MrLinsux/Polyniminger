using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Polyniminger
{
    struct Monomial
    {
        public int[] variables;        // состоит из степеней всех переменных
        public float scalar;           // коэффициент перед одночленом
        public int Power
        {
            // степень одночлена, т.е. сумма степеней каждой переменной
            get
            {
                int power = 0;
                for (int i = 0; i < variables.Length; i++)
                    power += variables[i];
                return power;
            }
        }
        public int VarNumber
        {
            get { return variables.Length; }
        }
        public bool isConst
        {
            get
            {
                for (int i = 0; i < VarNumber; i++)
                    if (variables[i] != 0)
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

        public Monomial(int n)
        {
            ///<summary>
            /// создаёт одночлен от n переменных из одного скаляра равного 0
            ///</summary>
            variables = new int[n];
            scalar = 0;
            for(int i = 0; i < n; i++)
                variables[i] = 0;
        }

        public Monomial(int n, float _scalar, params int[] powers)
        {
            ///<summary>
            /// создаёт одночлен от n переменных из скаляра и со всеми указаными степенями
            /// если степеней меньше, то остальные равны 0
            ///</summary>
            variables = new int[n];
            scalar = _scalar;
            for (int i = 0; i < powers.Length; i++)
                variables[i] = powers[i];
            for (int i = powers.Length; i < n; i++)
                variables[i] = 0;
        }

        public Monomial(int n, float _scalar)
        {
            ///<summary>
            /// создаёт одночлен от n переменных из одного скаляра
            ///</summary>
            variables = new int[n];
            scalar = _scalar;
            for (int i = 0; i < n; i++)
                variables[i] = 0;
        }
        public Monomial(int n, params int[] powers)
        {
            ///<summary>
            /// создаёт одночлен от n переменных из скаляра равного 1 и со всеми указаными степенями
            /// если степеней меньше, то остальные равны 0
            ///</summary>
            variables = new int[n];
            scalar = 1;
            for (int i = 0; i < powers.Length; i++)
                variables[i] = powers[i];
            for (int i = powers.Length - 1; i < n + 1; i++)
                variables[i] = 0;
        }

        public static explicit operator Monomial(Polynomial pol)
        {
            return pol.C;
        }
        public static Polynomial operator+(Monomial a, Monomial b)
        {
            if(a.variables.SequenceEqual(b.variables))
            {
                return new Monomial(a.VarNumber, a.scalar + b.scalar, a.variables);
            }
            else
            {
                return new Polynomial(new Monomial[] { Max(a, b), Min(a, b) });
            }
        }
        public static Polynomial operator -(Monomial a, Monomial c)
        {
            Monomial b = -c;
            if (a.variables == b.variables)
            {
                return new Monomial(a.VarNumber, a.scalar + b.scalar, a.variables);
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
            Monomial answer = new Monomial(a.VarNumber, a.scalar * b.scalar);
            for(int i = 0; i < a.VarNumber; i++)
                answer.variables[i] = a.variables[i] + b.variables[i];
            return answer;
        }
        public static Monomial operator *(Monomial a, float b)
        {
            return new Monomial(a.VarNumber, a.scalar * b, a.variables);
        }

        public static Monomial operator/(Monomial a, Monomial b)
        {
            Monomial ans = new Monomial(a.VarNumber);
            if ((b.scalar != 0) && (a % b))
            {
                ans.scalar = a.scalar / b.scalar;
                for (int i = 0; i < a.VarNumber; i++)
                {
                    ans.variables[i] = a.variables[i] - b.variables[i];
                }
            }

            return ans;
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public static bool operator ==(Monomial a, Monomial b)
        {
            if((a.scalar == b.scalar)&&(a.variables == b.variables))
                return true;
            else
                return false;
        }
        public static bool operator !=(Monomial a, Monomial b)
        {
            if ((a.scalar != b.scalar) || (a.variables != b.variables))
                return true;
            else
                return false;
        }
        public static bool operator >(Monomial a, Monomial b)
        {
            for (int i = 0; i < a.VarNumber; i++)
            {
                if (a.variables[i] < b.variables[i])
                    return false;
                else if (a.variables[i] > b.variables[i])
                    return true;
            }
            return false;
        }
        public static bool operator <(Monomial a, Monomial b)
        {
            for (int i = 0; i < a.VarNumber; i++)
            {
                if (a.variables[i] > b.variables[i])
                    return false;
                else if (a.variables[i] < b.variables[i])
                    return true;
            }
            return false;
        }
        public static bool operator % (Monomial a, Monomial b)
        {
            // делится ли один одночлен, на другой
            for(int i = 0; i < a.VarNumber; i++)
            {
                if (a.variables[i] - b.variables[i] < 0)
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
            Monomial ans = new Monomial(a.VarNumber);

            // сначала коэффициент
            while (b.scalar != a.scalar)
            {
                if (b.scalar > a.scalar)
                    b.scalar -= a.scalar;
                else
                    a.scalar -= b.scalar;
            }

            ans.scalar = a.scalar;
            // а потом и степеней переменных
            for(int i = 0; i < a.VarNumber; i++)
            {
                ans.variables[i] = Math.Min(a.variables[i], b.variables[i]);
            }

            return ans;
        }

        public static Monomial RandMonomial(int n, int min, int max)
        {
            Random rand = new Random();
            Monomial answer = new Monomial(n);
            answer.scalar = rand.Next(min, max);
            for (int i = 0; i < n; i++)
                answer.variables[i] = rand.Next(min, max);
            return answer;
        }
        public string GetMonomial(bool withSign = false)
        {
            return (withSign ? this.scalar : Math.Abs(this.scalar)) + "*(" + String.Join(", ", this.variables) + ")";
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
                    if (variables[i] != 0)
                        ans += vars[i] + "^" + variables[i];
                    else;
            else
            {
                if (this.isConst)
                    return this.Abs.ToString();
                else
                for (int i = 0; i < this.VarNumber; i++)
                    if (variables[i] != 0)
                        ans += vars[i] + (variables[i] == 1 ? "" : ("^" + variables[i]));
            }

            return ans;
        }

    }
}
