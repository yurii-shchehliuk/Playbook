const mongoose = require('mongoose');
const app = require('./config/express');
const logger = require('./config/logger');
const config = require('../config');

/**
 * Open mongoose connection and run server
 */
let server;
mongoose.set('strictQuery', false);
mongoose.connect(config.mongoose.url).then(() => {
    logger.info('Connected to MongoDB');
    server = app.listen(config.vars.port, () => {
        logger.info(`Listening to port ${config.vars.port}, app URL is: http://localhost:${config.vars.port}`);
    })
});