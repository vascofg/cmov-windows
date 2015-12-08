
//module.exports = function (sequelize, DataTypes) {
//    var User = sequelize.define(
//        'User',
//        {
//            email: {
//                type: DataTypes.STRING,
//                unique: true,
//                allowNull: false,
//                primaryKey: true
//            },
//            name: {
//                type: DataTypes.STRING
//            },
//            picture: {
//                type: DataTypes.STRING
//            },
//            token: {
//                type: DataTypes.TEXT,
//                unique: true
//            },
//            expireTime: {
//                type: DataTypes.INTEGER
//            },
//            card: {
//                type: DataTypes.STRING
//            },
//            cvv: {
//                type: DataTypes.STRING
//            },
//            cardDate: {
//                type: DataTypes.STRING
//            }
//        },
//        {
//            classMethods: {
//                associate: function (models) {
//                    User.hasMany(models.Ticket);
//                },
//                findAllUsers: function (userModel) {
//                    return userModel.findAll({
//                        where: {
//                            email: "aa@aa.aa"
//                        }
//                    });
//                },
//                addNewUser: function (userModel, email, name, authToken, expire) {
//                    return userModel.upsert({
//                        email: email,
//                        name: name,
//                        token:authToken,
//                        expireTime: expire
//                    });
//
//                    //return userModel.findOrCreate({
//                    //    where: {
//                    //        email: email
//                    //    }, defaults: {
//                    //        name: name,
//                    //        token: authToken,
//                    //        expireTime: expire
//                    //    }
//                    //});
//                },
//                findUserWithToken: function (userModel, token) {
//                    return userModel.findOne({
//                        where: {
//                            token: token
//                        }
//                    });
//                },
//                findUserWithEmail: function (userModel, email) {
//                    return userModel.findOne({
//                        where: {
//                            email: email
//                        }
//                    });
//                },
//                updateCardInfoForUserWithEmail: function (userModel, user, card, cvv, date) {
//                    if (user) {
//                        console.log(typeof user);
//                        return user.update({
//                            card: card,
//                            cvv: cvv,
//                            cardDate: date
//                        });
//                    }
//                }
//            },
//            tableName: 'user',
//            timestamps: false
//        }
//    );
//
//    //User.hasMany(Ticket);
//
//    return User;
//};