using System.Linq;
using static UnityEngine.Random;

public class EquationSystem
{
    public int[] GeneratedPassword()
    {
        var numbs = new int[6];

        for (int i = 0; i < numbs.Length; i++)
            numbs[i] = Range(0, 10);

        return numbs;
    }

    private int Equation(int ix, int a, int b)
    {
        switch (ix)
        {
            case 0:
                return a + b;
            case 1:
                return a - b;
            case 2:
                return a * b;
            case 3:
                return a / b;
            case 4:
                return int.Parse($"{a}{b}");
        }

        return -1;
    }

    public Expression[] GeneratedPuzzle(int[] password, int[] randomIxes)
    {
        var expressions = new Expression[6];

        for (int i = 0; i < 6; i++)
        {
            int[] ixes;
            var letters = "ABCDEF";

            while (true)
            {
                ixes = Enumerable.Range(0, 6).ToList().Shuffle().Take(2).ToArray();

                if (CheckCase(randomIxes[i], password[ixes[0]], password[ixes[1]]))
                    break;
            }

            expressions[i] = new Expression(letters[ixes[0]], "+,-,*,/,||".Split(',')[randomIxes[i]], letters[ixes[1]], Equation(randomIxes[i], password[ixes[0]], password[ixes[1]]));

        }
             
        return expressions;
    }

    private bool CheckCase(int ix, int a, int b)
    {
        var multiplicationTable = Enumerable.Range(0, 9).Select(x => Enumerable.Range(0, 9).Select(y => x * y).ToArray()).ToArray();

        switch (ix)
        {
            case 0:
                return Enumerable.Range(0, 18).Any(x => (a + b) == x);
            case 1:
                return Enumerable.Range(-9, 9).Any(x => (a - b) == x);
            case 2:
                return multiplicationTable.Any(x => x.Any(y => a * b == y)); // Change it later in case we figured out how to check multiplication
            case 3:
                if (b == 0)
                    return false;

                return Enumerable.Range(0, 9).Any(x => a == x);
            case 4:
                return Enumerable.Range(0, 99).Any(x => int.Parse($"{a}{b}") == x);

        }

        return false;
    }

 

}

public class Expression
{
    public char NumIxA { get; private set; }
    public string EquationExpression { get; private set; }
    public char NumIxB { get; private set; }
    public int Answer { get; private set; }

    public Expression(char numIxA, string equationExpression, char numIxB, int answer)
    {
        NumIxA = numIxA;
        EquationExpression = equationExpression;
        NumIxB = numIxB;
        Answer = answer;
    }


}