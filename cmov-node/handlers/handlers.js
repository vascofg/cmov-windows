var ticksArray = ['GOOG'];

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
