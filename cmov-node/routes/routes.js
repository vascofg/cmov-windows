var handlers = require('../handlers/handlers.js');

module.exports = [
    {
        method: 'GET',
        path: '/test',
        handler: function (request, reply) {
            handlers.testHandler(request, reply);
        }
    },
    {
        method: 'POST',
        path: '/addSub',
        handler: function (request, reply) {
            handlers.addSubHandler(request, reply);
        }
    },
    {
        method: 'POST',
        path: '/delSub',
        handler: function (request, reply) {
            handlers.delSubHandler(request, reply);
        }
    }
];