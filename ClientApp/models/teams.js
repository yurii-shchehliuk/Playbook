const mongoose = require('mongoose');
const Schema = mongoose.Schema;

const TeamsSchema = new Schema ({
    name: { type: String },
    flashscoreLink: { type: String }
})

module.exports = mongoose.model('Team', MatchesSchema);