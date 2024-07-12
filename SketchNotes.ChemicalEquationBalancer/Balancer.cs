using System;
using System.Collections.Generic;
using System.Linq;

namespace SketchNotes.ChemicalEquationBalancer
{
    public class Balancer
    {
        //static void Main(string[] args)
        //{
        //    Console.WriteLine(BalanceChemicalEquation("KMnO4 + HCl -> KCl + MnCl2 + Cl2 + H2O"));
        //    Console.ReadLine();
        //}

        public static string BalanceChemicalEquation(string equation)
        {
            int middle = equation.IndexOf('-');
            string leftSide = equation.Substring(0, middle - 1);
            string rightSide = equation.Substring(middle + 3, equation.Length - middle - 3);

            Compound[] leftSideCompounds = FindCompounds(leftSide);
            Compound[] rightSideCompounds = FindCompounds(rightSide);

            ElementTerm[] leftSideElements = FindElements(leftSideCompounds);
            ElementTerm[] rightSideElements = FindElements(rightSideCompounds);

            string[] uniqueElements = FindUniqueElements(leftSide, rightSide);

            if (uniqueElements == null)
            {
                return null;
            }

            //Excluding the furthest right column of 0's, not sure if will make difference
            double[,] equationMatrix = new double[uniqueElements.Length, leftSideCompounds.Count() + rightSideCompounds.Count()];

            for (int y = 0; y < equationMatrix.GetLength(0); y++)
            {
                for (int x = 0; x < equationMatrix.GetLength(1); x++)
                {
                    if (x < leftSideCompounds.Count())
                    {
                        if (leftSideCompounds[x].Elements.Contains(new ElementTerm(uniqueElements[y])))
                        {
                            equationMatrix[y, x] = leftSideCompounds[x].Elements.Find(i => i.Element == uniqueElements[y]).Coefficient;
                        }
                    }
                    else
                    {
                        if (rightSideCompounds[x - leftSideCompounds.Count()].Elements.Contains(new ElementTerm(uniqueElements[y])))
                        {
                            equationMatrix[y, x] = -rightSideCompounds[x - leftSideCompounds.Count()].Elements.Find(i => i.Element == uniqueElements[y]).Coefficient;
                        }
                    }
                }
            }

            ReducedRowEchelonForm(equationMatrix);
            Console.WriteLine(ToString(equationMatrix));
            List<Fraction> fractions = new List<Fraction>();

            for (int counter = 0; counter < equationMatrix.GetLength(0); counter++)
            {
                if (IsRowZero(equationMatrix, counter))
                {
                    fractions.Add(new Fraction(0, 1));
                }
                else
                {
                    fractions.Add(new Fraction(equationMatrix[counter, equationMatrix.GetLength(1) - 1]));
                }
            }

            List<int> denominators = new List<int>();

            for (int counter = 0; counter < fractions.Count; counter++)
            {
                denominators.Add(fractions[counter].Denominator);
            }

            int LeastCommonDenominator = LCM(denominators);

            for (int counter = 0; counter < fractions.Count; counter++)
            {
                if (counter < leftSideCompounds.Length)
                {
                    leftSideCompounds[counter].Coefficient = Math.Abs(fractions[counter].Numerator * (LeastCommonDenominator / fractions[counter].Denominator));
                }
                else
                {
                    rightSideCompounds[counter - leftSideCompounds.Length].Coefficient = Math.Abs(fractions[counter].Numerator * (LeastCommonDenominator / fractions[counter].Denominator));
                }
            }
            if (equationMatrix.GetLength(0) == equationMatrix.GetLength(1) && LeastCommonDenominator == 1)
            {
                rightSideCompounds[rightSideCompounds.Length - 1].Coefficient = 0;
            }
            else
            {
                rightSideCompounds[rightSideCompounds.Length - 1].Coefficient = LeastCommonDenominator;
            }

            //return fractions[1].ToString();
            return string.Join(" + ", (Object[])leftSideCompounds) + " -> " + string.Join(" + ", (Object[])rightSideCompounds);
            //return ToString(equationMatrix);
        }

        static int LCM(List<int> numbers)
        {
            int result = 1;

            foreach (int number in numbers)
            {
                result = LCM(result, number);
            }

            return result;
        }

        static int LCM(int num1, int num2)
        {
            int x = num1;
            int y = num2;

            while (num1 != num2)
            {
                if (num1 > num2)
                {
                    num1 -= num2;
                }
                else
                {
                    num2 -= num1;
                }
            }

            return (x * y) / num1;
        }

        static string[] FindUniqueElements(string leftSide, string rightSide)
        {
            List<string> leftSideElements = new List<string>();

            for (int counter = 0; counter < leftSide.Length; counter++)
            {
                if (char.IsUpper(leftSide, counter))
                {
                    if (counter + 1 < leftSide.Length && char.IsLower(leftSide, counter + 1))
                    {
                        leftSideElements.Add(leftSide.Substring(counter, 2));
                    }
                    else
                    {
                        leftSideElements.Add(leftSide.Substring(counter, 1));
                    }
                }
            }

            leftSideElements = leftSideElements.Distinct().ToList();

            List<string> rightSideElements = new List<string>();

            for (int counter = 0; counter < rightSide.Length; counter++)
            {
                if (char.IsUpper(rightSide, counter))
                {
                    if (counter + 1 < rightSide.Length && char.IsLower(rightSide, counter + 1))
                    {
                        rightSideElements.Add(rightSide.Substring(counter, 2));
                    }
                    else
                    {
                        rightSideElements.Add(rightSide.Substring(counter, 1));
                    }
                }
            }

            rightSideElements = rightSideElements.Distinct().ToList();

            if (rightSideElements.OrderBy(i => i).SequenceEqual(leftSideElements.OrderBy(i => i)))
            {
                return leftSideElements.ToArray();
            }
            else
            {
                return null;
            }
        }

