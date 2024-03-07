using System.Collections.Generic;
using System.Linq;
using static UnityEngine.Random;

public class EquationSystem
{
    public int[] GeneratedPassword()
    {
        var numbs = new int[6];
        do
        {
            for (int i = 0; i < numbs.Length; i++)
                numbs[i] = Range(0, 10);
        }
        while (Enumerable.Range(0, 9).Any(a => numbs.Count(b => b == a) >= 3));
        return numbs;
    }

    public int Equation(int ix, int a, int b)
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

    public WizardExpression[] GeneratedPuzzle(int[] password, int[] randomIxes)
    {
    tryagain:

        var expressions = new List<WizardExpression>();

        int[] ixes;
        var allPrev = new List<int[]>();

        for (int i = 0; i < 6; i++)
        {

            var letters = "ABCDEF";

            while (true)
            {
                ixes = Enumerable.Range(0, 6).ToList().Shuffle().Take(2).ToArray();

                var ixesToCheck = new[] { randomIxes[i], ixes[0], ixes[1] };
                /* A slew of checks to determine if the expression generated:
                * - uses the exact same combination of variables and operations as its previous
                * - shares the same value as another value.
                * - follows special rules denoting the operations.
                */
                if (CheckCase(randomIxes[i], password[ixes[0]], password[ixes[1]]) &&
                    (expressions.Count() == 0 ||
                    !allPrev.Any(a => ixesToCheck.SequenceEqual(a) ||
                    (ixesToCheck[0] == a[0] && ixesToCheck[1] == a[2] && ixesToCheck[2] == a[1]) ||
                    (Equation(ixesToCheck[0], password[ixesToCheck[1]], password[ixesToCheck[2]]) == Equation(a[0], password[a[1]], password[a[2]])))
                    ))
                {
                    allPrev.Add(ixesToCheck);
                    break;
                }

            }

            expressions.Add(new WizardExpression(letters[ixes[0]], "+,-,*,/,||".Split(',')[randomIxes[i]], letters[ixes[1]], Equation(randomIxes[i], password[ixes[0]], password[ixes[1]])));

        }

        if ("ABCDEF".Any(x => expressions.All(y => x != y.NumIxA && x != y.NumIxB)))
            goto tryagain;

             
        return expressions.ToArray();
    }

    private bool CheckCase(int ix, int a, int b)
    {
        switch (ix)
        {
            case 0:
                return Enumerable.Range(0, 18).Any(x => (a + b) == x);
            case 1:
                return Enumerable.Range(-9, 9).Any(x => (a - b) == x);
            case 2:
            case 3:
                if (new[] { a, b }.Any(x => x == 0))
                    return false;

                return a % b == 0;
            case 4:
                return Enumerable.Range(0, 99).Any(x => int.Parse($"{a}{b}") == x);

        }

        return false;
    }

 

}

public class WizardExpression
{
    public char NumIxA { get; private set; }
    public string EquationExpression { get; private set; }
    public char NumIxB { get; private set; }
    public int Answer { get; private set; }

    public WizardExpression(char numIxA, string equationExpression, char numIxB, int answer)
    {
        NumIxA = numIxA;
        EquationExpression = equationExpression;
        NumIxB = numIxB;
        Answer = answer;
    }


}