using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoDietMaker {
    class Program {
        public static float GetSPDelta(Food f, Dictionary<Food, int> StomachContents) {
            var nutSum = StomachContents.Sum(kvp => kvp.Key.SumNutrients * kvp.Key.Calories * kvp.Value);

            var carbSum = StomachContents.Sum(kvp => kvp.Key.Carbs * kvp.Key.Calories * kvp.Value);
            var protSum = StomachContents.Sum(kvp => kvp.Key.Protein * kvp.Key.Calories * kvp.Value);
            var fatsSum = StomachContents.Sum(kvp => kvp.Key.Fats * kvp.Key.Calories * kvp.Value);
            var vitsSum = StomachContents.Sum(kvp => kvp.Key.Vitamins * kvp.Key.Calories * kvp.Value);

            var mul = nutSum / (new[] { carbSum, protSum, fatsSum, vitsSum }.Max() * 4) * 2;
            if(float.IsNaN(mul)) mul = 1;

            var foodCount = StomachContents.Sum(kvp => kvp.Key.Calories * kvp.Value);

            var spBefore = 12 + (nutSum / foodCount) * mul;
            if(float.IsNaN(spBefore)) spBefore = 12; 

            nutSum += f.SumNutrients * f.Calories;
            carbSum += f.Carbs * f.Calories;
            protSum += f.Protein * f.Calories;
            fatsSum += f.Fats * f.Calories;
            vitsSum += f.Vitamins * f.Calories;

            mul = nutSum / (new[] { carbSum, protSum, fatsSum, vitsSum }.Max() * 4) * 2;
            foodCount += f.Calories;

            var spAfter = 12 + (nutSum / foodCount) * mul;

            return spAfter - spBefore;
        }

        public static float GetSPValue(Dictionary<Food, int> StomachContents) {
            var nutSum = StomachContents.Sum(kvp => kvp.Key.SumNutrients * kvp.Key.Calories * kvp.Value);

            var carbSum = StomachContents.Sum(kvp => kvp.Key.Carbs * kvp.Key.Calories * kvp.Value);
            var protSum = StomachContents.Sum(kvp => kvp.Key.Protein * kvp.Key.Calories * kvp.Value);
            var fatsSum = StomachContents.Sum(kvp => kvp.Key.Fats * kvp.Key.Calories * kvp.Value);
            var vitsSum = StomachContents.Sum(kvp => kvp.Key.Vitamins * kvp.Key.Calories * kvp.Value);

            var mul = nutSum / (new[] { carbSum, protSum, fatsSum, vitsSum }.Max() * 4) * 2;
            if(float.IsNaN(mul)) mul = 1;

            var foodCount = StomachContents.Sum(kvp => kvp.Key.Calories * kvp.Value);

            var rtn = 12 + (nutSum / foodCount) * mul;

            return float.IsNaN(rtn) ? 12 : rtn;

        }

        static void Main(string[] args) {
            var dict = File.ReadAllLines("masterdict.csv").Skip(1).Where(s => !string.IsNullOrEmpty(s)).Select(s => new Food(s)).ToArray();
            var stomach = File.ReadAllLines("stomach.txt").Where(s => !string.IsNullOrEmpty(s)).Select(s => s.Split(' ')).ToDictionary(l => dict.First(f => f.Name.Equals(string.Join(" ", l.Skip(1)))), l => int.Parse(l[0]));
            var avail = File.ReadAllLines("available.txt").Where(s => !string.IsNullOrEmpty(s)).Select(s => s.Split(' ')).ToDictionary(l => dict.First(f => f.Name.Equals(string.Join(" ", l.Skip(1)))), l => int.Parse(l[0]));

            Console.Write("Ignore availability counts? [y/N]>");
            while(true)
            {
                var input = Console.ReadLine();
                if(string.IsNullOrEmpty(input))
                {
                    break;
                }
                if(input.Equals("y", StringComparison.CurrentCultureIgnoreCase))
                {
                    avail = dict.ToDictionary(f => f, f => 10000);
                    break;
                }
                if(input.Equals("n", StringComparison.CurrentCultureIgnoreCase))
                {
                    break;
                }
                Console.Write("Invalid option. Ignore availability counts? [y/N]>");
            }

            Console.Write("How many calories can you consume? >");
            var cal = -1;
            while(cal == -1)
            {
                try
                {
                    cal = int.Parse(Console.ReadLine());
                } catch(FormatException)
                {
                    Console.Write("That doesn't seem to be a number, try again. How many calories? >");
                }
            }

            Console.WriteLine();
            Console.WriteLine("You want to eat the following foods:");

            while(true)
            {
                var canEat = avail.Where(kvp => kvp.Value > 0).Select(kvp => kvp.Key).Where(f => (cal - f.Calories) > 0).ToArray();
                if(canEat.Length == 0)
                    break;

                var foodDeltas = canEat.ToDictionary(f => f, f => GetSPDelta(f, stomach)).OrderByDescending(kvp => kvp.Value).ToArray();
                var food = canEat.OrderByDescending(f => GetSPDelta(f, stomach)).First();
                var spBefore = GetSPValue(stomach);
                cal -= food.Calories;
                if(stomach.ContainsKey(food))
                    stomach[food]++;
                else
                    stomach[food] = 1;
                var spAfter = GetSPValue(stomach);
                Console.WriteLine($"  - {food.Name} for {food.Calories} ( {spBefore:0.000}SP/d -> {spAfter:0.000}SP/d || {spAfter-spBefore:0.000} delta )");
            }
        }
    }

    struct Food {
        public string Name { get; }

        public int Carbs { get; }
        public int Protein { get; }
        public int Fats { get; }
        public int Vitamins { get; }

        public float SumNutrients => Carbs + Protein + Fats + Vitamins;

        public int Calories { get; }

        public Food(string ln) : this(ln.Split(',')) {}

        public Food(string[] lnparts) {
            Name = lnparts[0];
            Calories = int.Parse(lnparts[1]);
            Carbs = int.Parse(lnparts[2]);
            Protein = int.Parse(lnparts[3]);
            Fats = int.Parse(lnparts[4]);
            Vitamins = int.Parse(lnparts[5]);
        }

        public bool Equals(Food f) {
            return Name.Equals(f.Name) && Carbs == f.Carbs && Protein == f.Protein && Fats == f.Fats && Vitamins == f.Vitamins && Calories == f.Calories;
        }

        public override string ToString() => $"{Name}; {SumNutrients} Nutrients, {Calories} Cal";
    }
}