        //Returns true if the whole row is zero except for the last element.
        static bool IsRowZero(double[,] matrix, int row)
        {
            bool result = true;

            for (int counter = 0; counter < matrix.GetLength(1) - 1; counter++)
            {
                if (matrix[row, counter] != 0)
                {
                    result = false;
                }
            }

            return result;
        }

        static Compound[] FindCompounds(string equation)
        {
            string[] terms = equation.Split('+');
            List<Compound> termList = new List<Compound>();

            for (int counter = 0; counter < terms.Length; counter++)
            {
                termList.Add(new Compound(terms[counter].Trim(), counter));
            }

            return termList.ToArray();
        }

        static ElementTerm[] FindElements(Compound[] terms)
        {
            // I actually do not know how this works or how I wrote it, but it works so.
            // Let's figure out how this works.

            // Create a list of elements that will have the elements added to it as we pass through the terms.
            List<ElementTerm> elements = new List<ElementTerm>();

            // Loop through each Term.
            for (int termPosition = 0; termPosition < terms.Length; termPosition++)
            {
                string term = terms[termPosition].Term;

                // Loop through each character of the string of the Term.
                for (int counter = 0; counter < term.Length; counter++)
                {
                    // If the character is uppercase, marks start of a new element.
                    if (char.IsUpper(term, counter))
                    {
                        // Check if next character is also a letter, then it is a 2 letter element.
                        // Ensure that the next letter is still within the limits of the string.
                        if (counter + 1 < term.Length && char.IsLower(term, counter + 1))
                        {
                            // Calculate the coefficient of the element by starting with a 0.
                            string coefficient = "0";
                            int position = counter + 2;
                            
                            // While the next character is a number and haven't hit end of string yet.
                            while (position < term.Length && char.IsNumber(term[position]))
                            {
                                // Add each digit onto the coefficient string.
                                coefficient += term[position];
                                // Move onto the next character.
                                position++;
                            }

                            // Add a new element with the calculated coefficient, element symbol and use the position of the Term as the position for the element.
                            if (coefficient == "0")
                            {
                                coefficient = "1";
                            }

                            ElementTerm elementAdd = new ElementTerm(int.Parse(coefficient), term.Substring(counter, 2), termPosition);
                            elements.Add(elementAdd);
                            terms[termPosition].Elements.Add(elementAdd);
                            // Make the next character to check, the position after all the numbers.
                            // Need to decrease by one since for loop will increment counter by 1.
                            counter = position - 1;
                        }
                        // else the element is only one letter long.
                        else
                        {
                            // Calculate the coefficient of the element by starting with a 0.
                            string coefficient = "0";
                            int position = counter + 1;
                            // While the next character is a number and haven't hit end of string yet.
                            while (position < term.Length && char.IsNumber(term[position]))
                            {
                                // Add each digit onto the coefficient string.
                                coefficient += term[position];
                                // Move onto the next character.
                                position++;
                            }

                            if (coefficient == "0")
                            {
                                coefficient = "1";
                            }

                            // Add a new element with the calculated coefficient, element symbol and use the position of the Term as the position for the element.
                            ElementTerm elementAdd = new ElementTerm(int.Parse(coefficient), term.Substring(counter, 1), termPosition);
                            elements.Add(elementAdd);
                            terms[termPosition].Elements.Add(elementAdd);
                            // Make the next character to check, the position after all the numbers.
                            // Need to decrease by one since for loop will increment counter by 1.
                            counter = position - 1;
                        }
                    }
                }
            }

            //Return the array of elements.
            return elements.ToArray();
        }

        static List<string> SplitIntoTerms(string equation)
        {
            List<string> terms = new List<string>();
            string nextTerm = "";

            for (int counter = 0; counter < equation.Length; counter++)
            {
                if (equation[counter] == '+')
                {
                    terms.Add(nextTerm.Trim());
                    nextTerm = "";
                }
                else
                {
                    nextTerm += equation[counter];
                }
            }

            terms.Add(nextTerm.Trim());

            return terms;
        }

        static double[,] ReducedRowEchelonForm(double[,] matrix)
        {
            int lead = 0, rowCount = matrix.GetLength(0), columnCount = matrix.GetLength(1);

            for (int r = 0; r < rowCount; r++)
            {
                if (columnCount <= lead)
                {
                    break;
                }

                int i = r;

                while (matrix[i, lead] == 0)
                {
                    i++;

                    if (i == rowCount)
                    {
                        i = r;
                        lead++;

                        if (columnCount == lead)
                        {
                            lead--;

                            break;
                        }
                    }
                }

                for (int j = 0; j < columnCount; j++)
                {
                    (matrix[i, j], matrix[r, j]) = (matrix[r, j], matrix[i, j]);
                }

                double div = matrix[r, lead];

                if (div != 0)
                {
                    for (int j = 0; j < columnCount; j++)
                    {
                        matrix[r, j] /= div;
                    }
                }

                for (int j = 0; j < rowCount; j++)
                {
                    if (j != r)
                    {
                        double sub = matrix[j, lead];

                        for (int k = 0; k < columnCount; k++)
                        {
                            matrix[j, k] -= (sub * matrix[r, k]);
                        }
                    }
                }

                lead++;
            }

            return matrix;
        }

        static string ToString(double[,] grid)
        {
            string result = "";

            for (int y = 0; y < grid.GetLength(0); y++)
            {
                for (int x = 0; x < grid.GetLength(1); x++)
                {
                    result += grid[y, x].ToString();

                    if (x != grid.GetLength(1) - 1)
                    {
                        result += ",";
                    }
                }

                result += "\n";
            }

            return result;
        }
    }
}
