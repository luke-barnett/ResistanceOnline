define(['signalr.hubs', 'knockout', 'knockout.mapping'], function (signalr, ko, map) {
    var games = ko.observableArray();

    function PlayerInfo(data, game) {
        var self = this;
        map.fromJS(data, {}, this);

        self.showActual = ko.observable(false);
        self.image = ko.computed(function () {
            //todo if game over show all?
            if (self.showActual() || game.GameOver()) {
                return "/Images/" + data.Actual + ".png";
            } else {
                return "/Images/player.png";
            }
        });

        self.toggleShowActual = function () {
            self.showActual(!self.showActual());
        }

        return this;
    }
   
    var update = function (g) {
        var mapping = {
            PlayerInfo: { create: function (options) { return new PlayerInfo(options.data, options.parent); } }
        };
        map.fromJS(g, mapping, games);
        console.log(g);
    };

    var gameHub = $.connection.gameHub;
    gameHub.on('update', update);
    $.connection.hub.start();

    return games;
});