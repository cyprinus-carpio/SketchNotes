using System.Collections.Generic;

namespace SketchNotes.ChemicalEquationBalancer
{
    class Compound
    {
        public int Coefficient { get; set; }
        public string Term { get; private set; }
        public int Position { get; private set; }
        public List<ElementTerm> Elements { get; private set; }

        public Compound(string term, int Position)
        {
            Elements = new List<ElementTerm>();
            this.Term = term;
            this.Position = Position;
        }

        public override string ToString()
        {
            string coefficient = "";
            if (Coefficient != 1)
            {
                coefficient = Coefficient.ToString();
            }
            return coefficient + Term;
        }
    }
}
