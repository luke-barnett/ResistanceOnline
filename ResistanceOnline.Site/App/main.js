requirejs.config({
    paths: {
        'text': '/scripts/text',
        'durandal': '/scripts/durandal',
        'plugins': '/scripts/durandal/plugins',
        'transitions': '/scripts/durandal/transitions',
        'knockout': '/scripts/knockout-3.0.0',
<<<<<<< HEAD
        'knockout.mapping': '/scripts/knockout.mapping-latest',
        'knockout.punches': '/scripts/knockout.punches.min',
=======
>>>>>>> add signalr/durandal
        'jquery': '/scripts/jquery-2.1.0',
        'signalr.core': '/scripts/jquery.signalR-2.0.2.min',
        'signalr.hubs': '/signalr/hubs?'
    },
    shim: {
        "signalr.core": {
            deps: ['jquery'],
            exports: "$.connection"
        },
        "signalr.hubs": {
            deps: ['signalr.core']            
        }
    }
});

define(function (require) {
    var system = require('durandal/system'),
        app = require('durandal/app');

    system.debug(true);

    app.title = 'Resistance Online';

    app.configurePlugins({
        router: true,
        dialog: true
    });

    app.start().then(function () {
        app.setRoot('shell');
    });
});