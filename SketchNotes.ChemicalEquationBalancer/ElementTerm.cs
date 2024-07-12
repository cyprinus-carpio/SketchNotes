using System;
using System.Collections.Generic;

namespace SketchNotes.ChemicalEquationBalancer
{
    internal class ElementTerm
    {
        public int Coefficient { get; private set; }
        public string Element { get; private set; }
        public int Position { get; private set; }

        public ElementTerm(int Coefficient, string Element, int Position)
        {
            this.Coefficient = Coefficient;
            this.Element = Element;
            this.Position = Position;
        }

        public ElementTerm(string Element)
        {
            this.Element = Element;
        }

        public override string ToString()
        {
            string coefficient = "";
            if (Coefficient != 1)
            {
                coefficient = Coefficient.ToString();
            }

            return Element + coefficient;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            //If parameter cannot be cast to Rectangle return false.
            ElementTerm elementTerm = (ElementTerm)obj;
            if ((Object)elementTerm == null)
                return false;

            //Return true if the two rectangles match.
            return Element == elementTerm.Element;
        }


        public bool Equals(ElementTerm elementTerm)
        {
            return Element == elementTerm.Element;
        }

        public override int GetHashCode()
        {
            int hashCode = 1159441131;
            hashCode = hashCode * -1521134295 + Coefficient.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Element);
            hashCode = hashCode * -1521134295 + Position.GetHashCode();
            return hashCode;
        }
    }
}
