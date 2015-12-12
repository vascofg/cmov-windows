var http = require('http');
var request = require("request");
var ticksArray = ['GOOG'];
var subModel;

var wns = require('wns');

var options = {
    client_id: 'ms-app://s-1-15-2-4287070966-1989927393-467488029-2973939963-185367217-2307472262-677282393',
    client_secret: '1nKHUVIroDF3Hbhuygp6w075ZtOaxpnc'
};

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
var cronJob = cron.job('*/30 * * * * *', function(){

    var url = "http://finance.yahoo.com/d/quotes?f=sl1d1t1v&s=";
    ticksArray.forEach(function (tick) {
       url = url + tick + ",";
    });

    //console.log(url);

    request(url, function(error, response, body) {
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
                                    var channelUrl = sub.wns;
                                    var title = tickName + " stock value is";
                                    wns.sendTileSquareBlock(channelUrl, title, stockValue, options, function (error, result) {
                                        if (error)
                                            console.error(error);
                                        else
                                            console.log(result);
                                    });

                                    if (sub.max < stockValue) {
                                        console.log('manda notif');
                                        wns.sendToastText01(channelUrl,{
                                            text1: 'Stock for ' + tickName + " is above " + sub.max,
                                            text2: 'Current value is' + stockValue
                                        }, options, function (error, result) {
                                            if (error)
                                                console.error(error);
                                            else
                                                console.log(result);
                                        });

                                    } else if (sub.min > stockValue) {
                                        console.log('manda notif');
                                        wns.sendToastText01(channelUrl,{
                                            text1: 'Stock for ' + tickName + " is below " + sub.min,
                                            text2: 'Current value is' + stockValue
                                        }, options, function (error, result) {
                                            if (error)
                                                console.error(error);
                                            else
                                                console.log(result);
                                        });
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

    //var subModel = request.server.plugins['hapi-sequelized'].db.sequelize.models.Subscription;
    //
    //subModel.findAllSubscriptionsForTick(subModel, "GOOG").then(function (sub) {
    //    console.log(ticksArray);
    //    reply(sub[0].tick).code(200);
    //}) ;
    ////
    ////reply("Cenas fixes").code(200);

    var channelUrl = 'https://db3.notify.windows.com/?token=AwYAAAC6IexUGDHdtC0GyTsW6iH53x6OpWOz9IJO3VaD2YkOFeIA37aNwDvP2d2EjEgB12Yixx%2ftX%2bGCbz0rYa%2fu4gsZ6sbgSd1wMQ3vJ2uKeZC1ZXRzZyCC6a%2fJ2UPhgfxmlL4%3d';
    var options = {
        client_id: 'ms-app://s-1-15-2-4287070966-1989927393-467488029-2973939963-185367217-2307472262-677282393',
        client_secret: '1nKHUVIroDF3Hbhuygp6w075ZtOaxpnc'
    };

    //wns.sendTileSquareBlock(channelUrl, 'Yes!', 'It worked!', options, function (error, result) {
    //    if (error)
    //        console.error(error);
    //    else
    //        console.log(result);
    //});

    //wns.sendToastText01(channelUrl,{
    //    text1: 'This is a dog',
    //    text2: 'The dog is nice',
    //}, options, function (error, result) {
    //    if (error)
    //        console.error(error);
    //    else
    //        console.log(result);
    //});

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
