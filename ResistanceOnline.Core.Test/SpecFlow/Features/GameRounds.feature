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

Scenario Outline: When everyone decines the quest then the quest does not go ahead
	Given there is a standard game of 5 players
	And the game is in the <RoundNumber> round
	And the leader has chosen his crew
	When everyone rejects the quest
	Then the quest does not go ahead

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
	Then the quest does not go ahead

	Examples: 
	| NumberOfPlayers | NumberOfPlayersApprovingQuest |
	| 5               | 2                             |
	| 6               | 3                             |
	| 5               | 1                             |
	| 5               | 0                             |
	| 8               | 4                             |
	| 10              | 5                             |
	| 10              | 4                             |

Scenario: Three failed quests out of three results in evil winning
	Given there is a standard game of 5 players
	When the first quest is sabotaged
	And the second quest is sabotaged
	And the third quest is sabotaged
	Then evil wins the game

Scenario Outline: Three failed quests out of four results in evil winning
	Given there is a standard game of 5 players
	When the first quest is <FirstQuestOutcome>
	And the second quest is <SecondQuestOutcome>
	And the third quest is <ThirdQuestOutcome>
	And the fourth quest is <FourthQuestOutcome>
	Then evil wins the game

	Examples: 
	| FirstQuestOutcome | SecondQuestOutcome | ThirdQuestOutcome | FourthQuestOutcome |
	| successful        | sabotaged          | sabotaged         | sabotaged          |
	| sabotaged         | successful         | sabotaged         | sabotaged          |
	| sabotaged         | sabotaged          | successful        | sabotaged          |

Scenario Outline: Three failed quests out of five resules in evil winning
	Given there is a standard game of 5 players
	When the first quest is <FirstQuestOutcome>
	And the second quest is <SecondQuestOutcome>
	And the third quest is <ThirdQuestOutcome>
	And the fourth quest is <FourthQuestOutcome>
	And the fifth quest is <FifthQuestOutcome>
	Then evil wins the game

	Examples:
	| FirstQuestOutcome | SecondQuestOutcome | ThirdQuestOutcome | FourthQuestOutcome | FifthQuestOutcome |
	| successful        | successful         | sabotaged         | sabotaged          | sabotaged         |
	| successful        | sabotaged          | successful        | sabotaged          | sabotaged         |
	| successful        | sabotaged          | sabotaged         | successful         | sabotaged         |
	| sabotaged         | successful         | successful        | sabotaged          | sabotaged         |
	| sabotaged         | successful         | sabotaged         | successful         | sabotaged         |
	| sabotaged         | sabotaged          | successful        | successful         | sabotaged         |

Scenario: Three successful quests out of three results in good winning
	Given there is a standard game of 5 players
	And merlin is not a character
	When the first quest is successful
	And the second quest is successful
	And the third quest is successful
	Then good wins the game

Scenario Outline: Three successful quests out of four results in good winning
	Given there is a standard game of 5 players
	And merlin is not a character
	When the first quest is <FirstQuestOutcome>
	And the second quest is <SecondQuestOutcome>
	And the third quest is <ThirdQuestOutcome>
	And the fourth quest is <FourthQuestOutcome>
	Then good wins the game

	Examples: 
	| FirstQuestOutcome | SecondQuestOutcome | ThirdQuestOutcome | FourthQuestOutcome |
	| sabotaged         | successful         | successful        | successful         |
	| successful        | sabotaged          | successful        | successful         |
	| successful        | successful         | sabotaged         | successful         |

Scenario Outline: Three successful quests out of five resules in good winning
	Given there is a standard game of 5 players
	And merlin is not a character
	When the first quest is <FirstQuestOutcome>
	And the second quest is <SecondQuestOutcome>
	And the third quest is <ThirdQuestOutcome>
	And the fourth quest is <FourthQuestOutcome>
	And the fifth quest is <FifthQuestOutcome>
	Then good wins the game

	Examples:
	| FirstQuestOutcome | SecondQuestOutcome | ThirdQuestOutcome | FourthQuestOutcome | FifthQuestOutcome |
	| sabotaged         | sabotaged          | successful        | successful         | successful        |
	| sabotaged         | successful         | sabotaged         | successful         | successful        |
	| sabotaged         | successful         | successful        | sabotaged          | successful        |
	| successful        | sabotaged          | sabotaged         | successful         | successful        |
	| successful        | sabotaged          | successful        | sabotaged          | successful        |
	| successful        | successful         | sabotaged         | sabotaged          | successful        |

Scenario: Good succeed to complete quests and merlin is a character results in assasin to pick merlin
	Given there is a standard game of 5 players
	And merlin is a character
	And assasin is a character
	When good have completed three quests
	Then assasin is to pick merlin
	
Scenario: Good succeed to complete quests and merlin is not picked results in good winning
	Given there is a standard game of 5 players
	And merlin is a character
	And assasin is a character
	And good have completed three quests
	When the assasin fails to pick merlin
	Then good wins the game

Scenario: Good succeed to compelete quests and merlin is picked results in evil winning
	Given there is a standard game of 5 players
	And merlin is a character
	And assasin is a character
	And good have completed three quests
	When the assasin successfully picks merlin
	Then merlin dies