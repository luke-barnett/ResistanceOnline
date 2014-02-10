define(['signalr.hubs', 'knockout', 'knockout.mapping'], function (signalr, ko, map) {
    var games = ko.observableArray();

    var update = function (g) {
<<<<<<< HEAD
        map.fromJS(g, {}, games);
        console.log(g);
=======
        map.fromJS(g, {},  games)
>>>>>>> knockouts
    };

    var gameHub = $.connection.gameHub;
    gameHub.on('update', update);
    $.connection.hub.start();

    return games;
});