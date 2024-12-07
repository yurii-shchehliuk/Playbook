const express = require('express');
const routes = require('./routes');
const path = require('path');
const config = require('../../config');
const pkg = require('../../package.json');

const app = express();

const root = path.normalize(__dirname + '/..')

// parse json request body
app.use(express.json());

// set views path and default layout
app.set('views', root + '/app/views');
app.set('view engine', 'pug');

// Set locals
app.use(function(req, res, next) {
    res.locals.pkg = pkg;
    res.locals.env = config.vars.env;
    next();
});

// Static files middleware
app.use(express.static(root + '/public'));

// Set routes
app.use('/', routes);

/**
 * Expose app
 */
module.exports = app;