const leaguesModel = require('../../../models/leagues');

module.exports = {
    /**
     * Controller to List all leagues
     * @param {*} req 
     * @param {*} res 
     */
    async listAllLeagues(req, res) {    
        let query = {};
        if (req.params.country) query['country.code'] = req.params.country;
        const leagues = await leaguesModel.find(query).sort({"country.name": 1});

        res.render('pages/leagues/listAll', {
            title: 'Leagues',
            leagues 
        });
    },

    /**
     * Controller function to show single league page
     * @param {*} req 
     * @param {*} res 
     */
    async showLeaguePage(req, res) {
        let query = {};
        if (req.params.country) query['country.code'] = req.params.country;
        if (req.params.leagueName) query['name'] = req.params.leagueName;
        
        const league = await leaguesModel.findOne(query);
        res.render('pages/leagues/leaguePage', {
            title: 'Leagues',
            league 
        });
    }
}