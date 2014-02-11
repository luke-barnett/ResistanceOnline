define(['signalr.hubs', 'knockout', 'knockout.mapping'], function (signalr, ko, map) {
    var games = ko.observableArray();

    var update = function (g) {
        map.fromJS(g, {}, games);
    };

    var gameHub = $.connection.gameHub;
    gameHub.on('update', update);
    $.connection.hub.start();

    return games;
});