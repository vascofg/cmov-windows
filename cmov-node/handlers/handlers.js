var http = require('http');
var request = require("request");
var ticksArray = ['GOOG'];
var subModel;

//new CronJob(, function() {
//    console.log('You will see this message every second');
//}, null, true, 'America/Los_Angeles');

exports.saveSubModel = function (model) {
    subModel = model;
};

var cron = require('cron');
//every second '* * * * * *'
//every minute '0 * * * * *'
//'*/5 * * * * *' - runs every 5 seconds
var cronJob = cron.job('*/10 * * * * *', function(){

    var url = "http://finance.yahoo.com/d/quotes?f=sl1d1t1v&s=";
    ticksArray.forEach(function (tick) {
       url += url + tick + ",";
    });

    //console.log(url);

    request("http://finance.yahoo.com/d/quotes?f=sl1d1t1v&s=GOOG", function(error, response, body) {
        //console.log(response.statusCode);
        if (response.statusCode == 200) {
            var stockArray = body.split('\n');
            stockArray.forEach(function (stock) {
               //console.log(stock);
                var currentStockArray = stock.split(',');
                var re = new RegExp('"', 'g');
                var tickName = currentStockArray[0].replace(re, '');
                var stockValue = currentStockArray[1];
                if (subModel) {
                    if (tickName != "") {
                        subModel.findAllSubscriptionsForTick(subModel, tickName).then(function (subArray) {
                            if (subArray) {
                                //console.log(subArray);
                                subArray.forEach(function(sub) {
                                    if (sub.max < stockValue || sub.min > stockValue) {
                                        console.log('manda notif');
                                    } else {
                                        console.log('tou quietinho');
                                    }
                                })
                            }
                        });
                    }
                }
            });
        }
    });

    console.info('cron job completed');
});
cronJob.start();

exports.testHandler = function (request, reply) {

    var subModel = request.server.plugins['hapi-sequelized'].db.sequelize.models.Subscription;

    subModel.findAllSubscriptionsForTick(subModel, "GOOG").then(function (sub) {
        console.log(ticksArray);
        reply(sub[0].tick).code(200);
    }) ;
    //
    //reply("Cenas fixes").code(200);
};

exports.addSubHandler = function (request, reply) {
    var subModel = request.server.plugins['hapi-sequelized'].db.sequelize.models.Subscription;

    var tick = request.payload.tick;
    var max = request.payload.max;
    var min = request.payload.min;
    var wns = request.payload.wns;

    if (tick && max && min && wns) {
        subModel.addSub(subModel, tick, max, min, wns).then(function (sub) {
            //console.log(sub);
            if (sub) {
                if (ticksArray.indexOf(tick) < 0) {
                    ticksArray.push(tick);
                }
                reply().code(200);
            }
            else
                reply().code(500);
        })
            .catch(function (error) {
                console.log(error);
                reply().code(500);
            })
    }
};

//exports.handlerTest = function (request, reply) {
//    var userModel = request.server.plugins['hapi-sequelized'].db.sequelize.models.User;
//
//    userModel.findAll({}).then(function (users) {
//        users.forEach(function (user) {
//            console.log(user.email);
//            console.log(user.token);
//        })
//    })
//};
