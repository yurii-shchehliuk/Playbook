const express = require('express');
const router = express.Router();

const matchesController = require('../app/controllers/matchesController');
const leaguesController = require('../app/controllers/leaguesController');

// Home route
router.get('/', (req, res) => {
    res.render('pages/home');
});

// Leagues routes
router.get('/leagues', leaguesController.listAllLeagues);
router.get('/leagues/:country', leaguesController.listAllLeagues);
router.get('/leagues/:country/:leagueName', leaguesController.showLeaguePage);

// Matches routes
router.get('/matches', matchesController.listAllMatches);
router.get('/matches/:id', matchesController.showSingleMatchPage);

module.exports = router;
