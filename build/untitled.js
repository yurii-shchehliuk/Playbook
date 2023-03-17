[
  {
    $addFields: {
      lastRoundPlayed: {
        $max: ["$RoundNr"],
      },
      firstHalf: {
        $sum: [
          "$THome.GoalsPerFirst",
          "$TGuest.GoalsPerFirst",
        ],
      },
      secondHalf: {
        $sum: [
          "$THome.GoalsPerSecond",
          "$TGuest.GoalsPerSecond",
        ],
      },
      totalScored: {
        $sum: [
          "$THome.GoalsPerFirst",
          "$THome.GoalsPerSecond",
          "$TGuest.GoalsPerFirst",
          "$TGuest.GoalsPerSecond",
        ],
      },
      attacksTotal: {
        $sum: "$THome.Stats1.Attacks",
      },
    },
  },
  {
    $match: {
      // firstHalf: { $gte: 2 },
      // totalScored: { $gte: 4 },
    },
  },
  {
    // $project: {
    $group: {
      _id: "$THome.Name",
      // firstHalf: 1,
      // secondHalf: 1,
      // totalScored: 1,
      attacksTotal: {
        $sum: "$THome.Stats1.Attacks",
      },
      attacksAvg: {
        $avg: "$THome.Stats1.Attacks",
      },
      dangerousAttacksTotal: {
        $sum: "$THome.Stats1.DangerousAttacks",
      },
      dangerousAttacksAvg: {
        $avg: "$THome.Stats1.DangerousAttacks",
      },
      goalAttemptsTotal: {
        $sum: "$THome.Stats1.GoalAttempts",
      },
      goalAttemptsAvg: {
        $avg: "$THome.Stats1.GoalAttempts",
      },
      shotsOnGoalTotal: {
        $sum: "$THome.Stats1.ShotsOnGoal",
      },
      shotsOnGoalAvg: {
        $avg: "$THome.Stats1.ShotsOnGoal",
      },
      shotsOffGoalTotal: {
        $sum: "$THome.Stats1.ShotsOffGoal",
      },
      shotsOffGoalAvg: {
        $avg: "$THome.Stats1.ShotsOffGoal",
      },
      blockedShotsTotal: {
        $sum: "$THome.Stats1.BlockedShots",
      },
      blockedShotsAvg: {
        $avg: "$THome.Stats1.BlockedShots",
      },
      freeKicksTotal: {
        $sum: "$THome.Stats1.FreeKicks",
      },
      freeKicksAvg: {
        $avg: "$THome.Stats1.FreeKicks",
      },
      cornerKicksTotal: {
        $sum: "$THome.Stats1.CornerKicks",
      },
      cornerKicksAvg: {
        $avg: "$THome.Stats1.CornerKicks",
      },
      offsidesTotal: {
        $sum: "$THome.Stats1.Offsides",
      },
      offsidesAvg: {
        $avg: "$THome.Stats1.Offsides",
      },
      throwInTotal: {
        $sum: "$THome.Stats1.ThrowIn",
      },
      throwInAvg: {
        $avg: "$THome.Stats1.ThrowIn",
      },
      foulsTotal: {
        $sum: "$THome.Stats1.Fouls",
      },
      foulsAvg: {
        $avg: "$THome.Stats1.Fouls",
      },
      completedPassesTotal: {
        $sum: "$THome.Stats1.CompletedPasses",
      },
      completedPassesAvg: {
        $avg: "$THome.Stats1.CompletedPasses",
      },
      totalPassesTotal: {
        $sum: "$THome.Stats1.TotalPasses",
      },
      totalPassesAvg: {
        $avg: "$THome.Stats1.TotalPasses",
      },
      ballPossessionTotal: {
        $sum: "$THome.Stats1.BallPossession",
      },
      ballPossessionAvg: {
        $avg: "$THome.Stats1.BallPossession",
      },
      yellowCardsTotal: {
        $sum: "$THome.Stats1.YellowCards",
      },
      yellowCardsAvg: {
        $avg: "$THome.Stats1.YellowCards",
      },
      expectedGoalsTotal: {
        $sum: "$THome.Stats1.ExpectedGoals",
      },
      expectedGoalsAvg: {
        $avg: "$THome.Stats1.ExpectedGoals",
      },
      goalsFirstHalfTotal: {
        $sum: "$THome.GoalsPerFirst",
      },
      goalsFirstHalfAvg: {
        $avg: "$THome.GoalsPerFirst",
      },
    },
  },
  {
    $sort: { goalsFirstHalfTotal: -1 },
  },
]