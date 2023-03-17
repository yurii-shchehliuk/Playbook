var pipeline = [
  {
    $addFields: {
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
    },
  },
  {
    $match: {
      firstHalf: { $gte: 2 },
      // totalScored: { $gte: 4 },
    },
  },
  {
    // $project: {
    $group: {
      _id: "$THome.Name",
      attacksTotal: {
        $sum: "$THome.Stats1.Attacks",
      },
      attacksAvg: {
        $avg: "$THome.Stats1.Attacks",
      },
      possesionTotal: {
        $sum: "$THome.Stats1.BallPossession",
      },
      possesionAvg: {
        $avg: "$THome.Stats1.BallPossession",
      },
      goalsPerFirstTotal: {
        $sum: "$THome.GoalsPerFirst",
      },
      goalsPerFirstAvg: {
        $avg: "$THome.GoalsPerFirst",
      },
      // firstHalf: 1,
      // secondHalf: 1,
      // totalScored: 1,
    },
  },
  {
    $sort: { attacksTotal: -1 },
  },
]

db.at_bundesliga.aggregate(pipeline)