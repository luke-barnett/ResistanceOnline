Feature: GameRounds
	In order to make the game continue
	As the game runner
	I want to make sure that rounds start effectively

Scenario Outline: When everyone votes for the quest then the quest goes ahead
	Given there is a standard game of 5 players
	And the game is in the <RoundNumber> round
	And the leader has chosen his crew
	When everyone approves the quest
	Then the quest goes ahead

	Examples:
	| RoundNumber |
	| first       |
	| second      |
	| third       |
	| fourth      |
	| fifth       |

Scenario Outline: A quest only goes ahead if there is a majority vote for it
	Given there is a standard game of <NumberOfPlayers> players
	And the leader has chosen his crew
	When <NumberOfPlayersApprovingQuest> players approve the quest
	Then the quest goes ahead

	Examples: 
	| NumberOfPlayers | NumberOfPlayersApprovingQuest |
	| 5               | 3                             |
	| 6               | 4                             |
	| 5               | 4                             |
	| 5               | 5                             |
	| 8               | 5                             |
	| 10              | 6                             |
	| 10              | 8                             |

Scenario Outline: A quest does not go ahead when the majority do not vote for it
	Given there is a standard game of <NumberOfPlayers> players
	And the leader has chosen his crew
	When <NumberOfPlayersApprovingQuest> players approve the quest
	Then the quest does not goes ahead

	Examples: 
	| NumberOfPlayers | NumberOfPlayersApprovingQuest |
	| 5               | 2                             |
	| 6               | 3                             |
	| 5               | 1                             |
	| 5               | 0                             |
	| 8               | 4                             |
	| 10              | 5                             |
	| 10              | 4                             |