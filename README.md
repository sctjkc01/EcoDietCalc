# EcoDietCalc
Diet calculator for the game Eco

# Files required in executable directory:
## masterdict.csv
A CSV of all of the foods that are in your game, arranged as:
`Name, Calories, Carbs, Protein, Fats, Vitamins`

All foods available in a vanilla game are available as masterdict.vanilla.csv on the root of this repo.

## stomach.txt
A list of all foods you have in your stomach already.  One line per food, written like `5 Fiddlehead` or `10 Campfire Beans`.  The name should match a value in the masterdict.csv file.

## available.txt
A list of foods that you have available to consume, formatted like stomach.txt.  This file may be ignored if you opt to ignore availability during runtime.