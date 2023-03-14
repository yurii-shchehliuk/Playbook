var pipeline = [
    {
        $addFields: {
            homeScored: { $sum: ["$THome.GoalsPerFirst", "$THome.GoalsPerSecond"] },
            guestScored: { $sum: ["$TGuest.GoalsPerFirst", "$TGuest.GoalsPerSecond"] },
            totalScored: { $sum: ["$THome.GoalsPerFirst", "$THome.GoalsPerSecond", "$TGuest.GoalsPerFirst", "$TGuest.GoalsPerSecond"] },
        },
    },
    {
        $match: {
            homeScored: { $gte: 2 }
        }
    },
    {
        $project: {
            _id: "$THome.Name",
            attacksTotal: { $sum: "$THome.Stats0.Attacks" },
            possesion: { $sum: "$THome.Stats0.BallPossession" },
            goalsPerFirstTotal: { $sum: "$THome.GoalsPerFirst" },
            goalsPerFirstAvg: { $avg: "$THome.GoalsPerFirst" },
            homeScored: 1,
            guestScored: 1,
            totalScored: 1,
        }
    },
    {
        $sort: { attacksTotal: -1 }
    }
]

db.at_bundesliga.aggregate(pipeline)