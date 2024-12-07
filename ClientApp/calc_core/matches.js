const mongoose = require('mongoose');

module.exports = {
    async getAllMatchesList() {
        const collection = await mongoose.connection.db.collection('Playbook');
        const res = await collection.find().toArray();

        return res;
    },

    async getMatchById(id) {
        const collection = await mongoose.connection.db.collection('Playbook');
        const res = await collection.findOne({_id: id});
        console.log(res);
        return res;
    }
}