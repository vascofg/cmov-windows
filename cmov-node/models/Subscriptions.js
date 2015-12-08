
module.exports = function (sequelize, DataTypes) {
    var Subscription = sequelize.define(
        'Subscription',
        {
            tick: {
                type: DataTypes.STRING,
                unique: false,
                allowNull: false,
                primaryKey: true
            },
            max: {
                type: DataTypes.FLOAT
            },
            min: {
                type: DataTypes.FLOAT
            },
            wns: {
                type: DataTypes.TEXT,
                unique: false
            }
        },
        {
            classMethods: {
                associate: function (models) {
                },
                findAllSubscriptionsForTick: function (subModel, tick) {
                    return subModel.findAll({
                        where: {
                            tick: tick
                        }
                    });
                },
                addSub: function (subModel, tick, max, min, wns) {
                    return subModel.upsert({
                        tick: tick,
                        max: max,
                        min: min,
                        wns: wns
                    });
                }
            },
            tableName: 'subscription',
            timestamps: false
        }
    );

    return Subscription;
};