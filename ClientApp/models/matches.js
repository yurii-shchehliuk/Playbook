const mongoose = require('mongoose');
const Schema = mongoose.Schema;

const Incidents = new Schema({ any: [] });
const Summary = new Schema({ any: [] });

const MatchesSchema = new Schema ({
    Title: { type: String },
    Result: { type: String },
    Date: { type: String },
    league: {
        type: mongoose.Schema.Types.ObjectId, 
        ref: 'League',
    },
    teamHome: {
        type: mongoose.Schema.Types.ObjectId, 
        ref: 'Team',
    },
    teamAway: {

    },
    THomeId: { 
        
    },
    TGuestId: { type: String },
    
    Incidents: [Incidents],
    Summary: [Summary],
    
});

module.exports = mongoose.model('Match', MatchesSchema);