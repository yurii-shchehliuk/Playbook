const calcCore = require('../../../calc_core');

module.exports = {
    /**
     * Controller Function to list all matches
     * @param {*} req 
     * @param {*} res 
     */
    async listAllMatches(req, res) {
        const matches = await calcCore.matches.getAllMatchesList();

        res.render('pages/matches/listAll', {
            title: 'Matches',
            matches 
        });

    },

    /**
     * Controller function to show one specific match, defined by id in params
     * @param {*} req 
     * @param {*} res 
     */
    async showSingleMatchPage(req, res) {
        const matchId = req.params.id;
        const match = await calcCore.matches.getMatchById(matchId);

        res.render('pages/matches/matchPage', {
            title: match.Title,
            match 
        });
    }
}