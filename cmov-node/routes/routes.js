var handlers = require('../handlers/handlers.js');

module.exports = [
    {
        method: 'GET',
        path: '/test',
        handler: function (request, reply) {
            handlers.testHandler(request, reply);
        }
    }
];