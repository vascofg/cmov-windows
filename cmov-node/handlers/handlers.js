exports.testHandler = function (request, reply) {
    reply("Cenas fixes").code(200);
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
