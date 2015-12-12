var Glue = require('glue');

var manifest = {
    server: {},
    connections: [
        {
            port: process.env.PORT || 3000,
            host: '0.0.0.0'
        }
    ],
    plugins: [
        {
            'hapi-sequelize': {
                database: 'dbltcocs5l7m41',
                user: 'kyaiteeygnwzve',
                pass: 'XEk8haSjZ6PbsXc34AkewgGUMw',
                dialect: 'postgres',
                host: 'ec2-54-247-170-228.eu-west-1.compute.amazonaws.com',
                port: 5432,
                //uri: 'mysql://ba47ac6b0083d9:d35e2bbd@eu-cdbr-west-01.cleardb.com/heroku_be3348d54cc3478?reconnect=true',
                //uri: 'postgres://kyaiteeygnwzve:XEk8haSjZ6PbsXc34AkewgGUMw@ec2-54-247-170-228.eu-west-1.compute.amazonaws.com:5432/dbltcocs5l7m41',
                //uri: 'postgres://kyaiteeygnwzve:XEk8haSjZ6PbsXc34AkewgGUMw@ec2-54-247-170-228.eu-west-1.compute.amazonaws.com:5432/dbltcocs5l7m41?ssl=true&sslfactory=org.postgresql.ssl.NonValidatingFactory',
                models: 'models/**/*.js',
                sequelize: {
                    define: {
                        underscoredAll: true
                    },
                    dialectOptions: {
                        ssl: true
                    }
                }

            }
        }
        //{
        //    'bell': {
        //
        //    }
        //}
    ]
};

var options = {
    relativeTo: __dirname
};

Glue.compose(manifest, options, function (err, server) {

    if (err) {
        throw err;
    }

    console.log('syncing');
    var db = server.plugins['hapi-sequelize'].db;
    db.sequelize.sync({force: true}).then(function () {
        console.log('models synced');

        var handler = require('./handlers/handlers.js');
        handler.saveSubModel(server.plugins['hapi-sequelize'].db.sequelize.models.Subscription);
        //var tripModel = server.plugins['hapi-sequelized'].db.sequelize.models.Trip;
        //
        //tripModel.addTrips(tripModel);

        //var ticksArray = ['GOOG'];
        //
        //var getTicks = function (next) {
        //
        //    next(null, ticksArray);
        //};
        //
        //var addTick = function
        //
        //server.method('add', add, {});
    });

    var routes = require('./routes/routes.js');

    server.route(routes);

    //server.timeout = 100000;

    server.start(function () {

        console.log('Hapi days!');
        console.log(server.info.uri);
    });
});