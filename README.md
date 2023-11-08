**Minesweeper Solver**

I've developed a Minesweeper solver in C# that's tailored for quick and efficient game-solving. The core strategy of the program is to first make all the no-risk moves that can be directly inferred from the numbers on the game board. This involves identifying tiles that are definitely safe or are mines, without any guessing. However, when the game board presents situations where no-risk moves are not evident, it leverages a Monte Carlo algorithm. This algorithm intelligently simulates numerous potential arrangements of mines that comply with the adjacent numerical clues on the board. It considers only those configurations that are possible within the rules of Minesweeper.

Through these simulations, my solver estimates the most probable locations for mines when faced with ambiguity. This method allows me to combine a risk-free approach with a calculated probabilistic technique to solve Minesweeper puzzles very quickly and with a high level of success.

![Imgur](https://imgur.com/3396BTO.jpg)
